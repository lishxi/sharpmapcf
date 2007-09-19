// Portions copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
// Portions copyright 2006, 2007 - Rory Plaire (codekaizen@gmail.com)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using SharpMap.Converters.WellKnownText;
using SharpMap.CoordinateSystems;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Features;
using SharpMap.Geometries;
using SharpMap.Indexing;
using SharpMap.Indexing.RTree;
using SharpMap.Utilities;
using System.Globalization;

namespace SharpMap.Data.Providers.ShapeFile
{
	/// <summary>
	/// A data provider for the ESRI ShapeFile spatial data format.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The ShapeFile provider is used for accessing ESRI ShapeFiles. 
	/// The ShapeFile should at least contain the [filename].shp 
	/// and the [filename].shx index file. 
	/// If feature-data is to be used a [filename].dbf file should 
	/// also be present.
	/// </para>
	/// <para>
	/// M and Z values in a shapefile are currently ignored by SharpMap.
	/// </para>
	/// </remarks>
	/// <example>
	/// Adding a datasource to a layer:
	/// <code lang="C#">
	/// using SharpMap.Layers;
	/// using SharpMap.Data.Providers.ShapeFile;
	/// // [...]
	/// FeatureLayer myLayer = new FeatureLayer("My layer");
	/// myLayer.DataSource = new ShapeFile(@"C:\data\MyShapeData.shp");
	/// </code>
	/// </example>
	public class ShapeFileProvider : IWritableFeatureLayerProvider<uint>
	{
		#region FilterMethod

		/// <summary>
		/// A delegate to a filter method for feature data.
		/// </summary>
		/// <remarks>
		/// The FilterMethod delegate is used for applying a method that filters data from the dataset.
		/// The method should return 'true' if the feature should be included and false if not.
		/// <para>See the <see cref="FilterDelegate"/> property for more info</para>
		/// </remarks>
		/// <seealso cref="FilterDelegate"/>
		/// <param name="dr"><see cref="FeatureDataRow"/> to test on</param>
		/// <returns>true if this feature should be included, false if it should be filtered</returns>
		public delegate bool FilterMethod(FeatureDataRow dr);

		#endregion

		#region Instance fields
		private FilterMethod _filterDelegate;
		private int? _srid;
		private string _filename;
		private DbaseFile _dbaseFile;
		private FileStream _shapeFileStream;
		private BinaryReader _shapeFileReader;
		private BinaryWriter _shapeFileWriter;
		private readonly bool _hasFileBasedSpatialIndex;
		private bool _isOpen;
		private bool _coordsysReadFromFile = false;
		private ICoordinateSystem _coordinateSystem;
		private bool _disposed = false;
		private DynamicRTree<uint> _tree;
		private readonly ShapeFileHeader _header;
		private readonly ShapeFileIndex _shapeFileIndex;
		private ShapeFileDataReader _currentReader;
		private readonly object _readerSync = new object();
		private readonly bool _hasDbf;
		#endregion

		#region Object construction and disposal

		/// <summary>
		/// Initializes a ShapeFile data provider without a file-based spatial index.
		/// </summary>
		/// <param name="filename">Path to shapefile (.shp file).</param>
		public ShapeFileProvider(string filename)
			: this(filename, false)
		{
		}

		/// <summary>
		/// Initializes a ShapeFile data provider.
		/// </summary>
		/// <remarks>
		/// <para>
		/// If <paramref name="fileBasedIndex"/> is true, the spatial index 
		/// will be read from a local copy. If it doesn't exist,
		/// it will be generated and saved to [filename] + '.sidx'.
		/// </para>
		/// </remarks>
		/// <param name="filename">Path to shapefile (.shp file).</param>
		/// <param name="fileBasedIndex">True to create a file-based spatial index.</param>
		public ShapeFileProvider(string filename, bool fileBasedIndex)
		{
			_filename = filename;

			if (!File.Exists(filename))
			{
				throw new LayerDataLoadException(filename);
			}

			using (BinaryReader reader = new BinaryReader(File.OpenRead(filename)))
			{
				_header = new ShapeFileHeader(reader);
			}

			_shapeFileIndex = new ShapeFileIndex(this);

			_hasFileBasedSpatialIndex = fileBasedIndex;

			_hasDbf = File.Exists(DbfFilename);

			// Initialize DBF
			if (HasDbf)
			{
				_dbaseFile = new DbaseFile(DbfFilename);
			}
		}

		/// <summary>
		/// Finalizes the object
		/// </summary>
		~ShapeFileProvider()
		{
			dispose(false);
		}

		#region Dispose pattern

		/// <summary>
		/// Disposes the object
		/// </summary>
		void IDisposable.Dispose()
		{
			if (IsDisposed)
			{
				return;
			}

			dispose(true);
			IsDisposed = true;
			GC.SuppressFinalize(this);
		}

		private void dispose(bool disposing)
		{
			if (disposing)
			{
				if (_dbaseFile != null)
				{
					_dbaseFile.Close();
					_dbaseFile = null;
				}

				if (_shapeFileReader != null)
				{
					_shapeFileReader.Close();
					_shapeFileReader = null;
				}

				if (_shapeFileWriter != null)
				{
					_shapeFileWriter.Close();
					_shapeFileWriter = null;
				}

				if (_shapeFileStream != null)
				{
					_shapeFileStream.Close();
					_shapeFileStream = null;
				}

				if (_tree != null)
				{
					_tree.Dispose();
					_tree = null;
				}
			}

			_isOpen = false;
		}

		protected internal bool IsDisposed
		{
			get { return _disposed; }
			private set { _disposed = value; }
		}

		#endregion

		#endregion

		#region ToString

		/// <summary>
		/// Provides a string representation of the essential ShapeFile info.
		/// </summary>
		/// <returns>A string with the Name, HasDbf, FeatureCount and Extents values.</returns>
		public override string ToString()
		{
			return String.Format("[ShapeFile] Name: {0}; HasDbf: {1}; Features: {2}; Extents: {3}",
								 ConnectionId, HasDbf, GetFeatureCount(), GetExtents());
		}

		#endregion

		#region Public Methods and Properties (SharpMap ShapeFile API)

		#region Create static methods

		/// <summary>
		/// Creates a new <see cref="ShapeFile"/> instance and .shp and .shx file on disk.
		/// </summary>
		/// <param name="directory">Directory to create the shapefile in.</param>
		/// <param name="layerName">Name of the shapefile.</param>
		/// <param name="type">Type of shape to store in the shapefile.</param>
		/// <returns>A ShapeFile instance.</returns>
		public static ShapeFileProvider Create(string directory, string layerName, ShapeType type)
		{
			return Create(directory, layerName, type, null);
		}

		/// <summary>
		/// Creates a new <see cref="ShapeFile"/> instance and .shp, .shx and, optionally, .dbf file on disk.
		/// </summary>
		/// <remarks>If <paramref name="schema"/> is null, no .dbf file is created.</remarks>
		/// <param name="directory">Directory to create the shapefile in.</param>
		/// <param name="layerName">Name of the shapefile.</param>
		/// <param name="type">Type of shape to store in the shapefile.</param>
		/// <param name="schema">The schema for the attributes DBase file.</param>
		/// <returns>A ShapeFile instance.</returns>
		/// <exception cref="ShapeFileInvalidOperationException">
		/// 
		/// Thrown if <paramref name="type"/> is <see cref="Providers.ShapeFile.ShapeType.Null"/>.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Thrown if <paramref name="directory"/> is not a valid path.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="layerName"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Thrown if <paramref name="layerName"/> has invalid path characters.
		/// </exception>
		public static ShapeFileProvider Create(string directory, string layerName, ShapeType type, FeatureDataTable schema)
		{
			if (type == ShapeType.Null)
			{
				throw new ShapeFileInvalidOperationException("Cannot create a shapefile with a null geometry type");
			}

			if (String.IsNullOrEmpty(directory) || directory.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
			{
				throw new ArgumentException("Parameter must be a valid path", "directory");
			}

			DirectoryInfo directoryInfo = new DirectoryInfo(directory);

			return Create(directoryInfo, layerName, type, schema);
		}

		/// <summary>
		/// Creates a new <see cref="ShapeFile"/> instance and .shp, .shx and, optionally, 
		/// .dbf file on disk.
		/// </summary>
		/// <remarks>If <paramref name="model"/> is null, no .dbf file is created.</remarks>
		/// <param name="directory">Directory to create the shapefile in.</param>
		/// <param name="layerName">Name of the shapefile.</param>
		/// <param name="type">Type of shape to store in the shapefile.</param>
		/// <param name="model">The schema for the attributes DBase file.</param>
		/// <returns>A ShapeFile instance.</returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="layerName"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Thrown if <paramref name="layerName"/> has invalid path characters.
		/// </exception>
		public static ShapeFileProvider Create(DirectoryInfo directory, string layerName,
			ShapeType type, FeatureDataTable model)
		{
			CultureInfo culture = Thread.CurrentThread.CurrentCulture;
			Encoding encoding = Encoding.GetEncoding(culture.TextInfo.OEMCodePage);
			return Create(directory, layerName, type, model, culture, encoding);
		}


		/// <summary>
		/// Creates a new <see cref="ShapeFile"/> instance and .shp, .shx and, optionally, 
		/// .dbf file on disk.
		/// </summary>
		/// <remarks>If <paramref name="model"/> is null, no .dbf file is created.</remarks>
		/// <param name="directory">Directory to create the shapefile in.</param>
		/// <param name="layerName">Name of the shapefile.</param>
		/// <param name="type">Type of shape to store in the shapefile.</param>
		/// <param name="model">The schema for the attributes DBase file.</param>
		/// <param name="culture">
		/// The culture info to use to determine default encoding and attribute formatting.
		/// </param>
		/// <param name="encoding">
		/// The encoding to use if different from the <paramref name="culture"/>'s default encoding.
		/// </param>
		/// <returns>A ShapeFile instance.</returns>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="layerName"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Thrown if <paramref name="layerName"/> has invalid path characters.
		/// </exception>
		public static ShapeFileProvider Create(DirectoryInfo directory, string layerName, ShapeType type,
									   FeatureDataTable model, CultureInfo culture, Encoding encoding)
		{
			if (String.IsNullOrEmpty(layerName))
			{
				throw new ArgumentNullException("layerName");
			}

			if (layerName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
			{
				throw new ArgumentException("Parameter cannot have invalid filename characters", "layerName");
			}

			if (!String.IsNullOrEmpty(Path.GetExtension(layerName)))
			{
				layerName = Path.GetFileNameWithoutExtension(layerName);
			}

			DataTable schemaTable = null;

			if (model != null)
			{
				schemaTable = DbaseSchema.DeriveSchemaTable(model);
			}

			string shapeFile = Path.Combine(directory.FullName, layerName + ".shp");

			using (MemoryStream buffer = new MemoryStream(100))
			using (BinaryWriter writer = new BinaryWriter(buffer))
			{
				writer.Seek(0, SeekOrigin.Begin);
				writer.Write(ByteEncoder.GetBigEndian(ShapeFileConstants.HeaderStartCode));
				writer.Write(new byte[20]);
				writer.Write(ByteEncoder.GetBigEndian(ShapeFileConstants.HeaderSizeBytes / 2));
				writer.Write(ByteEncoder.GetLittleEndian(ShapeFileConstants.VersionCode));
				writer.Write(ByteEncoder.GetLittleEndian((int)type));
				writer.Write(ByteEncoder.GetLittleEndian(0.0));
				writer.Write(ByteEncoder.GetLittleEndian(0.0));
				writer.Write(ByteEncoder.GetLittleEndian(0.0));
				writer.Write(ByteEncoder.GetLittleEndian(0.0));
				writer.Write(new byte[32]); // Z-values and M-values

				byte[] header = buffer.ToArray();

				using (FileStream shape = File.Create(shapeFile))
				{
					shape.Write(header, 0, header.Length);
				}

				using (FileStream index = File.Create(Path.Combine(directory.FullName, layerName + ".shx")))
				{
					index.Write(header, 0, header.Length);
				}
			}

			if (schemaTable != null)
			{
				String filePath = Path.Combine(directory.FullName, layerName + ".dbf");
				DbaseFile file = DbaseFile.CreateDbaseFile(filePath, schemaTable, culture, encoding);
				file.Close();
			}

			return new ShapeFileProvider(shapeFile);
		}
		#endregion

		/// <summary>
		/// Gets the name of the DBase attribute file.
		/// </summary>
		public string DbfFilename
		{
			get
			{
				return Path.Combine(Path.GetDirectoryName(Path.GetFullPath(_filename)),
									Path.GetFileNameWithoutExtension(_filename) + ".dbf");
			}
		}

		/// <summary>
		/// Gets or sets the encoding used for parsing strings from the DBase DBF file.
		/// </summary>
		/// <remarks>
		/// The DBase default encoding is <see cref="System.Text.Encoding.UTF8"/>.
		/// </remarks>
		/// <exception cref="ShapeFileInvalidOperationException">
		/// Thrown if property is read or set and the shapefile is closed. 
		/// Check <see cref="IsOpen"/> before calling.
		/// </exception>
		/// <exception cref="ShapeFileIsInvalidException">
		/// Thrown if set and there is no DBase file with this shapefile.
		/// </exception>
		public Encoding Encoding
		{
			get
			{
				checkOpen();

				if (!HasDbf)
				{
					return Encoding.ASCII;
				}

				//enableReading();
				return _dbaseFile.Encoding;
			}
		}

		/// <summary>
		/// Gets or sets the filename of the shapefile
		/// </summary>
		/// <remarks>If the filename changes, indexes will be rebuilt</remarks>
		/// <exception cref="ShapeFileInvalidOperationException">
		/// Thrown if method is executed and the shapefile is open or 
		/// if set and the specified filename already exists.
		/// Check <see cref="IsOpen"/> before calling.
		/// </exception>
		/// <exception cref="FileNotFoundException">
		/// </exception>
		/// <exception cref="ShapeFileIsInvalidException">
		/// Thrown if set and the shapefile cannot be opened after a rename.
		/// </exception>
		public string Filename
		{
			get { return _filename; }
			set
			{
				if (value != _filename)
				{
					if (IsOpen)
					{
						throw new ShapeFileInvalidOperationException(
							"Cannot change filename while datasource is open.");
					}

					if (File.Exists(value))
					{
						throw new ShapeFileInvalidOperationException(
							String.Format("Can't rename shapefile because a " +
							"file of with the name {0} already exists.", value));
					}

					if (String.Compare(Path.GetExtension(value), ".shp", true) != 0)
					{
						throw new ShapeFileIsInvalidException(
							String.Format("Invalid shapefile filename: {0}.", value));
					}

					string oldName = Path.Combine(Path.GetDirectoryName(_filename), Path.GetFileNameWithoutExtension(_filename));
					string newName = Path.Combine(Path.GetDirectoryName(value), Path.GetFileNameWithoutExtension(value));

					// Below is not a complete list, but should suffice a great number
					// of scenarios
					string[] possibleShapefileFiles = new string[] {
                        ".shp",     // Geometry file
                        ".shx",     // Shape index file
                        ".dbf",     // Attributes file
                        ".sbn",     // ESRI spatial index
                        ".sbx",     // ESRI spatial index
                        ".idx",     // Geocoding index for read-only shapefiles
                        ".ids",     // Geocoding index for read-write shapefiles
                        ".prj",     // WKT projection file
                        ".fbx",     // ArcView spatial index
                        ".fbn",     // ArcView spatial index
                        ".ain",     // ESRI optional attribute index
                        ".aih",     // ESRI optional attribute index
                        ".atx",     // ArcCatalog attribute index
                        ".cpg",     // Code page file
                        ".sidx" };  // SharpMap spatial index 

					try
					{
						foreach (string ext in possibleShapefileFiles)
						{
							string oldFile = oldName + ext;
							string newFile = newName + ext;
							if (File.Exists(oldFile)) File.Copy(oldFile, newFile);
						}
					}
					catch (IOException)
					{
						// Rollback by deleting copied files
						foreach (string ext in possibleShapefileFiles)
						{
							try
							{
								string newFile = newName + ext;
								if (File.Exists(newFile)) File.Delete(newFile);
							}
							catch (IOException)
							{
								// Ignore and continue...
							}
						}
					}

					// Delete only after all are copied
					foreach (string ext in possibleShapefileFiles)
					{
						string oldFile = oldName + ext;
						if (File.Exists(oldFile)) File.Delete(oldFile);
					}

					_filename = value;

					if (_dbaseFile != null)
					{
						_dbaseFile.Close();
						_dbaseFile = null;
					}
				}
			}
		}

		/// <summary>
		/// Filter Delegate Method for limiting the datasource
		/// </summary>
		/// <remarks>
		/// <example>
		/// Using an anonymous method for filtering all features where the NAME column starts with S:
		/// <code lang="C#">
		/// myShapeDataSource.FilterDelegate = new FilterMethod(delegate(FeatureDataRow row) 
		///		{ return (!row["NAME"].ToString().StartsWith("S")); });
		/// </code>
		/// </example>
		/// <example>
		/// Declaring a delegate method for filtering (multi)polygon-features whose area is larger than 5.
		/// <code>
		/// using SharpMap.Geometries;
		/// [...]
		/// myShapeDataSource.FilterDelegate = CountryFilter;
		/// [...]
		/// public static bool CountryFilter(FeatureDataRow row)
		/// {
		///		if(row.Geometry is Polygon)
		///		{
		///			return (row.Geometry as Polygon).Area > 5;
		///		}
		///		if (row.Geometry is MultiPolygon)
		///		{
		///			return (row.Geometry as MultiPolygon).Area > 5;
		///		}
		///		else 
		///		{
		///			return true;
		///		}
		/// }
		/// </code>
		/// </example>
		/// </remarks>
		/// <seealso cref="FilterMethod"/>
		public FilterMethod FilterDelegate
		{
			get { return _filterDelegate; }
			set { _filterDelegate = value; }
		}

		/// <summary>
		/// Gets true if the shapefile has an attributes file, false otherwise.
		/// </summary>
		public bool HasDbf
		{
			get { return _hasDbf; }
		}

		/// <summary>
		/// The name given to the row identifier in a ShapeFileProvider.
		/// </summary>
		public string IdColumnName
		{
			get { return ShapeFileConstants.IdColumnName; }
		}

		/// <summary>
		/// Gets the record index (.shx file) filename for the given shapefile
		/// </summary>
		public string IndexFilename
		{
			get
			{
				return Path.Combine(Path.GetDirectoryName(Path.GetFullPath(_filename)),
									Path.GetFileNameWithoutExtension(_filename) + ".shx");
			}
		}

		public CultureInfo Locale
		{
			get { return _dbaseFile.CultureInfo; }
		}

		/// <summary>
		/// Forces a rebuild of the spatial index. 
		/// If the instance of the ShapeFile provider
		/// uses a file-based index the file is rewritten to disk,
		/// otherwise it is kept only in memory.
		/// </summary>
		/// <exception cref="ShapeFileInvalidOperationException">
		/// Thrown if method is executed and the shapefile is closed. 
		/// Check <see cref="IsOpen"/> before calling.
		/// </exception>
		public void RebuildSpatialIndex()
		{
			checkOpen();
			//enableReading();

			if (_hasFileBasedSpatialIndex)
			{
				if (File.Exists(_filename + ".sidx"))
				{
					File.Delete(_filename + ".sidx");
				}

				_tree = createSpatialIndexFromFile(_filename);
			}
			else
			{
				_tree = createSpatialIndex();
			}
		}

		/// <summary>
		/// Gets the <see cref="Providers.ShapeFile.ShapeType">
		/// shape geometry type</see> in this shapefile.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The property isn't set until the first time the datasource has been opened,
		/// and will throw an exception if this property has been called since initialization. 
		/// </para>
		/// <para>
		/// All the non-<see cref="SharpMap.Data.Providers.ShapeFile.ShapeType.Null"/> 
		/// shapes in a shapefile are required to be of the same shape type.
		/// </para>
		/// </remarks>
		/// <exception cref="ShapeFileInvalidOperationException">
		/// Thrown if property is read and the shapefile is closed. 
		/// Check <see cref="IsOpen"/> before calling.
		/// </exception>
		public ShapeType ShapeType
		{
			get
			{
				checkOpen();
				return _header.ShapeType;
			}
		}

		#region ILayerProvider Members

		/// <summary>
		/// Gets the connection ID of the datasource.
		/// </summary>
		/// <remarks>
		/// The connection ID of a shapefile is its filename.
		/// </remarks>
		public string ConnectionId
		{
			get { return _filename; }
		}

		public ICoordinateTransformation CoordinateTransformation
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		/// <summary>
		/// Computes the extents of the data source.
		/// </summary>
		/// <returns>
		/// A BoundingBox instance describing the extents of the entire data source.
		/// </returns>
		public BoundingBox GetExtents()
		{
			if (_tree != null)
			{
				return _tree.Root.BoundingBox;
			}

			return _header.Envelope;
		}

		/// <summary>
		/// Returns true if the datasource is currently open
		/// </summary>		
		public bool IsOpen
		{
			get { return _isOpen; }
		}

		/// <summary>
		/// Gets or sets the coordinate system of the ShapeFile. 
		/// </summary>
		/// <remarks>
		/// If a shapefile has a corresponding [filename].prj file containing a Well-Known Text 
		/// description of the coordinate system this will automatically be read.
		/// If this is not the case, the coordinate system will default to null.
		/// </remarks>
		/// <exception cref="ShapeFileInvalidOperationException">
		/// Thrown if property is set and the coordinate system is read from file.
		/// </exception>
		public ICoordinateSystem SpatialReference
		{
			get { return _coordinateSystem; }
			set
			{
				//checkOpen();
				if (_coordsysReadFromFile)
				{
					throw new ShapeFileInvalidOperationException(
						"Coordinate system is specified in projection file and is read only");
				}

				_coordinateSystem = value;
			}
		}

		/// <summary>
		/// Gets or sets the spatial reference ID.
		/// </summary>
		public int? Srid
		{
			get { return _srid; }
			set { _srid = value; }
		}

		#region Methods

		/// <summary>
		/// Closes the datasource
		/// </summary>
		public void Close()
		{
			(this as IDisposable).Dispose();
		}

		/// <summary>
		/// Opens the datasource
		/// </summary>
		public void Open()
		{
			Open(false);
		}
		#endregion

		/// <summary>
		/// Opens the shapefile with optional exclusive access for
		/// faster write performance during bulk updates.
		/// </summary>
		/// <param name="exclusive">
		/// True if exclusive access is desired, false otherwise.
		/// </param>
		public void Open(bool exclusive)
		{
			if (!_isOpen)
			{
				try
				{
					//enableReading();
					_shapeFileStream = new FileStream(Filename, FileMode.OpenOrCreate, FileAccess.ReadWrite,
						exclusive ? FileShare.None : FileShare.Read, 4096, FileOptions.None);

					_shapeFileReader = new BinaryReader(_shapeFileStream);
					_shapeFileWriter = new BinaryWriter(_shapeFileStream);

					_isOpen = true;

					// Read projection file
					parseProjection();

					// Load spatial (r-tree) index
					loadSpatialIndex(_hasFileBasedSpatialIndex);

					if (HasDbf)
					{
						_dbaseFile = new DbaseFile(DbfFilename);
						_dbaseFile.Open(exclusive);
					}
				}
				catch (Exception)
				{
					_isOpen = false;
					throw;
				}
			}
		}

		#endregion

		#region IFeatureLayerProvider Members

		/// <summary>
		/// Returns the data associated with all the geometries that are intersected by 'geom'.
		/// Please note that the ShapeFile provider currently doesn't fully support geometryintersection
		/// and thus only BoundingBox/BoundingBox querying are performed. The results are NOT
		/// guaranteed to lie withing 'geom'.
		/// </summary>
		/// <param name="geom"></param>
		/// <param name="ds">FeatureDataSet to fill with data.</param>
		/// <exception cref="ShapeFileInvalidOperationException">
		/// Thrown if method is called and the shapefile is closed. Check <see cref="IsOpen"/> before calling.
		/// </exception>
		public void ExecuteIntersectionQuery(Geometry geom, FeatureDataSet ds)
		{
			checkOpen();
			//enableReading();

			FeatureDataTable<uint> dt = HasDbf
											? _dbaseFile.NewTable
											: FeatureDataTable<uint>.CreateEmpty(ShapeFileConstants.IdColumnName);
			BoundingBox boundingBox = geom.GetBoundingBox();

			//Get candidates by intersecting the spatial index tree
			IEnumerable<uint> oidList = getKeysFromIndexEntries(_tree.Search(boundingBox));

			foreach (uint oid in oidList)
			{
				for (uint i = (uint)dt.Rows.Count - 1; i >= 0; i--)
				{
					FeatureDataRow<uint> fdr = getFeature(oid, dt);

					if (fdr.Geometry != null)
					{
						if (fdr.Geometry.GetBoundingBox().Intersects(boundingBox))
						{
							// TODO: replace above line with this:  if(fdr.Geometry.Intersects(bbox))  when relation model is complete
							if (FilterDelegate == null || FilterDelegate(fdr))
							{
								dt.AddRow(fdr);
							}
						}
					}
				}
			}

			ds.Tables.Add(dt);
		}

		public void ExecuteIntersectionQuery(Geometry geom, FeatureDataTable table)
		{
			throw new NotImplementedException();
		}

		public IFeatureDataReader ExecuteIntersectionQuery(Geometry geom)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns all objects whose BoundingBox intersects <paramref name="bounds"/>.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Please note that this method doesn't guarantee that the geometries returned actually 
		/// intersect <paramref name="bounds"/>, but only that their <see cref="BoundingBox"/> intersects bounds.
		/// </para>
		/// <para>This method is much faster than the QueryFeatures method, because intersection tests
		/// are performed on objects simplifed by their BoundingBox, and using the spatial index.</para>
		/// </remarks>
		/// <param name="bounds"><see cref="BoundingBox"/> which determines the view.</param>
		/// <param name="ds">The <see cref="FeatureDataSet"/> to fill 
		/// with features within the <paramref name="bounds">view</paramref>.</param>
		/// <exception cref="ShapeFileInvalidOperationException">
		/// Thrown if method is called and the shapefile is closed. Check <see cref="IsOpen"/> before calling.
		/// </exception>
		public void ExecuteIntersectionQuery(BoundingBox bounds, FeatureDataSet ds)
		{
			checkOpen();

			FeatureDataTable<uint> dt = getNewTable();

			// TODO: search the dataset for the table and merge results.
			ExecuteIntersectionQuery(bounds, dt);

			ds.Tables.Add(dt);
		}

		public void ExecuteIntersectionQuery(BoundingBox bounds, FeatureDataTable table)
		{
			checkOpen();

			if(!(table is FeatureDataTable<uint>))
			{
				throw new ArgumentException("Parameter 'table' must be of type FeatureDataTable<uint>.");
			}

			FeatureDataTable<uint> keyedTable = table as FeatureDataTable<uint>;

			SetTableSchema(keyedTable);
			
			IEnumerable<uint> objectsInQuery = GetObjectIdsInView(bounds);

			keyedTable.MergeFeatures(getFeatureRecordsFromIds(objectsInQuery, keyedTable));
        }

		public IFeatureDataReader ExecuteIntersectionQuery(BoundingBox box)
		{
			lock (_readerSync)
			{
				if (_currentReader != null)
				{
					throw new ShapeFileInvalidOperationException(
						"Can't open another ShapeFileDataReader on this ShapeFile, since another reader is already active.");
				}

				//enableReading();
				_currentReader = new ShapeFileDataReader(this, box);
				_currentReader.Disposed += readerDisposed;
				return _currentReader;
			}
		}

		/// <summary>
		/// Returns geometries whose bounding box intersects <paramref name="bounds"/>.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Please note that this method doesn't guarantee that the geometries returned actually 
		/// intersect <paramref name="bounds"/>, but only that their bounding box intersects <paramref name="bounds"/>.
		/// </para>
		/// <para>
		/// This method is much faster than the QueryFeatures method, because intersection tests
		/// are performed on objects simplifed by their BoundingBox, and using the spatial index.
		/// </para>
		/// </remarks>
		/// <param name="bounds">
		/// <see cref="BoundingBox"/> which determines the view.
		/// </param>
		/// <returns>
		/// A <see cref="IEnumerable{T}"/> containing the <see cref="Geometry"/> objects
		/// which are at least partially contained within the given <paramref name="bounds"/>.
		/// </returns>
		/// <exception cref="ShapeFileInvalidOperationException">
		/// Thrown if method is called and the 
		/// shapefile is closed. Check <see cref="IsOpen"/> before calling.
		/// </exception>
		public IEnumerable<Geometry> GetGeometriesInView(BoundingBox bounds)
		{
			checkOpen();
			//enableReading();

			foreach (uint oid in GetObjectIdsInView(bounds))
			{
				Geometry g = GetGeometryById(oid);

				if (!ReferenceEquals(g, null))
				{
					yield return g;
				}
			}
		}

		/// <summary>
		/// Returns the total number of features in the datasource (without any filter applied).
		/// </summary>
		/// <returns>
		/// The number of features contained in the shapefile.
		/// </returns>
		public int GetFeatureCount()
		{
			return _shapeFileIndex.Count;
		}

		public DataTable GetSchemaTable()
		{
			//enableReading();
			return _dbaseFile.GetSchemaTable();
		}

		/// <summary>
		/// Sets the schema of the given table to match the schema of the shapefile's attributes.
		/// </summary>
		/// <param name="target">Target table to set the schema of.</param>
		public void SetTableSchema(FeatureDataTable target)
		{
			checkOpen();
			_dbaseFile.SetTableSchema(target, SchemaMergeAction.Add | SchemaMergeAction.Key);
		}

		#endregion

		#region IFeatureLayerProvider<uint> Members

		/// <summary>
		/// Returns geometry Object IDs whose bounding box intersects <paramref name="bounds"/>.
		/// </summary>
		/// <param name="bounds">Bounds which to search for objects in.</param>
		/// <returns>
		/// An enumeration of object ids which have geometries whose 
		/// bounding box is intersected by <paramref name="bounds"/>.
		/// </returns>
		/// <exception cref="ShapeFileInvalidOperationException">
		/// Thrown if method is called and the shapefile is closed. Check <see cref="IsOpen"/> before calling.
		/// </exception>
		public IEnumerable<uint> GetObjectIdsInView(BoundingBox bounds)
		{
			checkOpen();
			//enableReading();

			foreach (uint id in getKeysFromIndexEntries(_tree.Search(bounds)))
			{
				yield return id;
			}
		}

		/// <summary>
		/// Returns the geometry corresponding to the Object ID
		/// </summary>
		/// <param name="oid">Object ID</param>
		/// <returns><see cref="Geometry"/></returns>
		/// <exception cref="ShapeFileInvalidOperationException">
		/// Thrown if method is called and the shapefile is closed. Check <see cref="IsOpen"/> before calling.
		/// </exception>
		public Geometry GetGeometryById(uint oid)
		{
			checkOpen();
			//enableReading();

			if (FilterDelegate != null) //Apply filtering
			{
				FeatureDataRow fdr = GetFeature(oid);

				if (fdr != null)
				{
					return fdr.Geometry;
				}
				else
				{
					return null;
				}
			}
			else
			{
				return readGeometry(oid);
			}
		}

		/// <summary>
		/// Gets a feature row from the datasource with the specified id.
		/// </summary>
		/// <param name="oid">Id of the feautre to return.</param>
		/// <returns>
		/// The feature corresponding to <paramref name="oid" />, or null if no feature is found.
		/// </returns>
		/// <exception cref="ShapeFileInvalidOperationException">
		/// Thrown if method is called and the shapefile is closed. Check <see cref="IsOpen"/> before calling.
		/// </exception>
		public FeatureDataRow<uint> GetFeature(uint oid)
		{
			return getFeature(oid, null);
		}

		/// <summary>
		/// Sets the schema of the given table to match the schema of the shapefile's attributes.
		/// </summary>
		/// <param name="target">Target table to set the schema of.</param>
		public void SetTableSchema(FeatureDataTable<uint> target)
		{
			if (String.CompareOrdinal(target.IdColumn.ColumnName, DbaseSchema.OidColumnName) != 0)
			{
				throw new InvalidOperationException(
					"Object ID column names for this schema and 'target' schema must be identical, including case. " +
					"For case-insensitive or type-only matching, use SetTableSchema(FeatureDataTable, SchemaMergeAction) " +
					"with the SchemaMergeAction.CaseInsensitive option and/or SchemaMergeAction.KeyByType option enabled.");
			}

			SetTableSchema(target, SchemaMergeAction.Add | SchemaMergeAction.Key);
		}

		/// <summary>
		/// Sets the schema of the given table to match the schema of the shapefile's attributes.
		/// </summary>
		/// <param name="target">Target table to set the schema of.</param>
		/// <param name="mergeAction">Action or actions to take when schemas don't match.</param>
		public void SetTableSchema(FeatureDataTable<uint> target, SchemaMergeAction mergeAction)
		{
			checkOpen();
			_dbaseFile.SetTableSchema(target, mergeAction);
		}

		#endregion

		#region IWritableFeatureLayerProvider<uint> Members

		/// <summary>
		/// Adds a feature to the end of a shapefile.
		/// </summary>
		/// <param name="feature">Feature to append.</param>
		/// <exception cref="ShapeFileInvalidOperationException">
		/// Thrown if method is called and the shapefile is closed. Check <see cref="IsOpen"/> before calling.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="feature"/> is null.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// Thrown if <paramref name="feature.Geometry"/> is null.
		/// </exception>
		public void Insert(FeatureDataRow<uint> feature)
		{
			if (feature == null)
			{
				throw new ArgumentNullException("feature");
			}

			if (feature.Geometry == null)
			{
				throw new InvalidOperationException("Cannot insert a feature with a null geometry");
			}

			checkOpen();
			//enableWriting();

			uint id = _shapeFileIndex.GetNextId();
			feature[ShapeFileConstants.IdColumnName] = id;

			_shapeFileIndex.AddFeatureToIndex(feature);

			BoundingBox featureEnvelope = feature.Geometry.GetBoundingBox();

			if (_tree != null)
			{
				_tree.Insert(new RTreeIndexEntry<uint>(id, featureEnvelope));
			}

			int offset = _shapeFileIndex[id].Offset;
			int length = _shapeFileIndex[id].Length;

			_header.FileLengthInWords = _shapeFileIndex.ComputeShapeFileSizeInWords();
			_header.Envelope = BoundingBox.Join(_header.Envelope, featureEnvelope);

			if (HasDbf)
			{
				_dbaseFile.AddRow(feature);
			}

			writeGeometry(feature.Geometry, id, offset, length);
			_header.WriteHeader(_shapeFileWriter);
			_shapeFileIndex.Save();
		}

		/// <summary>
		/// Adds features to the end of a shapefile.
		/// </summary>
		/// <param name="features">Enumeration of features to append.</param>
		/// <exception cref="ShapeFileInvalidOperationException">
		/// Thrown if method is called and the shapefile is closed. Check <see cref="IsOpen"/> before calling.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="features"/> is null.
		/// </exception>
		public void Insert(IEnumerable<FeatureDataRow<uint>> features)
		{
			if (features == null)
			{
				throw new ArgumentNullException("features");
			}

			checkOpen();
			//enableWriting();

			BoundingBox featuresEnvelope = BoundingBox.Empty;

			foreach (FeatureDataRow<uint> feature in features)
			{
				BoundingBox featureEnvelope = feature.Geometry == null
												  ? BoundingBox.Empty
												  : feature.Geometry.GetBoundingBox();

				featuresEnvelope.ExpandToInclude(featureEnvelope);

				uint id = _shapeFileIndex.GetNextId();

				_shapeFileIndex.AddFeatureToIndex(feature);

				if (_tree != null)
				{
					_tree.Insert(new RTreeIndexEntry<uint>(id, featureEnvelope));
				}

				feature[ShapeFileConstants.IdColumnName] = id;

				int offset = _shapeFileIndex[id].Offset;
				int length = _shapeFileIndex[id].Length;

				writeGeometry(feature.Geometry, id, offset, length);

				if (HasDbf)
				{
					_dbaseFile.AddRow(feature);
				}
			}

			_shapeFileIndex.Save();

			_header.Envelope = BoundingBox.Join(_header.Envelope, featuresEnvelope);
			_header.FileLengthInWords = _shapeFileIndex.ComputeShapeFileSizeInWords();
			_header.WriteHeader(_shapeFileWriter);
		}

		/// <summary>
		/// Updates a feature in a shapefile by deleting the previous 
		/// version and inserting the updated version.
		/// </summary>
		/// <param name="feature">Feature to update.</param>
		/// <exception cref="ShapeFileInvalidOperationException">
		/// Thrown if method is called and the shapefile is closed. 
		/// Check <see cref="IsOpen"/> before calling.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="feature"/> is null.
		/// </exception>
		public void Update(FeatureDataRow<uint> feature)
		{
			if (feature == null) throw new ArgumentNullException("feature");

			if (feature.RowState != DataRowState.Modified)
			{
				return;
			}

			checkOpen();
			//enableWriting();

			if (feature.IsGeometryModified)
			{
				Delete(feature);
				Insert(feature);
			}
			else if (HasDbf)
			{
				_dbaseFile.UpdateRow(feature.Id, feature);
			}

			feature.AcceptChanges();
		}

		/// <summary>
		/// Updates a set of features in a shapefile by deleting the previous 
		/// versions and inserting the updated versions.
		/// </summary>
		/// <param name="features">Enumeration of features to update.</param>
		/// <exception cref="ShapeFileInvalidOperationException">
		/// Thrown if method is called and the shapefile is closed. 
		/// Check <see cref="IsOpen"/> before calling.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="features"/> is null.
		/// </exception>
		public void Update(IEnumerable<FeatureDataRow<uint>> features)
		{
			if (features == null) throw new ArgumentNullException("features");

			checkOpen();
			//enableWriting();

			foreach (FeatureDataRow<uint> feature in features)
			{
				if (feature.RowState != DataRowState.Modified)
				{
					continue;
				}

				if (feature.IsGeometryModified)
				{
					Delete(feature);
					Insert(feature);
				}
				else if (HasDbf)
				{
					_dbaseFile.UpdateRow(feature.Id, feature);
				}

				feature.AcceptChanges();
			}
		}

		/// <summary>
		/// Deletes a row from the shapefile by marking it as deleted.
		/// </summary>
		/// <param name="feature">Feature to delete.</param>
		/// <exception cref="ShapeFileInvalidOperationException">
		/// Thrown if method is called and the shapefile is closed. 
		/// Check <see cref="IsOpen"/> before calling.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="feature"/> is null.
		/// </exception>
		public void Delete(FeatureDataRow<uint> feature)
		{
			if (feature == null)
			{
				throw new ArgumentNullException("feature");
			}

			if (!_shapeFileIndex.ContainsKey(feature.Id))
			{
				return;
			}

			checkOpen();
			//enableWriting();

			feature.Geometry = null;

			uint id = feature.Id;
			int length = _shapeFileIndex[id].Length;
			int offset = _shapeFileIndex[id].Offset;
			writeGeometry(null, feature.Id, offset, length);
		}

		/// <summary>
		/// Deletes a set of rows from the shapefile by marking them as deleted.
		/// </summary>
		/// <param name="features">Features to delete.</param>
		/// <exception cref="ShapeFileInvalidOperationException">
		/// Thrown if method is called and the shapefile is closed. 
		/// Check <see cref="IsOpen"/> before calling.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// Thrown if <paramref name="features"/> is null.
		/// </exception>
		public void Delete(IEnumerable<FeatureDataRow<uint>> features)
		{
			if (features == null)
			{
				throw new ArgumentNullException("features");
			}

			checkOpen();
			//enableWriting();

			foreach (FeatureDataRow<uint> feature in features)
			{
				if (!_shapeFileIndex.ContainsKey(feature.Id))
				{
					continue;
				}

				feature.Geometry = null;

				uint id = feature.Id;
				int length = _shapeFileIndex[id].Length;
				int offset = _shapeFileIndex[id].Offset;
				writeGeometry(null, feature.Id, offset, length);
			}
		}

		///// <summary>
		///// Saves features to the shapefile.
		///// </summary>
		///// <param name="table">
		///// A FeatureDataTable containing feature data and geometry.
		///// </param>
		///// <exception cref="ShapeFileInvalidOperationException">
		///// Thrown if method is called and the shapefile is closed. Check <see cref="IsOpen"/> before calling.
		///// </exception>
		//public void Save(FeatureDataTable<uint> table)
		//{
		//    if (table == null)
		//    {
		//        throw new ArgumentNullException("table");
		//    }

		//    checkOpen();
		//    enableWriting();

		//    _shapeFileStream.Position = ShapeFileConstants.HeaderSizeBytes;
		//    foreach (FeatureDataRow row in table.Rows)
		//    {
		//        if (row is FeatureDataRow<uint>)
		//        {
		//            _tree.Insert(new RTreeIndexEntry<uint>((row as FeatureDataRow<uint>).Id, row.Geometry.GetBoundingBox()));
		//        }
		//        else
		//        {
		//            _tree.Insert(new RTreeIndexEntry<uint>(getNextId(), row.Geometry.GetBoundingBox()));
		//        }

		//        writeFeatureRow(row);
		//    }

		//    writeIndex();
		//    writeHeader(_shapeFileWriter);
		//}

		#endregion

		#endregion

		#region IFeatureLayerProvider<uint> Explicit Members

		IEnumerable<uint> IFeatureLayerProvider<uint>.GetObjectIdsInView(BoundingBox boundingBox)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region General helper functions

		internal static int ComputeGeometryLengthInWords(Geometry geometry)
		{
			if (geometry == null)
			{
				throw new NotSupportedException("Writing null shapes not supported in this version.");
			}

			int byteCount;

			if (geometry is Point)
			{
				byteCount = 20; // ShapeType integer + 2 doubles at 8 bytes each
			}
			else if (geometry is MultiPoint)
			{
				byteCount = 4 /* ShapeType Integer */
							+ ShapeFileConstants.BoundingBoxFieldByteLength + 4 /* NumPoints integer */
							+ 16 * (geometry as MultiPoint).Points.Count;
			}
			else if (geometry is LineString)
			{
				byteCount = 4 /* ShapeType Integer */
							+ ShapeFileConstants.BoundingBoxFieldByteLength + 4 + 4
					/* NumPoints and NumParts integers */
							+ 4 /* Parts Array 1 integer long */
							+ 16 * (geometry as LineString).Vertices.Count;
			}
			else if (geometry is MultiLineString)
			{
				int pointCount = 0;

				foreach (LineString line in (geometry as MultiLineString).LineStrings)
				{
					pointCount += line.Vertices.Count;
				}

				byteCount = 4 /* ShapeType Integer */
							+ ShapeFileConstants.BoundingBoxFieldByteLength + 4 + 4
					/* NumPoints and NumParts integers */
							+ 4 * (geometry as MultiLineString).LineStrings.Count /* Parts array of integer indexes */
							+ 16 * pointCount;
			}
			else if (geometry is Polygon)
			{
				int pointCount = (geometry as Polygon).ExteriorRing.Vertices.Count;

				foreach (LinearRing ring in (geometry as Polygon).InteriorRings)
				{
					pointCount += ring.Vertices.Count;
				}

				byteCount = 4 /* ShapeType Integer */
							+ ShapeFileConstants.BoundingBoxFieldByteLength + 4 + 4
					/* NumPoints and NumParts integers */
							+
							4 *
							((geometry as Polygon).InteriorRings.Count + 1
					/* Parts array of rings: count of interior + 1 for exterior ring */)
							+ 16 * pointCount;
			}
			else
			{
				throw new NotSupportedException("Currently unsupported geometry type.");
			}

			return byteCount / 2; // number of 16-bit words
		}

		private void checkOpen()
		{
			if (!IsOpen)
			{
				throw new ShapeFileInvalidOperationException("An attempt was made to access a closed datasource.");
			}
		}

		private static IEnumerable<uint> getKeysFromIndexEntries(IEnumerable<RTreeIndexEntry<uint>> entries)
		{
			foreach (RTreeIndexEntry<uint> entry in entries)
			{
				yield return entry.Value;
			}
		}

		private IEnumerable<IFeatureDataRecord> getFeatureRecordsFromIds(IEnumerable<uint> ids, FeatureDataTable<uint> table)
		{
			foreach (uint id in ids)
			{
				yield return getFeature(id, table);
			}
		}

		private FeatureDataTable<uint> getNewTable()
		{
			if (HasDbf)
			{
				return _dbaseFile.NewTable;
			}
			else
			{
				return FeatureDataTable<uint>.CreateEmpty(ShapeFileConstants.IdColumnName);
			}
		}

		/// <summary>
		/// Gets a row from the DBase attribute file which has the 
		/// specified <paramref name="oid">object id</paramref> created from
		/// <paramref name="table"/>.
		/// </summary>
		/// <param name="oid">Object id to lookup.</param>
		/// <param name="table">DataTable with schema matching the feature to retrieve.</param>
		/// <returns>Row corresponding to the object id.</returns>
		/// <exception cref="ShapeFileInvalidOperationException">
		/// Thrown if method is called and the shapefile is closed. Check <see cref="IsOpen"/> before calling.
		/// </exception>
		private FeatureDataRow<uint> getFeature(uint oid, FeatureDataTable<uint> table)
		{
			checkOpen();
			//enableReading();

			if (table == null)
			{
				if (!HasDbf)
				{
					table = FeatureDataTable<uint>.CreateEmpty(ShapeFileConstants.IdColumnName);
				}
				else
				{
					table = _dbaseFile.NewTable;
				}
			}

			FeatureDataRow<uint> dr = HasDbf ? _dbaseFile.GetAttributes(oid, table) : table.NewRow(oid);
			dr.Geometry = readGeometry(oid);

			if (FilterDelegate == null || FilterDelegate(dr))
			{
				return dr;
			}
			else
			{
				return null;
			}
		}

		//private void enableReading()
		//{
		//    if (_shapeFileReader == null || !_shapeFileStream.CanRead)
		//    {
		//        if (_shapeFileStream != null)
		//        {
		//            _shapeFileStream.Close();
		//        }

		//        if (_exclusiveMode)
		//        {
		//            _shapeFileStream = File.Open(Filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
		//        }
		//        else
		//        {
		//            _shapeFileStream = File.Open(Filename, FileMode.Open, FileAccess.Read, FileShare.Read);
		//        }

		//        _shapeFileReader = new BinaryReader(_shapeFileStream);
		//    }

		//    if (HasDbf)
		//    {
		//        if (_dbaseFile == null)
		//        {
		//            _dbaseFile = new DbaseFile(DbfFilename);
		//        }

		//        if (!_dbaseFile.IsOpen)
		//        {
		//            _dbaseFile.Open();
		//        }
		//    }
		//}

		//private void enableWriting()
		//{
		//    if (_currentReader != null)
		//    {
		//        throw new ShapeFileInvalidOperationException(
		//            "Can't write to shapefile, since a ShapeFileDataReader is actively reading it.");
		//    }

		//    if (_shapeFileWriter == null || !_shapeFileStream.CanWrite)
		//    {
		//        if (_shapeFileStream != null)
		//        {
		//            _shapeFileStream.Close();
		//        }

		//        if (_exclusiveMode)
		//        {
		//            _shapeFileStream = File.Open(Filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
		//        }
		//        else
		//        {
		//            _shapeFileStream = File.Open(Filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
		//        }

		//        _shapeFileWriter = new BinaryWriter(_shapeFileStream);
		//    }

		//    if (HasDbf)
		//    {
		//        if (_dbaseFile == null)
		//        {
		//            _dbaseFile = new DbaseFile(DbfFilename);
		//        }

		//        if (!_dbaseFile.IsOpen)
		//        {
		//            _dbaseFile.Open();
		//        }
		//    }
		//}

		private void readerDisposed(object sender, EventArgs e)
		{
			lock (_readerSync)
			{
				_currentReader = null;
			}
		}

		#endregion

		#region Spatial indexing helper functions

		/// <summary>
		/// Loads a spatial index from a file. If it doesn't exist, one is created and saved
		/// </summary>
		/// <param name="filename"></param>
		/// <returns>QuadTree index</returns>
		private DynamicRTree<uint> createSpatialIndexFromFile(string filename)
		{
			if (File.Exists(filename + ".sidx"))
			{
				try
				{
					using (
						FileStream indexStream =
							new FileStream(filename + ".sidx", FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						return DynamicRTree<uint>.FromStream(indexStream);
					}
				}
				catch (ObsoleteIndexFileFormatException)
				{
					File.Delete(filename + ".sidx");
					return createSpatialIndexFromFile(filename);
				}
			}
			else
			{
				DynamicRTree<uint> tree = createSpatialIndex();

				using (
					FileStream indexStream =
						new FileStream(filename + ".sidx", FileMode.Create, FileAccess.ReadWrite, FileShare.None))
				{
					tree.SaveIndex(indexStream);
				}

				return tree;
			}
		}

		/// <summary>
		/// Generates a spatial index for a specified shape file.
		/// </summary>
		private DynamicRTree<uint> createSpatialIndex()
		{
			// TODO: implement Post-optimization restructure strategy
			IIndexRestructureStrategy restructureStrategy = new NullRestructuringStrategy();
			RestructuringHuristic restructureHeuristic = new RestructuringHuristic(RestructureOpportunity.None, 4.0);
			IEntryInsertStrategy<RTreeIndexEntry<uint>> insertStrategy = new GuttmanQuadraticInsert<uint>();
			INodeSplitStrategy nodeSplitStrategy = new GuttmanQuadraticSplit<uint>();
			DynamicRTreeBalanceHeuristic indexHeuristic = new DynamicRTreeBalanceHeuristic(4, 10, UInt16.MaxValue);
			IdleMonitor idleMonitor = null;

			DynamicRTree<uint> index =
				new SelfOptimizingDynamicSpatialIndex<uint>(restructureStrategy, restructureHeuristic, insertStrategy,
															nodeSplitStrategy, indexHeuristic, idleMonitor);

			for (uint i = 0; i < (uint)GetFeatureCount(); i++)
			{
				Geometry geom = readGeometry(i);

				if (geom == null)
				{
					continue;
				}

				BoundingBox box = geom.GetBoundingBox();

				if (!double.IsNaN(box.Left) && !double.IsNaN(box.Right) && !double.IsNaN(box.Bottom) &&
					!double.IsNaN(box.Top))
				{
					index.Insert(new RTreeIndexEntry<uint>(i, box));
				}
			}

			return index;
		}

		private void loadSpatialIndex(bool loadFromFile)
		{
			loadSpatialIndex(false, loadFromFile);
		}

		private void loadSpatialIndex(bool forceRebuild, bool loadFromFile)
		{
			//Only load the tree if we haven't already loaded it, or if we want to force a rebuild
			if (_tree == null || forceRebuild)
			{
				if (!loadFromFile)
				{
					_tree = createSpatialIndex();
				}
				else
				{
					_tree = createSpatialIndexFromFile(_filename);
				}
			}
		}

		#endregion

		#region Geometry reading helper functions
		/*
        /// <summary>
        /// Reads all boundingboxes of features in the shapefile. 
        /// This is used for spatial indexing.
        /// </summary>
        /// <returns></returns>
        private List<BoundingBox> getAllFeatureBoundingBoxes()
        {
            enableReading();
            List<BoundingBox> boxes = new List<BoundingBox>();

            foreach (KeyValuePair<uint, ShapeFileIndex.IndexEntry> kvp in _shapeFileIndex)
            {
                _shapeFileStream.Seek(kvp.Value.AbsoluteByteOffset + ShapeFileConstants.ShapeRecordHeaderByteLength,
                                      SeekOrigin.Begin);

                if ((ShapeType) ByteEncoder.GetLittleEndian(_shapeFileReader.ReadInt32()) != ShapeType.Null)
                {
                    double xMin = ByteEncoder.GetLittleEndian(_shapeFileReader.ReadDouble());
                    double yMin = ByteEncoder.GetLittleEndian(_shapeFileReader.ReadDouble());
                    double xMax, yMax;

                    if (ShapeType == ShapeType.Point)
                    {
                        xMax = xMin;
                        yMax = yMin;
                    }
                    else
                    {
                        xMax = ByteEncoder.GetLittleEndian(_shapeFileReader.ReadDouble());
                        yMax = ByteEncoder.GetLittleEndian(_shapeFileReader.ReadDouble());
                    }

                    boxes.Add(new BoundingBox(xMin, yMin, yMax, yMax));
                }
            }

            return boxes;
        }
        */

		/// <summary>
		/// Reads and parses the geometry with ID 'oid' from the ShapeFile.
		/// </summary>
		/// <remarks>
		/// <see cref="FilterDelegate">Filtering</see> is not applied to this method.
		/// </remarks>
		/// <param name="oid">Object ID</param>
		/// <returns>
		/// <see cref="SharpMap.Geometries.Geometry"/> instance from the ShapeFile corresponding to <paramref name="oid"/>.
		/// </returns>
		private Geometry readGeometry(uint oid)
		{
			//enableReading();
			_shapeFileReader.BaseStream.Seek(
				_shapeFileIndex[oid].AbsoluteByteOffset + ShapeFileConstants.ShapeRecordHeaderByteLength,
				SeekOrigin.Begin);

			// Shape type is a common value to all geometry
			ShapeType type = (ShapeType)ByteEncoder.GetLittleEndian(_shapeFileReader.ReadInt32());

			// Null geometries encode deleted lines, so object ids remain consistent
			if (type == ShapeType.Null)
			{
				return null;
			}

			Geometry g;

			switch (ShapeType)
			{
				case ShapeType.Point:
					g = readPoint();
					break;
				case ShapeType.PolyLine:
					g = readPolyLine();
					break;
				case ShapeType.Polygon:
					g = readPolygon();
					break;
				case ShapeType.MultiPoint:
					g = readMultiPoint();
					break;
				case ShapeType.PointZ:
					g = readPointZ();
					break;
				case ShapeType.PolyLineZ:
					g = readPolyLineZ();
					break;
				case ShapeType.PolygonZ:
					g = readPolygonZ();
					break;
				case ShapeType.MultiPointZ:
					g = readMultiPointZ();
					break;
				case ShapeType.PointM:
					g = readPointM();
					break;
				case ShapeType.PolyLineM:
					g = readPolyLineM();
					break;
				case ShapeType.PolygonM:
					g = readPolygonM();
					break;
				case ShapeType.MultiPointM:
					g = readMultiPointM();
					break;
				default:
					throw new ShapeFileUnsupportedGeometryException(
						"ShapeFile type " + ShapeType + " not supported");
			}

			g.SpatialReference = SpatialReference;
			return g;
		}

		private Geometry readMultiPointM()
		{
			throw new NotSupportedException("MultiPointM features are not currently supported");
		}

		private Geometry readPolygonM()
		{
			throw new NotSupportedException("PolygonM features are not currently supported");
		}

		private Geometry readMultiPointZ()
		{
			throw new NotSupportedException("MultiPointZ features are not currently supported");
		}

		private Geometry readPolyLineZ()
		{
			throw new NotSupportedException("PolyLineZ features are not currently supported");
		}

		private Geometry readPolyLineM()
		{
			throw new NotSupportedException("PolyLineM features are not currently supported");
		}

		private Geometry readPointM()
		{
			throw new NotSupportedException("PointM features are not currently supported");
		}

		private Geometry readPolygonZ()
		{
			throw new NotSupportedException("PolygonZ features are not currently supported");
		}

		private Geometry readPointZ()
		{
			throw new NotSupportedException("PointZ features are not currently supported");
		}

		private Geometry readPoint()
		{
			Point point = new Point(ByteEncoder.GetLittleEndian(_shapeFileReader.ReadDouble()),
									ByteEncoder.GetLittleEndian(_shapeFileReader.ReadDouble()));

			return point;
		}

		private Geometry readMultiPoint()
		{
			// Skip min/max box
			_shapeFileReader.BaseStream.Seek(ShapeFileConstants.BoundingBoxFieldByteLength, SeekOrigin.Current);

			MultiPoint feature = new MultiPoint();

			// Get the number of points
			int nPoints = ByteEncoder.GetLittleEndian(_shapeFileReader.ReadInt32());

			if (nPoints == 0)
			{
				return null;
			}

			for (int i = 0; i < nPoints; i++)
			{
				feature.Points.Add(new Point(ByteEncoder.GetLittleEndian(_shapeFileReader.ReadDouble()),
											 ByteEncoder.GetLittleEndian(_shapeFileReader.ReadDouble())));
			}

			return feature;
		}

		private void readPolyStructure(out int parts, out int points, out int[] segments)
		{
			// Skip min/max box
			_shapeFileReader.BaseStream.Seek(ShapeFileConstants.BoundingBoxFieldByteLength, SeekOrigin.Current);

			// Get number of parts (segments)
			parts = ByteEncoder.GetLittleEndian(_shapeFileReader.ReadInt32());

			// Get number of points
			points = ByteEncoder.GetLittleEndian(_shapeFileReader.ReadInt32());

			segments = new int[parts + 1];

			// Read in the segment indexes
			for (int b = 0; b < parts; b++)
			{
				segments[b] = ByteEncoder.GetLittleEndian(_shapeFileReader.ReadInt32());
			}

			// Add end point
			segments[parts] = points;
		}

		private Geometry readPolyLine()
		{
			int parts;
			int points;
			int[] segments;
			readPolyStructure(out parts, out points, out segments);

			if (parts == 0)
			{
				throw new ShapeFileIsInvalidException("Polyline found with 0 parts.");
			}

			MultiLineString mline = new MultiLineString();

			for (int lineId = 0; lineId < parts; lineId++)
			{
				LineString line = new LineString();

				for (int i = segments[lineId]; i < segments[lineId + 1]; i++)
				{
					Point p = new Point(ByteEncoder.GetLittleEndian(_shapeFileReader.ReadDouble()),
										ByteEncoder.GetLittleEndian(_shapeFileReader.ReadDouble()));

					line.Vertices.Add(p);
				}

				mline.LineStrings.Add(line);
			}

			if (mline.LineStrings.Count == 1)
			{
				return mline[0];
			}

			return mline;
		}

		private Geometry readPolygon()
		{
			int parts;
			int points;
			int[] segments;
			readPolyStructure(out parts, out points, out segments);

			if (parts == 0)
			{
				throw new ShapeFileIsInvalidException("Polygon found with 0 parts.");
			}

			// First read all the rings
			List<LinearRing> rings = new List<LinearRing>();

			for (int ringId = 0; ringId < parts; ringId++)
			{
				LinearRing ring = new LinearRing();

				for (int i = segments[ringId]; i < segments[ringId + 1]; i++)
				{
					ring.Vertices.Add(new Point(ByteEncoder.GetLittleEndian(_shapeFileReader.ReadDouble()),
												ByteEncoder.GetLittleEndian(_shapeFileReader.ReadDouble())));
				}

				rings.Add(ring);
			}

			bool[] isCounterClockWise = new bool[rings.Count];
			int polygonCount = 0;

			for (int i = 0; i < rings.Count; i++)
			{
				isCounterClockWise[i] = rings[i].IsCcw();

				if (!isCounterClockWise[i])
				{
					polygonCount++;
				}
			}

			//We only have one polygon
			if (polygonCount == 1)
			{
				Polygon poly = new Polygon();
				poly.ExteriorRing = rings[0];

				if (rings.Count > 1)
				{
					for (int i = 1; i < rings.Count; i++)
					{
						poly.InteriorRings.Add(rings[i]);
					}
				}

				return poly;
			}
			else
			{
				MultiPolygon mpoly = new MultiPolygon();
				Polygon poly = new Polygon();
				poly.ExteriorRing = rings[0];

				for (int i = 1; i < rings.Count; i++)
				{
					if (!isCounterClockWise[i])
					{
						mpoly.Polygons.Add(poly);
						poly = new Polygon(rings[i]);
					}
					else
					{
						poly.InteriorRings.Add(rings[i]);
					}
				}

				mpoly.Polygons.Add(poly);
				return mpoly;
			}
		}

		#endregion

		#region File parsing helpers

		/// <summary>
		/// Reads and parses the projection if a projection file exists
		/// </summary>
		private void parseProjection()
		{
			string projfile =
				Path.Combine(Path.GetDirectoryName(Filename), Path.GetFileNameWithoutExtension(Filename) + ".prj");

			if (File.Exists(projfile))
			{
				try
				{
					string wkt = File.ReadAllText(projfile);
					_coordinateSystem = (ICoordinateSystem)CoordinateSystemWktReader.Parse(wkt);
					_coordsysReadFromFile = true;
				}
				catch (ArgumentException ex)
				{
					Trace.TraceWarning("Coordinate system file '" + projfile
									   + "' found, but could not be parsed. WKT parser returned:" + ex.Message);

					throw new ShapeFileIsInvalidException("Invalid .prj file", ex);
				}
			}
		}

		#endregion

		#region File writing helper functions

		//private void writeFeatureRow(FeatureDataRow feature)
		//{
		//    uint recordNumber = addIndexEntry(feature);

		//    if (HasDbf)
		//    {
		//        _dbaseWriter.AddRow(feature);
		//    }

		//    writeGeometry(feature.Geometry, recordNumber, _shapeIndex[recordNumber].Length);
		//}


		private void writeGeometry(Geometry g, uint recordNumber, int recordOffsetInWords, int recordLengthInWords)
		{
			_shapeFileStream.Position = recordOffsetInWords * 2;

			// Record numbers are 1- based in shapefile
			recordNumber += 1;

			_shapeFileWriter.Write(ByteEncoder.GetBigEndian(recordNumber));
			_shapeFileWriter.Write(ByteEncoder.GetBigEndian(recordLengthInWords));

			if (g == null)
			{
				_shapeFileWriter.Write(ByteEncoder.GetLittleEndian((int)ShapeType.Null));
			}

			switch (ShapeType)
			{
				case ShapeType.Point:
					writePoint(g as Point);
					break;
				case ShapeType.PolyLine:
					if (g is LineString)
					{
						writeLineString(g as LineString);
					}
					else if (g is MultiLineString)
					{
						writeMultiLineString(g as MultiLineString);
					}
					break;
				case ShapeType.Polygon:
					writePolygon(g as Polygon);
					break;
				case ShapeType.MultiPoint:
					writeMultiPoint(g as MultiPoint);
					break;
				case ShapeType.PointZ:
				case ShapeType.PolyLineZ:
				case ShapeType.PolygonZ:
				case ShapeType.MultiPointZ:
				case ShapeType.PointM:
				case ShapeType.PolyLineM:
				case ShapeType.PolygonM:
				case ShapeType.MultiPointM:
				case ShapeType.MultiPatch:
				case ShapeType.Null:
				default:
					throw new NotSupportedException(String.Format(
														"Writing geometry type {0} is not supported in the current version.",
														ShapeType));
			}

			_shapeFileWriter.Flush();
		}

		private void writeCoordinate(double x, double y)
		{
			_shapeFileWriter.Write(ByteEncoder.GetLittleEndian(x));
			_shapeFileWriter.Write(ByteEncoder.GetLittleEndian(y));
		}

		private void writePoint(Point point)
		{
			_shapeFileWriter.Write(ByteEncoder.GetLittleEndian((int)ShapeType.Point));
			writeCoordinate(point.X, point.Y);
		}

		private void writeBoundingBox(BoundingBox box)
		{
			_shapeFileWriter.Write(ByteEncoder.GetLittleEndian(box.Left));
			_shapeFileWriter.Write(ByteEncoder.GetLittleEndian(box.Bottom));
			_shapeFileWriter.Write(ByteEncoder.GetLittleEndian(box.Right));
			_shapeFileWriter.Write(ByteEncoder.GetLittleEndian(box.Top));
		}

		private void writeMultiPoint(MultiPoint multiPoint)
		{
			_shapeFileWriter.Write(ByteEncoder.GetLittleEndian((int)ShapeType.MultiPoint));
			writeBoundingBox(multiPoint.GetBoundingBox());
			_shapeFileWriter.Write(ByteEncoder.GetLittleEndian(multiPoint.Points.Count));

			foreach (Point point in multiPoint.Points)
			{
				writeCoordinate(point.X, point.Y);
			}
		}

		private void writePolySegments(BoundingBox bbox, int[] parts, Point[] points)
		{
			writeBoundingBox(bbox);
			_shapeFileWriter.Write(ByteEncoder.GetLittleEndian(parts.Length));
			_shapeFileWriter.Write(ByteEncoder.GetLittleEndian(points.Length));

			foreach (int partIndex in parts)
			{
				_shapeFileWriter.Write(ByteEncoder.GetLittleEndian(partIndex));
			}

			foreach (Point point in points)
			{
				writeCoordinate(point.X, point.Y);
			}
		}

		private void writeLineString(LineString lineString)
		{
			_shapeFileWriter.Write(ByteEncoder.GetLittleEndian((int)ShapeType.PolyLine));
			writePolySegments(lineString.GetBoundingBox(), new int[] { 0 }, lineString.Vertices.ToArray());
		}

		private void writeMultiLineString(MultiLineString multiLineString)
		{
			int[] parts = new int[multiLineString.LineStrings.Count];
			List<Point> allPoints = new List<Point>();

			int currentPartsIndex = 0;
			foreach (LineString line in multiLineString.LineStrings)
			{
				parts[currentPartsIndex++] = allPoints.Count;
				allPoints.AddRange(line.Vertices);
			}

			_shapeFileWriter.Write(ByteEncoder.GetLittleEndian((int)ShapeType.PolyLine));
			writePolySegments(multiLineString.GetBoundingBox(), parts, allPoints.ToArray());
		}

		private void writePolygon(Polygon polygon)
		{
			int[] parts = new int[polygon.NumInteriorRing + 1];
			List<Point> allPoints = new List<Point>();
			int currentPartsIndex = 0;
			parts[currentPartsIndex++] = 0;
			allPoints.AddRange(polygon.ExteriorRing.Vertices);

			foreach (LinearRing ring in polygon.InteriorRings)
			{
				parts[currentPartsIndex++] = allPoints.Count;
				allPoints.AddRange(ring.Vertices);
			}

			_shapeFileWriter.Write(ByteEncoder.GetLittleEndian((int)ShapeType.Polygon));
			writePolySegments(polygon.GetBoundingBox(), parts, allPoints.ToArray());
		}

		#endregion
	}
}