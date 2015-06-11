# System.Reflection.Emit issues #

Reflection.Emit namespace
Things regarding DynamicMethod, and ILGenerator, that doesnt exist in CF

**In SharpMap\Features\FeatureDataRow.cs**

> Changes made:
```
 static FeatureDataRow(){
 #if !CFBuild
     DynamicMethod getColumnsMethod = new DynamicMethod("FeatureDataRow_GetColumns",
		MethodAttributes.Static | MethodAttributes.Public,
		CallingConventions.Standard,
		typeof(DataColumnCollection),	// return type
		new Type[] { typeof(DataRow)},	// one parameter of type DataRow
		typeof(DataRow),	 	// owning type
		false);				// don't skip JIT visibility checks

        
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


*Added this method for making what DynamicMethod getColumnsMethod does
#if CFBuild
 static DataColumnCollection GetColumnsInvoker(DataRow row)
 {
   //OpCodes.ldfld  Finds the value of a field in the object whose reference is currently on the evaluation stack.
   FieldInfo columnsField = typeof(DataRow).GetField("_columns", BindingFlags.NonPublic | BindingFlags.Instance);
   // The return type is the same as the one associated with the field. The field may be either an instance field (in which case the object must not be a null reference) or a static field. So _columns should be a DataColumnCollection 
   DataColumnCollection colCollection = (DataColumnCollection)columnsField.GetValue(row);
   return colCollection;
}
#endif

```




**In SharpMap\Features\FeatureDataTable.cs**
> Changed
> `private static GetDefaultViewDelegate generateGetDefaultViewDelegate()´

> And added

> `static FeatureDataView GetDefaultViewInvoker(FeatureDataTable featureDataT)`

> Changed
> `private static RestoreIndexEventsDelegate generateRestoreIndexEventsDelegate()`

> And added
> > `static void RestoreIndexEventsInvoker(DataTable table, bool forcesReset)`


> Changed

> `private static SuspendIndexEventsDelegate generateSuspendIndexEventsDelegate()`

> And added

> `static void SuspendIndexEventsInvoker(DataTable table)`

> Changed

> `private static CloneToDelegate generateCloneToDelegate()`

> And added

> `static DataTable CloneToDelegateInvoker(DataTable src, DataTable dst, DataSet dataSet, bool skipExpressions)`



**In SharpMap\Features\FeatureDataSet.cs**
> Changed method

> `private static SetDefaultViewManagerDelegate generateSetDefaultViewManagerDelegate()`

> And added

> `static void SetDefaultViewManagerInvoker(FeatureDataSet dataSet, FeatureDataViewManager viewManager) {`

> Changed method

> `private static GetDefaultViewManagerDelegate generateGetDefaultViewManagerDelegate()`

> And added

> `static FeatureDataViewManager GetDefaultViewManagerInvoker(FeatureDataSet dataSet) {`


**In SharpMap\Features\FeatureDataView.cs** **(High, probably wrong)**
> Changed method

> `private static SetLockedDelegate GenerateSetLockedDelegate()`

> And added

> `static void SetLockedInvoker(DataView view, bool locked) {`

> Changed method:

> `private static SetDataViewManagerDelegate GenerateSetDataViewManagerDelegate()`

> And Added

> `static void SetDataViewManagerInvoker(FeatureDataView view, DataViewManager dataViewManager) {`













