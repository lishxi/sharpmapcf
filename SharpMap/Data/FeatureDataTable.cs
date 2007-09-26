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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Reflection;
#if !CFBuild
using System.Reflection.Emit;
#endif
using System.Threading;
using SharpMap.Data;
using SharpMap.Geometries;
using SharpMap.Indexing;
using SharpMap.Indexing.RTree;
using SharpMap.Utilities;

namespace SharpMap.Data
{
    /// <summary>
    /// Represents one feature table of in-memory spatial data. 
    /// </summary>
#if !CFBuild
    [Serializable]
#endif
    public class FeatureDataTable
        : DataTable, IEnumerable<FeatureDataRow>, IEnumerable<IFeatureDataRecord>
    {
        #region Nested types
        private delegate FeatureDataView GetDefaultViewDelegate(FeatureDataTable table);
        private delegate DataTable CloneToDelegate(DataTable src, DataTable dst, DataSet dataSet, bool skipExpressions);
        private delegate void SuspendIndexEventsDelegate(DataTable table);
        private delegate void RestoreIndexEventsDelegate(DataTable table, bool forcesReset);
        #endregion

        #region Type fields
        private static readonly GetDefaultViewDelegate _getDefaultView;
        private static readonly CloneToDelegate _cloneTo;
        private static readonly SuspendIndexEventsDelegate _suspendIndexEvents;
        private static readonly RestoreIndexEventsDelegate _restoreIndexEvents;
        #endregion

        #region Static constructors

        static FeatureDataTable()
        {
            _getDefaultView = generateGetDefaultViewDelegate();
            _cloneTo = generateCloneToDelegate();
            _suspendIndexEvents = generateSuspendIndexEventsDelegate();
            _restoreIndexEvents = generateRestoreIndexEventsDelegate();
        }

        #endregion

        #region Object fields
        private SelfOptimizingDynamicSpatialIndex<FeatureDataRow> _rTreeIndex;
        private Geometry _cachedRegion;
        #endregion

        #region Object constructors

        /// <summary>
        /// Initializes a new instance of the FeatureDataTable class with no arguments.
        /// </summary>
        public FeatureDataTable()
        {
            Constraints.CollectionChanged += OnConstraintsChanged;
        }

        /// <summary>
        /// Initializes a new instance of the FeatureDataTable class with the given
        /// table name.
        /// </summary>
        public FeatureDataTable(string tableName)
            : base(tableName)
        {
            Constraints.CollectionChanged += OnConstraintsChanged;
        }

        /// <summary>
        /// Intitalizes a new instance of the FeatureDataTable class and
        /// copies the name and structure of the given <paramref name="table"/>.
        /// </summary>
        /// <param name="table"></param>
        public FeatureDataTable(DataTable table)
            : base(table.TableName)
        {
            Constraints.CollectionChanged += OnConstraintsChanged;

            if (table.DataSet == null || (table.CaseSensitive != table.DataSet.CaseSensitive))
            {
                CaseSensitive = table.CaseSensitive;
            }

            if (table.DataSet == null || (table.Locale.ToString() != table.DataSet.Locale.ToString()))
            {
                Locale = table.Locale;
            }

            if (table.DataSet == null || (table.Namespace != table.DataSet.Namespace))
            {
                Namespace = table.Namespace;
            }

            Prefix = table.Prefix;
            MinimumCapacity = table.MinimumCapacity;
            DisplayExpression = table.DisplayExpression;
        }

        #endregion

        #region Events

        public event EventHandler<FeaturesRequestedEventArgs> FeaturesRequested;

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

        #endregion

        #region Properties
        /// <summary>
        /// Gets the containing FeatureDataSet.
        /// </summary>
        public new FeatureDataSet DataSet
        {
            get { return (FeatureDataSet)base.DataSet; }
        }

        /// <summary>
        /// Gets the default FeatureDataView for the table.
        /// </summary>
        public new FeatureDataView DefaultView
        {
            get
            {
                FeatureDataView defaultView = DefaultViewInternal;

                if (defaultView == null)
                {
                    if (DataSet != null)
                    {
                        defaultView = DataSet.DefaultViewManager.CreateDataView(this);
                    }
                    else
                    {
                        defaultView = new FeatureDataView(this, true);
                        // This call to SetIndex2 is actually performed in the DataView..ctor(DataTable)
                        // constructor, but for some reason is left out of the DataView..ctor(DataTable, bool)
                        // constructor. Since we call DataView..ctor(DataTable) in 
                        // FeatureDataView..ctor(FeatureDataTable, bool), we don't need this here.
                        //defaultView.SetIndex2("", DataViewRowState.CurrentRows, null, true);
                    }

                    FeatureDataView baseDefaultView = _getDefaultView(this);
                    defaultView = Interlocked.CompareExchange(ref baseDefaultView, defaultView, null);

                    if (defaultView == null)
                    {
                        defaultView = baseDefaultView;
                    }
                }

                return defaultView;
            }
        }

        /// <summary>
        /// Gets the full extents of all features in the feature table.
        /// </summary>
        public BoundingBox Extents
        {
            get { return _cachedRegion.GetBoundingBox(); }
        }

        /// <summary>
        /// Gets the number of feature rows in the feature table.
        /// </summary>
#if !CFBuild
        [Browsable(false)]
#endif
        public int FeatureCount
        {
            get { return base.Rows.Count; }
        }

        /// <summary>
        /// Gets the feature data row at the specified index.
        /// </summary>
        /// <param name="index">Index of the row to retrieve.</param>
        /// <returns>
        /// The FeatureDataRow at the given <paramref name="index"/>.
        /// </returns>
        public FeatureDataRow this[int index]
        {
            get { return base.Rows[index] as FeatureDataRow; }
        }

        /// <summary>
        /// Gets or sets a value indicating if the table is spatially indexed.
        /// </summary>
        public bool IsSpatiallyIndexed
        {
            get { return _rTreeIndex != null; }
            set
            {
                if (value && _rTreeIndex == null)
                {
                    initializeSpatialIndex();
                }
                else if (!value && _rTreeIndex != null)
                {
                    _rTreeIndex.Dispose();
                    _rTreeIndex = null;
                }
            }
        }

        /// <summary>
        /// Gets the collection of rows that belong to this table.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// Thrown if this property is set.
        /// </exception>
        public new DataRowCollection Rows
        {
            get { return base.Rows; }
            set { throw new NotSupportedException(); }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Adds a row to the FeatureDataTable
        /// </summary>
        /// <param name="row"></param>
        public void AddRow(FeatureDataRow row)
        {
            base.Rows.Add(row);
        }

        /// <summary>
        /// Clones the structure of the FeatureDataTable, 
        /// including all FeatureDataTable schemas and constraints. 
        /// </summary>
        /// <returns></returns>
        public new FeatureDataTable Clone()
        {
            return base.Clone() as FeatureDataTable;
        }

        public void CloneTo(FeatureDataTable table)
        {
            _cloneTo(this, table, null, false);
        }

        public FeatureDataRow Find(object key)
        {
            return Rows.Find(key) as FeatureDataRow;
        }

        public new FeatureDataTable GetChanges()
        {
            FeatureDataTable changes = Clone();
            FeatureDataRow row;

            for (int i = 0; i < Rows.Count; i++)
            {
                row = Rows[i] as FeatureDataRow;
                if (row.RowState != DataRowState.Unchanged)
                {
                    changes.ImportRow(row);
                }
            }

            if (changes.Rows.Count == 0)
            {
                return null;
            }

            return changes;
        }

        /// <summary>
        /// Returns an enumerator for enumering the rows of the FeatureDataTable
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator<FeatureDataRow> GetEnumerator()
        {
            foreach (FeatureDataRow row in Rows)
            {
                yield return row;
            }
        }

        public void ImportRow(FeatureDataRow featureRow)
        {
            FeatureDataRow newRow = NewRow();
            copyRow(featureRow, newRow);
            AddRow(newRow);
        }

        public override void Load(IDataReader reader, LoadOption loadOption, FillErrorEventHandler errorHandler)
        {
            if (!(reader is IFeatureDataReader))
            {
                throw new NotSupportedException("Only IFeatureDataReader instances are supported to load from.");
            }

            Load(reader as IFeatureDataReader, loadOption, errorHandler);
        }

        public void Load(IFeatureDataReader reader, LoadOption loadOption, FillErrorEventHandler errorHandler)
        {
            if (reader == null) throw new ArgumentNullException("reader");

            LoadFeaturesAdapter adapter = new LoadFeaturesAdapter();
            adapter.FillLoadOption = loadOption;
            adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;

            if (errorHandler != null)
            {
                adapter.FillError += errorHandler;
            }

            adapter.Fill(this, reader);

            if (!reader.IsClosed && !reader.NextResult())
            {
                reader.Close();
            }
        }

        /// <summary>
        /// Creates a new FeatureDataRow with the same schema as the table.
        /// </summary>
        /// <returns></returns>
        public new FeatureDataRow NewRow()
        {
            return base.NewRow() as FeatureDataRow;
        }

        /// <summary>
        /// Removes the row from the table
        /// </summary>
        /// <param name="row">Row to remove</param>
        public void RemoveRow(FeatureDataRow row)
        {
            base.Rows.Remove(row);
        }

        public IEnumerable<FeatureDataRow> Select(BoundingBox bounds)
        {
            if (IsSpatiallyIndexed)
            {
                foreach (RTreeIndexEntry<FeatureDataRow> entry in _rTreeIndex.Search(bounds))
                {
                    yield return entry.Value;
                }
            }
            else
            {
                foreach (FeatureDataRow feature in this)
                {
                    if (bounds.Intersects(feature.Geometry))
                    {
                        yield return feature;
                    }
                }
            }
        }

        public IEnumerable<FeatureDataRow> Select(Geometry geometry)
        {
            if (IsSpatiallyIndexed)
            {
                foreach (RTreeIndexEntry<FeatureDataRow> entry in _rTreeIndex.Search(geometry))
                {
                    yield return entry.Value;
                }
            }
            else
            {
                foreach (FeatureDataRow feature in this)
                {
                    if (geometry.Intersects(feature.Geometry))
                    {
                        yield return feature;
                    }
                }
            }
        }

        #endregion

        protected virtual void OnConstraintsChanged(object sender, CollectionChangeEventArgs args)
        {
            if (args.Action == CollectionChangeAction.Add && args.Element is UniqueConstraint)
            {
                UniqueConstraint constraint = args.Element as UniqueConstraint;

                // If more than one column is added to the primary key, throw
                // an exception - we don't support it for now.
                if (constraint.IsPrimaryKey && constraint.Columns.Length > 1)
                {
                    throw new NotSupportedException("Compound primary keys not supported.");
                }
            }
        }

        internal Geometry CachedRegion
        {
            get { return _cachedRegion ?? Point.Empty; }
        }

        internal void RequestFeatures(Geometry missingRegion)
        {
            EventHandler<FeaturesRequestedEventArgs> e = FeaturesRequested;

            if (e != null)
            {
                FeaturesRequestedEventArgs args = new FeaturesRequestedEventArgs(null, missingRegion);

                e(this, args);
            }
        }

        internal void RequestFeatures(IEnumerable missingOids)
        {
            EventHandler<FeaturesRequestedEventArgs> e = FeaturesRequested;

            if (e != null)
            {
                FeaturesRequestedEventArgs args = new FeaturesRequestedEventArgs(missingOids, null);

                e(this, args);
            }
        }

        #region Protected overrides

        protected override void OnTableCleared(DataTableClearEventArgs e)
        {
            base.OnTableCleared(e);

            if (_rTreeIndex != null)
            {
                _rTreeIndex.Clear();
            }
        }

        /// <summary>
        /// Creates and returns a new instance of a FeatureDataTable.
        /// </summary>
        /// <returns>An empty FeatureDataTable.</returns>
        protected override DataTable CreateInstance()
        {
            return new FeatureDataTable();
        }

        /// <summary>
        /// Creates a new FeatureDataRow with the same schema as the table, 
        /// based on a datarow builder.
        /// </summary>
        /// <param name="builder">
        /// The DataRowBuilder instance to use to construct
        /// a new row.
        /// </param>
        /// <returns>A new DataRow using the schema in the DataRowBuilder.</returns>
        protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
        {
            return new FeatureDataRow(builder);
        }

        /// <summary>
        /// Returns the FeatureDataRow type.
        /// </summary>
        /// <returns>The <see cref="Type"/> <see cref="FeatureDataRow"/>.</returns>
        protected override Type GetRowType()
        {
            return typeof(FeatureDataRow);
        }

        #endregion

        #region Event generators

        /// <summary>
        /// Raises the FeatureDataRowChanged event. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRowChanged(DataRowChangeEventArgs e)
        {
            FeatureDataRow r = e.Row as FeatureDataRow;

            Debug.Assert(r != null);

            switch (e.Action)
            {
                case DataRowAction.Add:
#warning: Does this never work, since r.Geometry isn't populated when rows are loaded?
                    if (r.Geometry != null)
                    {
                        if (_cachedRegion == null)
                        {
                            _cachedRegion = r.Geometry;
                        }
                        else
                        {
                            _cachedRegion = _cachedRegion.Union(r.Geometry);
                        }

                        if (IsSpatiallyIndexed)
                        {
                            RTreeIndexEntry<FeatureDataRow> entry =
                                new RTreeIndexEntry<FeatureDataRow>(r, r.Geometry.GetBoundingBox());
                            _rTreeIndex.Insert(entry);
                        }
                    }
                    break;
                case DataRowAction.Change:
                    throw new NotImplementedException(
                        "Need to implement FeatureDataTable cached region update on changed row.");
                case DataRowAction.ChangeCurrentAndOriginal:
                    throw new NotImplementedException(
                        "Need to implement FeatureDataTable cached region update on changed row.");
                case DataRowAction.ChangeOriginal:
                    throw new NotImplementedException(
                        "Need to implement FeatureDataTable cached region update on changed row.");
                case DataRowAction.Delete:
                    if (r.Geometry != null)
                    {
                        _cachedRegion = _cachedRegion.Difference(r.Geometry);

                        if (IsSpatiallyIndexed)
                        {
                            RTreeIndexEntry<FeatureDataRow> entry =
                                new RTreeIndexEntry<FeatureDataRow>(r, r.Geometry.GetBoundingBox());
                            _rTreeIndex.Remove(entry);
                        }
                    }
                    break;
                case DataRowAction.Commit:
                case DataRowAction.Rollback:
                case DataRowAction.Nothing:
                default:
                    break;
            }

            if ((FeatureDataRowChanged != null))
            {
                FeatureDataRowChanged(this, new FeatureDataRowChangeEventArgs(((FeatureDataRow)(e.Row)), e.Action));
            }

            base.OnRowChanged(e);
        }

        /// <summary>
        /// Raises the FeatureDataRowChanging event. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRowChanging(DataRowChangeEventArgs e)
        {
            base.OnRowChanging(e);

            if ((FeatureDataRowChanging != null))
            {
                FeatureDataRowChanging(this, new FeatureDataRowChangeEventArgs(((FeatureDataRow)(e.Row)), e.Action));
            }
        }

        /// <summary>
        /// Raises the FeatureDataRowDeleted event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRowDeleted(DataRowChangeEventArgs e)
        {
            base.OnRowDeleted(e);

            if ((FeatureDataRowDeleted != null))
            {
                FeatureDataRowDeleted(this, new FeatureDataRowChangeEventArgs(((FeatureDataRow)(e.Row)), e.Action));
            }
        }

        /// <summary>
        /// Raises the FeatureDataRowDeleting event. 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnRowDeleting(DataRowChangeEventArgs e)
        {
            base.OnRowDeleting(e);
            if ((FeatureDataRowDeleting != null))
            {
                FeatureDataRowDeleting(this, new FeatureDataRowChangeEventArgs(((FeatureDataRow)(e.Row)), e.Action));
            }
        }

        #endregion

        #region Internal helper methods

        internal FeatureDataView DefaultViewInternal
        {
            get { return _getDefaultView(this); }
        }

        internal void RowGeometryChanged(FeatureDataRow row)
        {
            row.BeginEdit();
            row.EndEdit();
        }

        public void MergeSchema(FeatureDataTable target)
        {
            MergeSchema(target, SchemaMergeAction.Add | SchemaMergeAction.Key);
        }

        public void MergeSchema(FeatureDataTable target, SchemaMergeAction schemaMergeAction)
        {
            FeatureMerger merger = new FeatureMerger(target, true, schemaMergeAction);
            merger.MergeSchema(this);
        }

        internal void MergeFeature(IFeatureDataRecord record)
        {
            MergeFeature(record, SchemaMergeAction.AddWithKey);
        }

        internal void MergeFeature(IFeatureDataRecord record, SchemaMergeAction schemaMergeAction)
        {
            FeatureMerger merger = new FeatureMerger(this, true, schemaMergeAction);
            merger.MergeFeature(record);
        }

        internal void MergeFeatures(IEnumerable<IFeatureDataRecord> records)
        {
            MergeFeatures(records, SchemaMergeAction.AddWithKey);
        }

        internal void MergeFeatures(IEnumerable<IFeatureDataRecord> records, SchemaMergeAction schemaMergeAction)
        {
            FeatureMerger merger = new FeatureMerger(this, true, schemaMergeAction);
            merger.MergeFeatures(records);
        }

        internal void SuspendIndexEvents()
        {
            _suspendIndexEvents(this);
        }

        internal void RestoreIndexEvents(bool forceReset)
        {
            _restoreIndexEvents(this, forceReset);
        }

        #endregion

        #region Private static helper methods


        private static GetDefaultViewDelegate generateGetDefaultViewDelegate()
        {
#if !CFBuild
            DynamicMethod get_DefaultViewMethod = new DynamicMethod("get_DefaultView_DynamicMethod",
                                                                    typeof(FeatureDataView),
                                                                    new Type[] { typeof(FeatureDataTable) }, typeof(DataTable));

            ILGenerator il = get_DefaultViewMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, typeof(DataTable).GetField("defaultView", BindingFlags.Instance | BindingFlags.NonPublic));
            il.Emit(OpCodes.Ret);

            return get_DefaultViewMethod.CreateDelegate(typeof(GetDefaultViewDelegate))
                   as GetDefaultViewDelegate;
#else
            //GetDefaultViewDelegate del = GetDefaultViewInvoker;
            GetDefaultViewDelegate del = new GetDefaultViewDelegate(GetDefaultViewInvoker);
            return del;
#endif

        }

#if CFBuild
        static FeatureDataView GetDefaultViewInvoker(FeatureDataTable featureDataT)
        {
            //OpCodes.ldfld  Finds the value of a field in the object whose reference is currently on the evaluation stack.
            FieldInfo defaultViewField = typeof(DataTable).GetField("defaultView",
                                                    BindingFlags.Instance | BindingFlags.NonPublic);

            // The return type is the same as the one associated with the field. The field may be either an instance 
            //field (in which case the object must not be a null reference) or a static field.
            //So _columns should be a DataColumnCollection 
            FeatureDataView view = (FeatureDataView)defaultViewField.GetValue(featureDataT);
            return view;
        }
#endif




        private static RestoreIndexEventsDelegate generateRestoreIndexEventsDelegate()
        {
#if !CFBuild
            DynamicMethod restoreIndexEventsMethod = new DynamicMethod("FeatureDataTable_RestoreIndexEvents",
                                                                       MethodAttributes.Public | MethodAttributes.Static,
                                                                       CallingConventions.Standard,
                                                                       null,
                                                                       new Type[] { typeof(DataTable), typeof(bool) },
                                                                       typeof(DataTable),
                                                                       false);

            ILGenerator il = restoreIndexEventsMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            MethodInfo restoreIndexEventsInfo = typeof(DataTable).GetMethod("RestoreIndexEvents",
                                                                            BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(bool) }, null);
            il.Emit(OpCodes.Call, restoreIndexEventsInfo);
            il.Emit(OpCodes.Ret);

            return (RestoreIndexEventsDelegate)restoreIndexEventsMethod.CreateDelegate(typeof(RestoreIndexEventsDelegate));

#else
            //return RestoreIndexEventsInvoker;
            RestoreIndexEventsDelegate del = new RestoreIndexEventsDelegate(RestoreIndexEventsInvoker);
            return del;
#endif
        }

#if CFBuild
        static void RestoreIndexEventsInvoker(DataTable table, bool forcesReset)
        {
            MethodInfo restoreIndexEventsInfo = typeof(DataTable).GetMethod("RestoreIndexEvents",
                           BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(bool) }, null);
            object[] parameters = new object[] { forcesReset };
            restoreIndexEventsInfo.Invoke(table, parameters);
        }
#endif


        private static SuspendIndexEventsDelegate generateSuspendIndexEventsDelegate()
        {
#if !CFBuild
            DynamicMethod suspendIndexEventsMethod = new DynamicMethod("FeatureDataTable_SuspendIndexEvents",
                                                                       MethodAttributes.Public | MethodAttributes.Static,
                                                                       CallingConventions.Standard,
                                                                       null,
                                                                       new Type[] { typeof(DataTable) },
                                                                       typeof(DataTable),
                                                                       false);

            ILGenerator il = suspendIndexEventsMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            MethodInfo suspendIndexEventsInfo = typeof(DataTable).GetMethod("SuspendIndexEvents",
                                                                            BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
            il.Emit(OpCodes.Call, suspendIndexEventsInfo);
            il.Emit(OpCodes.Ret);

            return (SuspendIndexEventsDelegate)suspendIndexEventsMethod.CreateDelegate(typeof(SuspendIndexEventsDelegate));

#else
            SuspendIndexEventsDelegate del = new SuspendIndexEventsDelegate(SuspendIndexEventsInvoker);
            return del;
#endif
        }

#if CFBuild
        static void SuspendIndexEventsInvoker(DataTable table)
        {
            Type[] types = new Type[0];
            MethodInfo suspendIndexEventsInfo = typeof(DataTable).GetMethod("SuspendIndexEvents",
                            BindingFlags.NonPublic | BindingFlags.Instance, null, types, null);

            suspendIndexEventsInfo.Invoke(table, null);
        }
#endif


        private static CloneToDelegate generateCloneToDelegate()
        {
#if !CFBuild
            DynamicMethod cloneToMethod = new DynamicMethod("FeatureDataTable_CloneTo",
                                                            MethodAttributes.Public | MethodAttributes.Static,
                                                            CallingConventions.Standard,
                                                            typeof(DataTable),
                                                            new Type[] { typeof(DataTable), typeof(DataTable), typeof(DataSet), typeof(bool) },
                                                            typeof(DataTable),
                                                            false);

            ILGenerator il = cloneToMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldarg_3);
            MethodInfo cloneToInfo = typeof(DataTable).GetMethod("CloneTo", BindingFlags.NonPublic | BindingFlags.Instance);
            il.Emit(OpCodes.Call, cloneToInfo);
            il.Emit(OpCodes.Ret);

            return (CloneToDelegate)cloneToMethod.CreateDelegate(typeof(CloneToDelegate));
#else
            CloneToDelegate del = new CloneToDelegate(CloneToDelegateInvoker);
            return del;
#endif
        }

#if CFBuild
        static DataTable CloneToDelegateInvoker(DataTable src, DataTable dst, DataSet dataSet, bool skipExpressions)
        {
            MethodInfo cloneToInfo = typeof(DataTable).GetMethod("CloneTo", BindingFlags.NonPublic | BindingFlags.Instance);
            DataTable dt = (DataTable)cloneToInfo.Invoke(src, new object[] { dst, dataSet, skipExpressions });
            return dt;
        }
#endif

        #endregion

        #region Private helper methods

        private static void copyRow(FeatureDataRow source, FeatureDataRow target)
        {
            if (source.Table.Columns.Count != target.Table.Columns.Count)
            {
                throw new InvalidOperationException("Can't import a feature row with a different number of columns.");
            }

            for (int columnIndex = 0; columnIndex < source.Table.Columns.Count; columnIndex++)
            {
                target[columnIndex] = source[columnIndex];
            }

            target.Geometry = source.Geometry == null
                                  ? null
                                  : source.Geometry.Clone();
        }

        private void initializeSpatialIndex()
        {
            // TODO: implement Post-optimization restructure strategy
            IIndexRestructureStrategy restructureStrategy = new NullRestructuringStrategy();
            RestructuringHuristic restructureHeuristic = new RestructuringHuristic(RestructureOpportunity.None, 4.0);
            IEntryInsertStrategy<RTreeIndexEntry<FeatureDataRow>> insertStrategy = new GuttmanQuadraticInsert<FeatureDataRow>();
            INodeSplitStrategy nodeSplitStrategy = new GuttmanQuadraticSplit<FeatureDataRow>();
            IdleMonitor idleMonitor = null;
            _rTreeIndex = new SelfOptimizingDynamicSpatialIndex<FeatureDataRow>(restructureStrategy,
                                                                                restructureHeuristic, insertStrategy,
                                                                                nodeSplitStrategy,
                                                                                new DynamicRTreeBalanceHeuristic(), idleMonitor);
        }

        #endregion

        #region IEnumerable members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IEnumerable<IFeatureDataRecord> members

        IEnumerator<IFeatureDataRecord> IEnumerable<IFeatureDataRecord>.GetEnumerator()
        {
            foreach (FeatureDataRow row in this)
            {
                yield return row;
            }
        }

        #endregion
    }

#if !CFBuild
    [Serializable]
#endif
    public class FeaturesRequestedEventArgs : EventArgs
    {
        private readonly Geometry _requestedRegion;
        private readonly IEnumerable _requestedOids;
        private readonly object _requestedExpression;

        public FeaturesRequestedEventArgs(IEnumerable missingOids, Geometry missingRegion)
        {
            _requestedOids = missingOids;
            _requestedRegion = missingRegion;
        }

        public IEnumerable RequestedOids
        {
            get { return _requestedOids; }
        }

        public Geometry RequestedRegion
        {
            get { return _requestedRegion; }
        }

        public Object RequestedExpression
        {
            get { return _requestedExpression; }
        }
    }
}