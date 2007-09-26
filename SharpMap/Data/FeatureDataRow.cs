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
using System.Data;
#if !CFBuild
using System.Reflection.Emit;
#endif
using SharpMap.Data;
using SharpMap.Geometries;
using System.Reflection;

namespace SharpMap.Data
{
    /// <summary>
    /// Represents a geographic feature, stored as 
    /// a row of data in a <see cref="FeatureDataTable"/>.
    /// </summary>
#if !CFBuild
    [Serializable]
#endif
    public class FeatureDataRow : DataRow, IFeatureDataRecord
    {
        #region Nested types
        private delegate DataColumnCollection GetColumnsDelegate(DataRow row);
        #endregion

        #region Type fields
        private static readonly GetColumnsDelegate _getColumns; 
        #endregion

        #region Static constructor
        static FeatureDataRow()
        {
#if !CFBuild
            DynamicMethod getColumnsMethod = new DynamicMethod("FeatureDataRow_GetColumns",
                                                               MethodAttributes.Static | MethodAttributes.Public,
                                                               CallingConventions.Standard,
                                                               typeof(DataColumnCollection),		// return type
                                                               new Type[] { typeof(DataRow)},		// one parameter of type DataRow
                                                               typeof(DataRow),					// owning type
                                                               false);								// don't skip JIT visibility checks

            ILGenerator il = getColumnsMethod.GetILGenerator();
            FieldInfo columnsField = typeof(DataRow).GetField("_columns", BindingFlags.NonPublic | BindingFlags.Instance);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, columnsField);
            il.Emit(OpCodes.Ret);

            _getColumns = (GetColumnsDelegate)getColumnsMethod.CreateDelegate(typeof(GetColumnsDelegate));
#else   //Converts the dynamic method into a static method of this class
            _getColumns = GetColumnsInvoker;
#endif
        }

#if CFBuild
        static DataColumnCollection GetColumnsInvoker(DataRow row)
        {
            //OpCodes.ldfld  Finds the value of a field in the object whose reference is currently on the evaluation stack.
            FieldInfo columnsField = typeof(DataRow).GetField("_columns",
                                                                BindingFlags.NonPublic | BindingFlags.Instance);
            // The return type is the same as the one associated with the field. The field may be either an instance 
            //field (in which case the object must not be a null reference) or a static field.
            //So _columns should be a DataColumnCollection 
            DataColumnCollection colCollection = (DataColumnCollection)columnsField.GetValue(row);
            return colCollection;
        }
#endif

        #endregion

        #region Instance fields
        // TODO: implement original and proposed geometry to match DataRow RowState model
        private Geometry _originalGeometry;
        private Geometry _currentGeometry;
        private Geometry _proposedGeometry;
        private bool _isGeometryModified = false;
        private BoundingBox _extents = BoundingBox.Empty;
        private bool _isFullyLoaded;
        #endregion

        #region Object constructor
        internal FeatureDataRow(DataRowBuilder rb)
            : base(rb)
        {
        } 
        #endregion

        /// <summary>
        /// Accepts all pending values in the feature and makes them
        /// the current state.
        /// </summary>
        public new void AcceptChanges()
        {
            base.AcceptChanges();
            _isGeometryModified = false;
        }

        /// <summary>
        /// Gets or sets geographic extents for the feature.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if <see cref="Geometry"/> is not null, since Extents
        /// just reflects the Geometry's extents, if set.
        /// </exception>
        public BoundingBox Extents
        {
            get
            {
                if (Geometry != null)
                {
                    return Geometry.GetBoundingBox();
                }
                else
                {
                    return _extents;
                }
            }
            set
            {
                if(Geometry != null)
                {
                    throw new InvalidOperationException("Geometry is not null - cannot set extents.");
                }

                _extents = value;
            }
        }

        /// <summary>
        /// The geometry of the feature.
        /// </summary>
        public Geometry Geometry
        {
            get { return _currentGeometry; }
            set
            {
                if (_currentGeometry == value)
                {
                    return;
                }

                _currentGeometry = value;

                if (RowState != DataRowState.Detached)
                {
                    _isGeometryModified = true;
                    Table.RowGeometryChanged(this);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this feature record
        /// has been fully loaded from the data source.
        /// </summary>
        public bool IsFullyLoaded
        {
            get { return _isFullyLoaded; }
            internal set { _isFullyLoaded = value; }
        }

        /// <summary>
        /// Gets true if the <see cref="Geometry"/> value for the
        /// feature has been modified.
        /// </summary>
        public bool IsGeometryModified
        {
            get { return _isGeometryModified; }
        }

        /// <summary>
        /// Returns true of the geometry is null.
        /// </summary>
        /// <returns></returns>
        public bool IsGeometryNull()
        {
            return Geometry == null;
        }

        /// <summary>
        /// Gets the <see cref="FeatureDataTable"/> for which this
        /// row has schema.
        /// </summary>
        public new FeatureDataTable Table
        {
            get { return base.Table as FeatureDataTable; }
        }

        /// <summary>
        /// Sets the geometry column to null.
        /// </summary>
        public void SetGeometryNull()
        {
            Geometry = null;
        }

        #region IDataRecord Members

        public int FieldCount
        {
            get { return InternalColumns.Count; }
        }

        public bool GetBoolean(int i)
        {
            return Convert.ToBoolean(this[i]);
        }

        public byte GetByte(int i)
        {
            return Convert.ToByte(this[i]);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return Convert.ToChar(this[i]);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            return InternalColumns[i].DataType.ToString();
        }

        public DateTime GetDateTime(int i)
        {
            return Convert.ToDateTime(this[i]);
        }

        public decimal GetDecimal(int i)
        {
            return Convert.ToDecimal(this[i]);
        }

        public double GetDouble(int i)
        {
            return Convert.ToDouble(this[i]);
        }

        public Type GetFieldType(int i)
        {
            return InternalColumns[i].DataType;
        }

        public float GetFloat(int i)
        {
            return Convert.ToSingle(this[i]);
        }

        public Guid GetGuid(int i)
        {
            return (Guid)this[i];
        }

        public short GetInt16(int i)
        {
            return Convert.ToInt16(this[i]);
        }

        public int GetInt32(int i)
        {
            return Convert.ToInt32(this[i]);
        }

        public long GetInt64(int i)
        {
            return Convert.ToInt64(this[i]);
        }

        public string GetName(int i)
        {
            return InternalColumns[i].ColumnName;
        }

        public int GetOrdinal(string name)
        {
            return InternalColumns[name].Ordinal;
        }

        public string GetString(int i)
        {
            return Convert.ToString(this[i]);
        }

        public object GetValue(int i)
        {
            return this[i];
        }

        public int GetValues(object[] values)
        {
            object[] items = ItemArray;
            int elementsCopied = Math.Max(values.Length, items.Length);
            Array.Copy(items, values, elementsCopied);
            return elementsCopied;
        }

        public bool IsDBNull(int i)
        {
            return IsNull(i);
        }

        #endregion

        protected DataColumnCollection InternalColumns
        {
            get { return _getColumns(this); }
        }

        #region IFeatureDataRecord Members

        public virtual object GetOid()
        {
            if (Table != null && HasOid)
            {
                return this[Table.PrimaryKey[0]];
            }

            return null;
        }

        public virtual TOid GetOid<TOid>()
        {
            throw new NotSupportedException(
                "GetOid<TOid> is not supported for weakly typed FeatureDataRow. " +
                "Use FeatureDataRow<TOid> instead.");
        }

        public virtual bool HasOid
        {
            get 
            {
                return Table == null ? false : Table.PrimaryKey.Length == 1;
            }
        }

        public virtual Type OidType
        {
            get { return HasOid ? Table.PrimaryKey[0].DataType : null; }
        }

        #endregion
    }
}