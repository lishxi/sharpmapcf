// Copyright 2006 - Morten Nielsen (www.iter.dk)
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
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Data;
using SharpMap.Converters.WellKnownBinary;
using SharpMap.Converters.WellKnownText;
using SharpMap.CoordinateSystems;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Features;
using SharpMap.Geometries;
using System.Globalization;

namespace SharpMap.Data.Providers.GeometryProvider
{
	/// <summary>
	/// Datasource for storing a limited set of geometries.
	/// </summary>
	/// <remarks>
	/// <para>The GeometryProvider doesn�t utilize performance optimizations of spatial indexing,
	/// and thus is primarily meant for rendering a limited set of Geometries.</para>
	/// <para>A common use of the GeometryProvider is for highlighting a set of selected features.</para>
	/// <example>
	/// The following example gets data within a BoundingBox of another datasource and adds it to the map.
	/// <code lang="C#">
	/// List{Geometry} geometries = myMap.Layers[0].DataSource.GetGeometriesInView(myBox);
	/// FeatureLayer laySelected = new FeatureLayer("Selected Features");
	/// laySelected.DataSource = new GeometryProvider(geometries);
	/// laySelected.Style.Outline = new Pen(Color.Magenta, 3f);
	/// laySelected.Style.EnableOutline = true;
	/// myMap.Layers.Add(laySelected);
	/// </code>
	/// </example>
	/// <example>
	/// Adding points of interest to the map. This is useful for vehicle tracking etc.
	/// <code lang="C#">
	/// List{SharpMap.Geometries.Geometry} geometries = new List{SharpMap.Geometries.Geometry}();
	/// //Add two points
	/// geometries.Add(new SharpMap.Geometries.Point(23.345,64.325));
	/// geometries.Add(new SharpMap.Geometries.Point(23.879,64.194));
	/// SharpMap.Layers.FeatureLayer layerVehicles = new SharpMap.Layers.FeatureLayer("Vechicles");
	/// layerVehicles.DataSource = new SharpMap.Data.Providers.GeometryProvider(geometries);
	/// layerVehicles.Style.Symbol = Bitmap.FromFile(@"C:\data\car.gif");
	/// myMap.Layers.Add(layerVehicles);
	/// </code>
	/// </example>
	/// </remarks>
	public class GeometryProvider : IFeatureLayerProvider<uint>
	{
		private ICoordinateTransformation _coordinateTransformation;
		private ICoordinateSystem _coordinateSystem;
		private readonly List<Geometry> _geometries = new List<Geometry>();
		private int? _srid = null;
		private bool _isDisposed;

		#region Object Construction / Disposal

		/// <summary>
		/// Initializes a new instance of the <see cref="GeometryProvider"/>.
		/// </summary>
		/// <param name="geometry">
		/// Geometry to be added to this datasource.
		/// </param>
		public GeometryProvider(Geometry geometry)
		{
			_geometries = new List<Geometry>();
			_geometries.Add(geometry);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GeometryProvider"/>.
		/// </summary>
		/// <param name="geometries">
		/// Set of geometries to add to this datasource.
		/// </param>
		public GeometryProvider(IEnumerable<Geometry> geometries)
		{
			_geometries.AddRange(geometries);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GeometryProvider"/>.
		/// </summary>
		/// <param name="feature">
		/// Feature which has geometry to be used in this datasource.
		/// </param>
		public GeometryProvider(FeatureDataRow feature)
		{
			_geometries = new List<Geometry>();
			_geometries.Add(feature.Geometry);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GeometryProvider"/>.
		/// </summary>
		/// <param name="features">
		/// Features which have geometry to be used in this datasource.
		/// </param>
		public GeometryProvider(IEnumerable<FeatureDataRow> features)
		{
			_geometries = new List<Geometry>();

			foreach (FeatureDataRow row in features)
			{
				_geometries.Add(row.Geometry);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GeometryProvider"/>.
		/// </summary>
		/// <param name="wellKnownBinaryGeometry">
		/// <see cref="SharpMap.Geometries.Geometry"/> as Well-Known Binary 
		/// to add to this datasource.
		/// </param>
		public GeometryProvider(byte[] wellKnownBinaryGeometry)
			: this(GeometryFromWkb.Parse(wellKnownBinaryGeometry))
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GeometryProvider"/>
		/// </summary>
		/// <param name="wellKnownTextGeometry">
		/// <see cref="SharpMap.Geometries.Geometry"/> as Well-Known Text 
		/// to add to this datasource.
		/// </param>
		public GeometryProvider(string wellKnownTextGeometry)
			: this(GeometryFromWkt.Parse(wellKnownTextGeometry))
		{
		}

		#region Dispose Pattern

		~GeometryProvider()
		{
			Dispose(false);
		}

		#region IDisposable Members

		/// <summary>
		/// Disposes the object
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			IsDisposed = true;
			GC.SuppressFinalize(this);
		}

		#endregion

		public bool IsDisposed
		{
			get { return _isDisposed; }
			private set { _isDisposed = value; }
		}

		protected void Dispose(bool disposing)
		{
			if (IsDisposed)
			{
				return;
			}

			if (disposing)
			{
				_geometries.Clear();
			}
		}

		#endregion

		#endregion

		/// <summary>
		/// Gets or sets the geometries this datasource contains.
		/// </summary>
		public IList<Geometry> Geometries
		{
			get { return _geometries; }
			set
			{
				_geometries.Clear();
				_geometries.AddRange(value);
			}
		}

		#region ILayerProvider Members

		/// <summary>
		/// Gets the connection ID of the datasource
		/// </summary>
		/// <remarks>
		/// The ConnectionID is meant for Connection Pooling which doesn't apply to this datasource. Instead
		/// <c>String.Empty</c> is returned.
		/// </remarks>
		public string ConnectionId
		{
			get { return String.Empty; }
		}

		public ICoordinateTransformation CoordinateTransformation
		{
			get { return _coordinateTransformation; }
			set { _coordinateTransformation = value; }
		}

		/// <summary>
		/// Returns true if the datasource is currently open
		/// </summary>
		public bool IsOpen
		{
			get { return true; }
		}

		public ICoordinateSystem SpatialReference
		{
			get { return _coordinateSystem; }
			set { _coordinateSystem = value; }
		}

		/// <summary>
		/// The spatial reference ID.
		/// </summary>
		public int? Srid
		{
			get { return _srid; }
			set { _srid = value; }
		}

		/// <summary>
		/// Closes the datasource
		/// </summary>
		public void Close()
		{
			//Do nothing;
		}

		/// <summary>
		/// Boundingbox of dataset
		/// </summary>
		/// <returns>boundingbox</returns>
		public BoundingBox GetExtents()
		{
			BoundingBox box = BoundingBox.Empty;

			if (_geometries.Count == 0)
			{
				return box;
			}

			foreach (Geometry g in Geometries)
			{
				if (!g.IsEmpty())
				{
					box.ExpandToInclude(g.GetBoundingBox());
				}
			}

			return box;
		}

		/// <summary>
		/// Opens the datasource
		/// </summary>
		public void Open()
		{
			//Do nothing;
		}

		#endregion

        #region IFeatureLayerProvider Members


        public CultureInfo Locale
        {
            get { return CultureInfo.InvariantCulture; }
        }

		/// <summary>
		/// Returns features within the specified bounding box.
		/// </summary>
		/// <param name="box">The bounding box to intersect with.</param>
		/// <returns>An enumeration of all geometries which intersect <paramref name="box"/>.</returns>
		public IEnumerable<Geometry> GetGeometriesInView(BoundingBox box)
		{
			List<Geometry> list = new List<Geometry>();

			foreach (Geometry g in _geometries)
			{
				if (!g.IsEmpty())
				{
					if (g.GetBoundingBox().Intersects(box))
					{
						list.Add(g);
					}
				}
			}

			return list.AsReadOnly();
		}

		public DataTable GetSchemaTable()
		{
			throw new NotSupportedException("Attribute data is not supported by the GeometryProvider.");
		}

		/// <summary>
		/// Throws an NotSupportedException. Geometry intersection is not yet supported.
		/// </summary>
		/// <param name="geometry">The geometry used to intersect with.</param>
		public IFeatureDataReader ExecuteIntersectionQuery(Geometry geometry)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Throws an NotSupportedException. Geometry intersection is not yet supported.
		/// </summary>
		/// <param name="geometry">The geometry used to intersect with.</param>
		/// <param name="table">FeatureDataTable to fill data into.</param>
		public void ExecuteIntersectionQuery(Geometry geometry, FeatureDataTable table)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Throws an NotSupportedException. Geometry intersection is not yet supported.
		/// </summary>
		/// <param name="geometry">The geometry used to intersect with.</param>
		/// <param name="ds">FeatureDataSet to fill data into.</param>
		public void ExecuteIntersectionQuery(Geometry geometry, FeatureDataSet ds)
		{
			throw new NotSupportedException();
		}

		public IFeatureDataReader ExecuteIntersectionQuery(BoundingBox box)
		{
			return new GeometryDataReader(this, box);
		}

		public void ExecuteIntersectionQuery(BoundingBox box, FeatureDataTable table)
		{
			if (table == null) throw new ArgumentNullException("table");

			List<Geometry> intersection = new List<Geometry>();

			foreach (Geometry geometry in Geometries)
			{
				if (box.Intersects(geometry.GetBoundingBox()))
				{
					intersection.Add(geometry);
				}
			}

			foreach (FeatureDataRow row in table)
			{
				if (intersection.Exists(delegate(Geometry match) { return match.Equals(row.Geometry); }))
				{
					intersection.Remove(row.Geometry);
				}
			}

			foreach (Geometry geometry in intersection)
			{
				FeatureDataRow row = table.NewRow();
				row.Geometry = geometry;
				table.AddRow(row);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="box"></param>
		/// <param name="ds">FeatureDataSet to fill data into</param>
		public void ExecuteIntersectionQuery(BoundingBox box, FeatureDataSet ds)
		{
			if (ds == null) throw new ArgumentNullException("ds");

			FeatureDataTable table = ds.Tables[ConnectionId];

			if(table == null)
			{
				table = new FeatureDataTable(ConnectionId);
				ds.Tables.Add(table);
			}

			ExecuteIntersectionQuery(box, table);
		}

		/// <summary>
		/// Retrieves the number of features accessible with this provider.
		/// </summary>
		/// <returns>The number of features this provider can access.</returns>
		public int GetFeatureCount()
		{
			return _geometries.Count;
        }

		#endregion

		#region IFeatureLayerProvider<uint> Members

		/// <summary>
		/// Returns all objects whose boundingbox intersects 'bbox'.
		/// </summary>
		/// <param name="box"></param>
		/// <returns></returns>
		public IEnumerable<uint> GetObjectIdsInView(BoundingBox box)
		{
			for (uint i = 0; i < _geometries.Count; i++)
			{
				if (_geometries[(int)i].GetBoundingBox().Intersects(box))
				{
					yield return i;
				}
			}
		}

		/// <summary>
		/// Throws an NotSupportedException. Attribute data is not supported by this datasource.
		/// </summary>
		/// <param name="oid"></param>
		/// <returns></returns>
		public FeatureDataRow<uint> GetFeature(uint oid)
		{
			throw new NotSupportedException("Attribute data is not supported by the GeometryProvider.");
		}

		/// <summary>
		/// Returns the geometry corresponding to the Object ID
		/// </summary>
		/// <param name="oid">Object ID</param>
		/// <returns>geometry</returns>
		public Geometry GetGeometryById(uint oid)
		{
			return _geometries[(int)oid];
		}

		public void SetTableSchema(FeatureDataTable<uint> table)
		{
            SetTableSchema(table, SchemaMergeAction.Add | SchemaMergeAction.Key);
        }

        public void SetTableSchema(FeatureDataTable<uint> table, SchemaMergeAction schemaMergeAction)
        {
            if (table == null) throw new ArgumentNullException("table");

            table.Columns.Clear();
        }

		#endregion

		#region IFeatureLayerProvider Explicit Members

		void IFeatureLayerProvider.SetTableSchema(FeatureDataTable table)
		{
			table.Clear();
		}

		#endregion
    }
}