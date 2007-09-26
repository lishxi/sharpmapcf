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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;
#if !CFBuild
using System.Reflection.Emit;
#endif
using SharpMap.Data;
using SharpMap.Geometries;
using System.ComponentModel;

namespace SharpMap.Data
{
    /// <summary>
    /// Provides a data-bindable view of feature data stored in a 
    /// <see cref="FeatureDataTable"/>
    /// and can provide sorted, filtered, searchable and editable access to that data.
    /// </summary>
    public class FeatureDataView : DataView, IEnumerable<FeatureDataRow>
    {
        #region Nested types

        private delegate void SetDataViewManagerDelegate(FeatureDataView view, DataViewManager dataViewManager);

        private delegate void SetLockedDelegate(DataView view, bool locked);

        private delegate void SetIndex2Delegate(
            FeatureDataView view, string newSort, DataViewRowState dataViewRowState, object expression, bool fireEvent);

        #endregion

        #region Type fields

        private static readonly SetDataViewManagerDelegate _setDataViewManager;
        private static readonly SetLockedDelegate _setLocked;
        private static readonly SetIndex2Delegate _setIndex2;

        #endregion

        #region Static constructor

        static FeatureDataView()
        {
            // Create method to set DataViewManager
            _setDataViewManager = GenerateSetDataViewManagerDelegate();

            // Create method to set locked status
            _setLocked = GenerateSetLockedDelegate();

            // Create a delegate to the SetIndex2 method
            _setIndex2 = GenerateSetIndex2Delegate();
        }

        #endregion

        #region Instance fields
        private Geometry _queryGeometry;
        private readonly SpatialQueryType _queryType;
        private readonly ArrayList _oidFilter = new ArrayList();
        #endregion

        #region Object constructors

        public FeatureDataView(FeatureDataTable table)
            : this(table, null, null, DataViewRowState.CurrentRows)
        {
        }

        public FeatureDataView(FeatureDataTable table, Geometry intersectionFilter,
                               string sort, DataViewRowState rowState)
            : this(table, intersectionFilter, SpatialQueryType.Intersects, sort, rowState)
        {
        }

        public FeatureDataView(FeatureDataTable table, Geometry query, SpatialQueryType queryType,
                               string sort, DataViewRowState rowState)
            : base(table, "", String.IsNullOrEmpty(sort) ? table.PrimaryKey.Length == 1 ? table.PrimaryKey[0].ColumnName : "" : sort, rowState)
        {
            // TODO: Support all query types in FeatureDataView
            if (queryType != SpatialQueryType.Intersects)
            {
                throw new NotSupportedException(
                    "Only query type of SpatialQueryType.Intersects currently supported.");
            }

            _queryGeometry = query;
            _queryType = queryType;

            // This call rebuilds the index which was just built with 
            // the call into the base constructor, which may be a performance
            // hit. A more robust solution would be to just recreate the 
            // behavior of the base constructor here, so we can create the 
            // underlying index once.
            setFilterPredicate();
        }

        internal FeatureDataView(FeatureDataTable table, bool locked)
            : this(table)
        {
            // The DataView is locked when it is created as a default
            // view for a table.
            _setLocked(this, locked);

            // This call rebuilds the index which was just built with 
            // the call into the base constructor, which may be a performance
            // hit. A more robust solution would be to just recreate the 
            // behavior of the base constructor here, so we can create the 
            // underlying index once.
            setFilterPredicate();
        }

        #endregion

        /// <summary>
        /// Gets or sets the <see cref="Geometry"/> instance used
        /// to filter the table data based on intersection.
        /// </summary>
        public Geometry GeometryFilter
        {
            get { return _queryGeometry == null ? null : _queryGeometry.Clone(); }
            set
            {
                if (_queryGeometry == value)
                {
                    return;
                }

                _queryGeometry = value;

                if (!Table.CachedRegion.Contains(_queryGeometry))
                {
                    onMissingRegion(_queryGeometry.Difference(Table.CachedRegion));
                }

                // TODO: Perhaps resetting the entire index is a bit drastic... 
                // Reconsider how to enlist and retire rows incrementally, 
                // perhaps doing RTree diffs.
                Reset();
            }
        }

        public SpatialQueryType GeometryFilterType
        {
            get { return _queryType; }
        }

        public IEnumerable OidFilter
        {
            get { return _oidFilter; }
            set
            {
                _oidFilter.Clear();

                ArrayList missingOids = new ArrayList();

                if (value != null)
                {
                    foreach (object oid in value)
                    {
                        _oidFilter.Add(oid);
                        FeatureDataRow feature = Table.Find(oid);

                        if (feature == null || !feature.IsFullyLoaded)
                        {
                            missingOids.Add(oid);
                        }
                    }

                    if (missingOids.Count > 0)
                    {
                        onMissingOids(missingOids);
                    }
                }

                Reset();
            }
        }

        /// <summary>
        /// Gets the bounds which this view covers as a <see cref="BoundingBox"/>.
        /// </summary>
        public BoundingBox Extents
        {
            get
            {
                if (_queryGeometry == null)
                {
                    return BoundingBox.Empty;
                }

                return _queryGeometry.GetBoundingBox();
            }
            //set
            //{
            //    if (value == BoundingBox.Empty)
            //    {
            //        GeometryFilter = null;
            //    }

            //    GeometryFilter = value.ToGeometry();
            //}
        }

        #region DataView overrides and shadows
        public override DataRowView AddNew()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the DataViewManager which is managing this view's settings.
        /// </summary>
        public new FeatureDataViewManager DataViewManager
        {
            get { return base.DataViewManager as FeatureDataViewManager; }
        }

        public override string RowFilter
        {
            get
            {
                return "";
            }
            set
            {
                throw new NotSupportedException(
                    "RowFilter expressions not supported at this time.");
            }
        }

        public new FeatureDataTable Table
        {
            get { return base.Table as FeatureDataTable; }
            set { base.Table = value; }
        }

        // TODO: Please don't optimize the following apparently useless overrides out, 
        // I need to figure out what to do with them [ck]

        protected override void ColumnCollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            base.ColumnCollectionChanged(sender, e);
        }

        protected override void IndexListChanged(object sender, ListChangedEventArgs e)
        {
            base.IndexListChanged(sender, e);
        }

        protected override void OnListChanged(ListChangedEventArgs e)
        {
            base.OnListChanged(e);
        }

        protected override void UpdateIndex(bool force)
        {
            base.UpdateIndex(force);
        }

        #endregion

        #region IEnumerable<FeatureDataRow> Members

        IEnumerator<FeatureDataRow> IEnumerable<FeatureDataRow>.GetEnumerator()
        {
            IEnumerator e = GetEnumerator();

            while (e.MoveNext())
            {
                FeatureDataRow feature = (e.Current as DataRowView).Row as FeatureDataRow;
                yield return feature;
            }
        }

        #endregion

        #region DataView internals access methods
        internal void SetDataViewManager(FeatureDataViewManager featureDataViewManager)
        {
            // Call the delegate we wired up to bypass the normally inaccessible 
            // base class method
            _setDataViewManager(this, featureDataViewManager);
        }

        internal void SetIndex2(string newSort, DataViewRowState dataViewRowState,
                                object dataExpression, bool fireEvent)
        {
            // Call the delegate we wired up to bypass the normally inaccessible 
            // base class method
            _setIndex2(this, newSort, dataViewRowState, dataExpression, fireEvent);
        }
        #endregion

        #region Private instance helper methods

        private void setFilterPredicate()
        {
            // TODO: rethink how the view is filtered... perhaps we could bypass SetIndex2 and create
            // the System.Data.Index directly with rows returned from the SharpMap.Indexing.RTree
            object iFilter = createRowPredicateFilter(isRowInView);
            SetIndex2(Sort, RowStateFilter, iFilter, true);
        }

        private bool isRowInView(DataRow row)
        {
            FeatureDataRow feature = row as FeatureDataRow;

            if (feature == null)
            {
                return false;
            }

            return inGeometryFilter(feature) 
                && inOidFilter(feature)
                && inAttributeFilter();
        }

        private bool inGeometryFilter(FeatureDataRow feature)
        {
            return GeometryFilter == null ||
            GeometryFilter.Intersects(feature.Geometry);
        }

        private bool inOidFilter(FeatureDataRow feature)
        {
            return _oidFilter.Count == 0 ||
                (feature.HasOid && _oidFilter.Contains(feature.GetOid()));
        }

        private bool inAttributeFilter()
        {
            // TODO: perhaps this is where we can execute the DataExpression filter
            return true;
        }

        private void onMissingRegion(Geometry missingRegion)
        {
            Table.RequestFeatures(missingRegion);
        }

        private void onMissingOids(IEnumerable missingOids)
        {
            Table.RequestFeatures(missingOids);
        }
        #endregion

        #region Private static helper methods

        private static SetIndex2Delegate GenerateSetIndex2Delegate()
        {
#if !CFBuild
            // TODO: check if this could be redone with a DynamicMethod,
            // using System.Object in place of the inaccessible System.Data.DataExpression,
            // relying on delegate covariance.
            MethodInfo setIndex2Info = typeof(DataView).GetMethod(
                "SetIndex2", BindingFlags.NonPublic | BindingFlags.Instance);
            ConstructorInfo setIndexDelegateCtor = typeof(SetIndex2Delegate)
                .GetConstructor(new Type[] { typeof(object), typeof(IntPtr) });
            IntPtr setIndex2Pointer = setIndex2Info.MethodHandle.GetFunctionPointer();
            return (SetIndex2Delegate)setIndexDelegateCtor.Invoke(new Object[] { null, setIndex2Pointer });
#else
            return null;
#endif
        }

        private static SetLockedDelegate GenerateSetLockedDelegate()
        {
#if !CFBuild
            // Use LCG to create a set accessor to the DataView.locked field
            DynamicMethod setLockedMethod = new DynamicMethod("set_locked_DynamicMethod",
                                                              null, new Type[] { typeof(DataView), typeof(bool) },
                                                              typeof(DataView));

            ILGenerator il = setLockedMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            FieldInfo lockedField = typeof(DataView).GetField("locked", BindingFlags.Instance | BindingFlags.NonPublic);
            il.Emit(OpCodes.Stfld, lockedField);
            il.Emit(OpCodes.Ret);

            return setLockedMethod.CreateDelegate(typeof(SetLockedDelegate)) as SetLockedDelegate;
#else
            SetLockedDelegate d = new SetLockedDelegate(SetLockedInvoker);
            return d;
#endif
        }

#if CFBuild
        static void SetLockedInvoker(DataView view, bool locked)
        {
            FieldInfo lockedField = typeof(DataView).GetField("locked", BindingFlags.Instance | BindingFlags.NonPublic);
            lockedField.SetValue(view, locked);
        }
#endif


        private static SetDataViewManagerDelegate GenerateSetDataViewManagerDelegate()
        {
#if !CFBuild
            // Use LCG to create a delegate to the internal DataView.SetDataViewManager method
            DynamicMethod setDataViewManagerMethod = new DynamicMethod("set_DataViewManager_DynamicMethod",
                                                                       null,
                                                                       new Type[]
                                                                           {
                                                                               typeof (FeatureDataView),
                                                                               typeof (DataViewManager)
                                                                           }, typeof(DataView));

            ILGenerator il = setDataViewManagerMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            MethodInfo setDataViewManagerInfo = typeof(DataView).GetMethod("SetDataViewManager",
                                                                           BindingFlags.Instance |
                                                                           BindingFlags.NonPublic, null,
                                                                           new Type[] { typeof(DataViewManager) }, null);
            il.Emit(OpCodes.Call, setDataViewManagerInfo);

            return setDataViewManagerMethod.CreateDelegate(typeof(SetDataViewManagerDelegate))
                   as SetDataViewManagerDelegate;

#else
            SetDataViewManagerDelegate d = new SetDataViewManagerDelegate(SetDataViewManagerInvoker);
            return d;
#endif
        }

#if CFBuild
        static void SetDataViewManagerInvoker(FeatureDataView view, DataViewManager dataViewManager)
        {
            MethodInfo setDataViewManagerInfo = typeof(DataView).GetMethod("SetDataViewManager",
                                                                                BindingFlags.Instance |
                                                                                BindingFlags.NonPublic, null,
                                                                                new Type[] { typeof(DataViewManager) }, null);

            setDataViewManagerInfo.Invoke(view, new object[] { dataViewManager });

        }
#endif


        private static object createRowPredicateFilter(Predicate<DataRow> filter)
        {
            // HACK: This is the only way we have to take control of what predicate is used to filter the DataView.
            // Unfortunately, this type is only available in the v2.0 CLR which ships with .Net v3.5 Beta 2 (v2.0.50727.1378)
            // Currently, the only two choices to provided spatially filtered views are to implement 
            // System.ComponentModel.IBindingListView or to rely on v3.5.
            string rowPredicateFilterTypeName =
                "System.Data.DataView+RowPredicateFilter, System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
            Type rowPredicateFilterType = Type.GetType(rowPredicateFilterTypeName);
            Debug.Assert(rowPredicateFilterType != null,
                "Can't find the type System.Data.DataView+RowPredicateFilter. " +
                "This is probably because you are not running the v2.0 CLR which ships with .Net v3.5 Beta 2.");
            object[] args = new object[] { filter };

#if !CFBuild
            object rowPredicateFilter = Activator.CreateInstance(rowPredicateFilterType,
                                                                 BindingFlags.Instance | BindingFlags.NonPublic,
                                                                 null, args, null);
#else //Probably Wrong
            //args An array of arguments that match in number, order, and type the parameters of the constructor
            //to invoke. If args is an empty array or a null reference, the constructor that takes no parameters
            //(the default constructor) is invoked. 

            object rowPredicateFilter = Activator.CreateInstance(rowPredicateFilterType);
#endif
            return rowPredicateFilter;
        }
        #endregion
    }
}