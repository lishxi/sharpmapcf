// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
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
using System.Data;
using System.Xml;
#if !CFBuild
using System.Runtime.Serialization;
#endif

namespace SharpMap.Data
{
	/// <summary>
	/// Represents an in-memory cache of spatial data. The FeatureDataSet is an extension of System.Data.DataSet
    /// </summary>
#if !CFBuild
	[Serializable]
#endif
    public class FeatureDataSet : DataSet
	{
		/// <summary>
		/// Initializes a new instance of the FeatureDataSet class.
		/// </summary>
		public FeatureDataSet()
		{
			this.InitClass();
			System.ComponentModel.CollectionChangeEventHandler schemaChangedHandler = new System.ComponentModel.CollectionChangeEventHandler(this.SchemaChanged);
			//this.Tables.CollectionChanged += schemaChangedHandler;
			this.Relations.CollectionChanged += schemaChangedHandler;
			this.InitClass();
        }

#if !CFBuild //No Serialization support in CF Serialization may not work in CF, even with the new CF Beta. Lost Functionality.
		/// <summary>
		/// nitializes a new instance of the FeatureDataSet class.
		/// </summary>
		/// <param name="info">serialization info</param>
		/// <param name="context">streaming context</param>
		protected FeatureDataSet(SerializationInfo info, StreamingContext context)
		{
			string strSchema = ((string)(info.GetValue("XmlSchema", typeof(string))));
			if ((strSchema != null))
			{
				DataSet ds = new DataSet();
				ds.ReadXmlSchema(new XmlTextReader(new System.IO.StringReader(strSchema)));
				if ((ds.Tables["FeatureTable"] != null))
				{
					this.Tables.Add(new FeatureDataTable(ds.Tables["FeatureTable"]));
				}
				this.DataSetName = ds.DataSetName;
				this.Prefix = ds.Prefix;
				this.Namespace = ds.Namespace;
				this.Locale = ds.Locale;
				this.CaseSensitive = ds.CaseSensitive;
				this.EnforceConstraints = ds.EnforceConstraints;
				this.Merge(ds, false, System.Data.MissingSchemaAction.Add);
			}
			else
			{
				this.InitClass();
			}
			this.GetSerializationData(info, context);
			System.ComponentModel.CollectionChangeEventHandler schemaChangedHandler = new System.ComponentModel.CollectionChangeEventHandler(this.SchemaChanged);
			//this.Tables.CollectionChanged += schemaChangedHandler;
			this.Relations.CollectionChanged += schemaChangedHandler;
		}
#endif
        private FeatureTableCollection _FeatureTables;

		/// <summary>
		/// Gets the collection of tables contained in the FeatureDataSet
		/// </summary>
		public new FeatureTableCollection Tables
		{
			get
			{
				return _FeatureTables;
			}
		}

		/// <summary>
		/// Copies the structure of the FeatureDataSet, including all FeatureDataTable schemas, relations, and constraints. Does not copy any data. 
		/// </summary>
		/// <returns></returns>
		public new FeatureDataSet Clone()
		{
			FeatureDataSet cln = ((FeatureDataSet)(base.Clone()));
			return cln;
		}

		/// <summary>
		/// Gets a value indicating whether Tables property should be persisted.
		/// </summary>
		/// <returns></returns>
		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		/// <summary>
		/// Gets a value indicating whether Relations property should be persisted.
		/// </summary>
		/// <returns></returns>
		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		protected override void ReadXmlSerializable(XmlReader reader)
		{
			this.Reset();
			DataSet ds = new DataSet();
			ds.ReadXml(reader);
			//if ((ds.Tables["FeatureTable"] != null))
			//{
			//    this.Tables.Add(new FeatureDataTable(ds.Tables["FeatureTable"]));
			//}
			this.DataSetName = ds.DataSetName;
			this.Prefix = ds.Prefix;
			this.Namespace = ds.Namespace;
			this.Locale = ds.Locale;
			this.CaseSensitive = ds.CaseSensitive;
			this.EnforceConstraints = ds.EnforceConstraints;
			this.Merge(ds, false, System.Data.MissingSchemaAction.Add);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		protected override System.Xml.Schema.XmlSchema GetSchemaSerializable()
		{
			System.IO.MemoryStream stream = new System.IO.MemoryStream();
			this.WriteXmlSchema(new XmlTextWriter(stream, null));
			stream.Position = 0;
			return System.Xml.Schema.XmlSchema.Read(new XmlTextReader(stream), null);
		}


		private void InitClass()
		{
			_FeatureTables = new FeatureTableCollection();
			//this.DataSetName = "FeatureDataSet";
			this.Prefix = "";
			this.Namespace = "http://tempuri.org/FeatureDataSet.xsd";
			this.Locale = new System.Globalization.CultureInfo("en-US");
			this.CaseSensitive = false;
			this.EnforceConstraints = true;
		}

		private bool ShouldSerializeFeatureTable()
		{
			return false;
		}

		private void SchemaChanged(object sender, System.ComponentModel.CollectionChangeEventArgs e)
		{
			if ((e.Action == System.ComponentModel.CollectionChangeAction.Remove))
			{
				//this.InitVars();
			}
		}
	}

	/// <summary>
	/// Represents the method that will handle the RowChanging, RowChanged, RowDeleting, and RowDeleted events of a FeatureDataTable. 
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	public delegate void FeatureDataRowChangeEventHandler(object sender, FeatureDataRowChangeEventArgs e);

	/// <summary>
	/// Represents one feature table of in-memory spatial data. 
	/// </summary>
	[System.Diagnostics.DebuggerStepThrough()]
#if !CFBuild
	[Serializable()]
#endif
	public class FeatureDataTable : DataTable, System.Collections.IEnumerable
	{
		/// <summary>
		/// Initializes a new instance of the FeatureDataTable class with no arguments.
		/// </summary>
		public FeatureDataTable() : base()
		{
			this.InitClass();
		}

		/// <summary>
		/// Intitalizes a new instance of the FeatureDataTable class with the specified table name.
		/// </summary>
		/// <param name="table"></param>
		public FeatureDataTable(DataTable table)
			: base(table.TableName)
		{
            if (table.DataSet != null)
            {
                if ((table.CaseSensitive != table.DataSet.CaseSensitive))
                {
                    CaseSensitive = table.CaseSensitive;
                }
                if ((table.Locale.ToString() != table.DataSet.Locale.ToString()))
                {
                    Locale = table.Locale;
                }
                if ((table.Namespace != table.DataSet.Namespace))
                {
                    Namespace = table.Namespace;
                }
            }

			Prefix = table.Prefix;
			MinimumCapacity = table.MinimumCapacity;
			DisplayExpression = table.DisplayExpression;
		}

		/// <summary>
		/// Gets the number of rows in the table
        /// </summary>
#if !CFBuild //No Browsable in CF. Lost functionality?
        [System.ComponentModel.Browsable(false)] 
#endif
        public int Count
		{
			get
			{
				return base.Rows.Count;
			}
		}

		/// <summary>
		/// Gets the feature data row at the specified index
		/// </summary>
		/// <param name="index">row index</param>
		/// <returns>FeatureDataRow</returns>
		public FeatureDataRow this[int index]
		{
			get
			{
				return (FeatureDataRow)base.Rows[index];
			}
		}

		/// <summary>
		/// Occurs after a FeatureDataRow has been changed successfully. 
		/// </summary>
		public event FeatureDataRowChangeEventHandler FeatureDataRowChanged;

		/// <summary>
		/// Occurs when a FeatureDataRow is changing. 
		/// </summary>
		public event FeatureDataRowChangeEventHandler FeatureDataRowChanging;

		/// <summary>
		/// Occurs after a row in the table has been deleted.
		/// </summary>
		public event FeatureDataRowChangeEventHandler FeatureDataRowDeleted;

		/// <summary>
		/// Occurs before a row in the table is about to be deleted.
		/// </summary>
		public event FeatureDataRowChangeEventHandler FeatureDataRowDeleting;

		/// <summary>
		/// Adds a row to the FeatureDataTable
		/// </summary>
		/// <param name="row"></param>
		public void AddRow(FeatureDataRow row)
		{
			base.Rows.Add(row);
		}

		/// <summary>
		/// Returns an enumerator for enumering the rows of the FeatureDataTable
		/// </summary>
		/// <returns></returns>
		public System.Collections.IEnumerator GetEnumerator()
		{
			return base.Rows.GetEnumerator();
		}

		/// <summary>
		/// Clones the structure of the FeatureDataTable, including all FeatureDataTable schemas and constraints. 
		/// </summary>
		/// <returns></returns>
		public new FeatureDataTable Clone()
		{
			FeatureDataTable cln = ((FeatureDataTable)(base.Clone()));
			cln.InitVars();
			return cln;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		protected override DataTable CreateInstance()
		{
			return new FeatureDataTable();
		}

		internal void InitVars()
		{
			//this.columnFeatureGeometry = this.Columns["FeatureGeometry"];
		}

		private void InitClass()
		{
			//this.columnFeatureGeometry = new DataColumn("FeatureGeometry", typeof(SharpMap.Geometries.Geometry), null, System.Data.MappingType.Element);
			//this.Columns.Add(this.columnFeatureGeometry);
		}

		/// <summary>
		/// Creates a new FeatureDataRow with the same schema as the table.
		/// </summary>
		/// <returns></returns>
		public new FeatureDataRow NewRow()
		{
			return (FeatureDataRow)base.NewRow();
		}

		/// <summary>
		/// Creates a new FeatureDataRow with the same schema as the table, based on a datarow builder
		/// </summary>
		/// <param name="builder"></param>
		/// <returns></returns>
		protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
		{
			return new FeatureDataRow(builder);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		protected override System.Type GetRowType()
		{
			return typeof(FeatureDataRow);
		}

		/// <summary>
		/// Raises the FeatureDataRowChanged event. 
		/// </summary>
		/// <param name="e"></param>
		protected override void OnRowChanged(DataRowChangeEventArgs e)
		{
			base.OnRowChanged(e);
			if ((this.FeatureDataRowChanged != null))
			{
				this.FeatureDataRowChanged(this, new FeatureDataRowChangeEventArgs(((FeatureDataRow)(e.Row)), e.Action));
			}
		}

		/// <summary>
		/// Raises the FeatureDataRowChanging event. 
		/// </summary>
		/// <param name="e"></param>
		protected override void OnRowChanging(DataRowChangeEventArgs e)
		{
			base.OnRowChanging(e);
			if ((this.FeatureDataRowChanging != null))
			{
				this.FeatureDataRowChanging(this, new FeatureDataRowChangeEventArgs(((FeatureDataRow)(e.Row)), e.Action));
			}
		}

		/// <summary>
		/// Raises the FeatureDataRowDeleted event
		/// </summary>
		/// <param name="e"></param>
		protected override void OnRowDeleted(DataRowChangeEventArgs e)
		{
			base.OnRowDeleted(e);
			if ((this.FeatureDataRowDeleted != null))
			{
				this.FeatureDataRowDeleted(this, new FeatureDataRowChangeEventArgs(((FeatureDataRow)(e.Row)), e.Action));
			}
		}

		/// <summary>
		/// Raises the FeatureDataRowDeleting event. 
		/// </summary>
		/// <param name="e"></param>
		protected override void OnRowDeleting(DataRowChangeEventArgs e)
		{
			base.OnRowDeleting(e);
			if ((this.FeatureDataRowDeleting != null))
			{
				this.FeatureDataRowDeleting(this, new FeatureDataRowChangeEventArgs(((FeatureDataRow)(e.Row)), e.Action));
			}
		}

        ///// <summary>
        ///// Gets the collection of rows that belong to this table.
        ///// </summary>
        //public new DataRowCollection Rows
        //{
        //    get { throw (new NotSupportedException()); }
        //    set { throw (new NotSupportedException()); }
        //}

		/// <summary>
		/// Removes the row from the table
		/// </summary>
		/// <param name="row">Row to remove</param>
		public void RemoveRow(FeatureDataRow row)
		{
			base.Rows.Remove(row);
		}
	}

	/// <summary>
	/// Represents the collection of tables for the FeatureDataSet.
	/// </summary>
#if !CFBuild
	[Serializable()]
#endif
	public class FeatureTableCollection : System.Collections.Generic.List<FeatureDataTable>
	{
	}

	/// <summary>
	/// Represents a row of data in a FeatureDataTable.
	/// </summary>
	[System.Diagnostics.DebuggerStepThrough()]
#if !CFBuild
	[Serializable()]
#endif
	public class FeatureDataRow : DataRow
	{

		//private FeatureDataTable tableFeatureTable;

		internal FeatureDataRow(DataRowBuilder rb) : base(rb)
		{
		}
		private SharpMap.Geometries.Geometry _Geometry;

		/// <summary>
		/// The geometry of the current feature
		/// </summary>
		public SharpMap.Geometries.Geometry Geometry
		{
			get { return _Geometry; }
			set { _Geometry = value; }
		}

		/// <summary>
		/// Returns true of the geometry is null
		/// </summary>
		/// <returns></returns>
		public bool IsFeatureGeometryNull()
		{
			return this.Geometry == null;
		}

		/// <summary>
		/// Sets the geometry column to null
		/// </summary>
		public void SetFeatureGeometryNull()
		{
			this.Geometry = null;
		}
	}

	/// <summary>
	/// Occurs after a FeatureDataRow has been changed successfully.
	/// </summary>
	[System.Diagnostics.DebuggerStepThrough()]
	public class FeatureDataRowChangeEventArgs : EventArgs
	{

		private FeatureDataRow eventRow;

		private DataRowAction eventAction;

		/// <summary>
		/// Initializes a new instance of the FeatureDataRowChangeEventArgs class.
		/// </summary>
		/// <param name="row"></param>
		/// <param name="action"></param>
		public FeatureDataRowChangeEventArgs(FeatureDataRow row, DataRowAction action)
		{
			this.eventRow = row;
			this.eventAction = action;
		}

		/// <summary>
		/// Gets the row upon which an action has occurred.
		/// </summary>
		public FeatureDataRow Row
		{
			get
			{
				return this.eventRow;
			}
		}

		/// <summary>
		/// Gets the action that has occurred on a FeatureDataRow.
		/// </summary>
		public DataRowAction Action
		{
			get
			{
				return this.eventAction;
			}
		}
	}
}

