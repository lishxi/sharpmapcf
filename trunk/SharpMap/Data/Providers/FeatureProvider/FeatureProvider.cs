// Copyright 2006, 2007 - Rory Plaire (codekaizen@gmail.com)
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
using SharpMap.CoordinateSystems;
using SharpMap.CoordinateSystems.Transformations;
using SharpMap.Geometries;
using System.Globalization;

namespace SharpMap.Data.Providers.FeatureProvider
{
	public class FeatureProvider : IWritableFeatureLayerProvider<Guid>
	{
        internal readonly static string OidColumnName = "Oid";
		private FeatureDataTable<Guid> _features = new FeatureDataTable<Guid>(OidColumnName);
		private ICoordinateTransformation _transform = null;

		public FeatureProvider(params DataColumn[] columns)
		{
            foreach (DataColumn column in columns)
            {
                string keyColumnName = _features.PrimaryKey[0].ColumnName;
                if (String.Compare(keyColumnName, column.ColumnName) != 0)
                {
                    _features.Columns.Add(column);
                }
            }
		}

		#region IWritableFeatureLayerProvider<Guid> Members

		public void Insert(FeatureDataRow<Guid> feature)
		{
			_features.ImportRow(feature);
		}

		public void Insert(IEnumerable<FeatureDataRow<Guid>> features)
		{
			foreach (FeatureDataRow<Guid> feature in features)
			{
				Insert(feature);
			}
		}

		public void Update(FeatureDataRow<Guid> feature)
		{
			throw new NotImplementedException();
		}

		public void Update(IEnumerable<FeatureDataRow<Guid>> features)
		{
			throw new NotImplementedException();
		}

		public void Delete(FeatureDataRow<Guid> feature)
		{
			throw new NotImplementedException();
		}

		public void Delete(IEnumerable<FeatureDataRow<Guid>> features)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region IFeatureLayerProvider<Guid> Members

		public IEnumerable<Guid> GetObjectIdsInView(BoundingBox boundingBox)
		{
			throw new NotImplementedException();
		}

		public Geometry GetGeometryById(Guid oid)
		{
			throw new NotImplementedException();
		}

		public FeatureDataRow<Guid> GetFeature(Guid oid)
		{
			throw new NotImplementedException();
		}

		public void SetTableSchema(FeatureDataTable<Guid> table)
		{
			throw new NotImplementedException();
        }

        public void SetTableSchema(FeatureDataTable<Guid> table, SchemaMergeAction schemaMergeAction)
        {
            _features.MergeSchema(table, schemaMergeAction);
        }

		#endregion

        #region IFeatureLayerProvider Members

        public IAsyncResult BeginExecuteFeatureQuery(Geometry geometry, FeatureDataSet dataSet, SpatialQueryType queryType, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginExecuteFeatureQuery(Geometry geometry, FeatureDataTable table, SpatialQueryType queryType, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginExecuteIntersectionQuery(BoundingBox bounds, FeatureDataSet dataSet, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginExecuteIntersectionQuery(BoundingBox bounds, FeatureDataSet dataSet, QueryExecutionOptions options, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginExecuteIntersectionQuery(BoundingBox bounds, FeatureDataTable table, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginExecuteIntersectionQuery(BoundingBox bounds, FeatureDataTable table, QueryExecutionOptions options, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        public void EndExecuteFeatureQuery(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Throws an NotSupportedException. 
        /// </summary>
        /// <param name="geometry">The geometry used to query with.</param>
        /// <param name="queryType">Type of spatial query to execute.</param>
        public IFeatureDataReader ExecuteFeatureQuery(Geometry geometry, SpatialQueryType queryType)
	    {
	        throw new NotImplementedException();
	    }

        /// <summary>
        /// Throws an NotSupportedException.
        /// </summary>
        /// <param name="geometry">The geometry used to query with.</param>
        /// <param name="dataSet">FeatureDataTable to fill data into.</param>
        /// <param name="queryType">Type of spatial query to execute.</param>
        public void ExecuteFeatureQuery(Geometry geometry, FeatureDataSet dataSet, SpatialQueryType queryType)
		{
			throw new NotImplementedException();
		}
        
	    /// <summary>
		/// Throws an NotSupportedException.
		/// </summary>
        /// <param name="geometry">The geometry used to query with.</param>
		/// <param name="table">FeatureDataTable to fill data into.</param>
        /// <param name="queryType">Type of spatial query to execute.</param>
        public void ExecuteFeatureQuery(Geometry geometry, FeatureDataTable table, SpatialQueryType queryType)
		{
			throw new NotImplementedException();
		}

        /// <summary>
        /// Gets the geometries intersecting the specified <see cref="SharpMap.Geometries.BoundingBox"/>.
        /// </summary>
        /// <param name="bounds">BoundingBox to intersect with.</param>
        /// <returns>
        /// An enumeration of features within the specified <see cref="SharpMap.Geometries.BoundingBox"/>.
        /// </returns>
        public IEnumerable<Geometry> ExecuteGeometryIntersectionQuery(BoundingBox bounds)
	    {
	        throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves a <see cref="IFeatureDataReader"/> for the features that 
        /// are intersected by <paramref name="bounds"/>.
        /// </summary>
        /// <param name="bounds">BoundingBox to intersect with.</param>
        /// <returns>An IFeatureDataReader to iterate over the results.</returns>
	    public IFeatureDataReader ExecuteIntersectionQuery(BoundingBox bounds)
	    {
            return ExecuteIntersectionQuery(bounds, QueryExecutionOptions.All);
        }

        /// <summary>
        /// Retrieves a <see cref="IFeatureDataReader"/> for the features that 
        /// are intersected by <paramref name="bounds"/>.
        /// </summary>
        /// <param name="bounds">BoundingBox to intersect with.</param>
        /// <param name="options">Options indicating which data to retrieve.</param>
        /// <returns>An IFeatureDataReader to iterate over the results.</returns>
        public IFeatureDataReader ExecuteIntersectionQuery(BoundingBox bounds, QueryExecutionOptions options)
        {
            FeatureDataReader reader = new FeatureDataReader(_features, bounds, options);
            return reader;
        }

        /// <summary>
        /// Retrieves the data associated with all the features that 
        /// are intersected by <paramref name="bounds"/>.
        /// </summary>
        /// <param name="bounds">BoundingBox to intersect with.</param>
        /// <param name="dataSet">FeatureDataSet to fill data into.</param>
        public void ExecuteIntersectionQuery(BoundingBox bounds, FeatureDataSet dataSet)
		{
            ExecuteIntersectionQuery(bounds, dataSet, QueryExecutionOptions.All);
        }

        /// <summary>
        /// Retrieves the data associated with all the features that 
        /// are intersected by <paramref name="bounds"/>.
        /// </summary>
        /// <param name="bounds">BoundingBox to intersect with.</param>
        /// <param name="dataSet">FeatureDataSet to fill data into.</param>
        /// <param name="options">Options indicating which data to retrieve.</param>
        public void ExecuteIntersectionQuery(BoundingBox bounds, FeatureDataSet dataSet, QueryExecutionOptions options)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retrieves the data associated with all the features that 
        /// are intersected by <paramref name="bounds"/>.
        /// </summary>
        /// <param name="bounds">BoundingBox to intersect with.</param>
        /// <param name="table">FeatureDataTable to fill data into.</param>
        public void ExecuteIntersectionQuery(BoundingBox bounds, FeatureDataTable table)
		{
            ExecuteIntersectionQuery(bounds, table, QueryExecutionOptions.All);
        }

        /// <summary>
        /// Retrieves the data associated with all the features that 
        /// are intersected by <paramref name="bounds"/>.
        /// </summary>
        /// <param name="bounds">BoundingBox to intersect with.</param>
        /// <param name="table">FeatureDataTable to fill data into.</param>
        /// <param name="options">Options indicating which data to retrieve.</param>
        public void ExecuteIntersectionQuery(BoundingBox bounds, FeatureDataTable table, QueryExecutionOptions options)
        {
            IFeatureDataReader reader = ExecuteIntersectionQuery(bounds, options);

            table.Load(reader);
        }

        /// <summary>
        /// Returns the number of features in the entire dataset.
        /// </summary>
        /// <returns>Count of the features in the entire dataset.</returns>
	    public int GetFeatureCount()
		{
			return _features.FeatureCount;
		}

        /// <summary>
        /// Returns a <see cref="DataTable"/> with rows describing the columns in the schema
        /// for the configured provider. Provides the same result as 
        /// <see cref="IDataReader.GetSchemaTable"/>.
        /// </summary>
        /// <seealso cref="IDataReader.GetSchemaTable"/>
        /// <returns>A DataTable that describes the column metadata.</returns>
	    public DataTable GetSchemaTable()
	    {
	        throw new NotImplementedException();
	    }

        /// <summary>
        /// Gets the locale of the data as a CultureInfo.
        /// </summary>
	    public CultureInfo Locale
	    {
	        get { throw new NotImplementedException(); }
	    }

        /// <summary>
        /// Configures a <see cref="FeatureDataTable{TOid}"/> with the schema 
        /// present in the IProvider with the given connection.
        /// </summary>
        /// <param name="table">The FeatureDataTable to configure the schema of.</param>
	    public void SetTableSchema(FeatureDataTable table)
		{
			_features.MergeSchema(table);
        }

		#endregion

		#region ILayerProvider Members

		public ICoordinateTransformation CoordinateTransformation
		{
			get { return _transform; }
			set { _transform = value; }
		}

		public ICoordinateSystem SpatialReference
		{
			get { return GeographicCoordinateSystem.WGS84;  }
		}

		public bool IsOpen
		{
			get { return true; }
		}

		public int? Srid
		{
			get { return null; }
			set {  }
		}

		public BoundingBox GetExtents()
		{
			return _features.Extents;
		}

		public string ConnectionId
		{
			get { return String.Empty; }
		}

		public void Open()
		{
			// Do nothing...
		}

		public void Close()
		{
			// Do nothing...
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if(_features != null)
			{
				_features.Dispose();
				_features = null;
			}
		}

		#endregion
    }
}