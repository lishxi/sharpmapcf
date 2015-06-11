# Introduction #

These are the steps followed trying to port SharpMap v2.0 to the .NET Compact Framewok.

#  #
The original source code has not been removed, but commented using a preprocessing directive (CFBuild).
For example, there's no System.Runtime.Serialization in CF so we do
```
 #if !CFBuild
   using System.Runtime.Serialization;
 #endif
```
# Procedure #
  * External references
  * Removing unsupported features
  * Deleted Methods
  * Deleted fields
  * Other things
  * System.Reflection.Emit issues [PortingDynamicMethod](PortingDynamicMethod.md)


## External references ##


## Removing unsupported features ##
Deleted [Serializable](Serializable.md) Attribute on:
  * SharpMap/Rendering/Rendering2D/Size2D.cs
  * SharpMap/Styles/Style.cs
  * SharpMap/Styles/StyleColor.cs
  * SharpMap/Styles/StyleBrush.cs
  * SharpMap/Styles/StyleFont.cs
  * SharpMap/Styles/StyleFontFamily.cs
  * SharpMap/Styles/StyleFontPen.cs
  * SharpMap/Presentation/ViewPointHoverEventArgs.cs
  * SharpMap/Rendering/Rendering3D.ViewRectangle3D.cs
  * SharpMap/Utilities/Tolerance.cs
  * SharpMap\Presentation\LayerActionEventArgs.cs
  * SharpMap\Indexing\BinaryTree\BinaryTreeNode.cs
  * SharpMap\Data\LayerDataLoadException.cs
  * SharpMap\Geometries\BoundingBox.cs
  * SharpMap\Layers\Layer.cs
  * SharpMap\Geometries\MultiPolygon.cs
  * SharpMap\Converters\ParseException.cs
  * SharpMap\Geometries\MultiLineString.cs
  * SharpMap\Features\FeatureDataRow.cs
  * SharpMap\Indexing\BinaryTree\BinaryTree.cs
  * SharpMap\Rendering\Rendering2D\Point2D.cs
  * SharpMap\Layers\DuplicateLayerException.cs
  * SharpMap\Rendering\Rendering2D\Matrix2D.cs
  * SharpMap\Features\FeatureDataTable.cs
  * SharpMap\Presentation\MapViewPropertyChangeEventArgs.cs
  * SharpMap\Data\SharpMapDataException.cs
  * SharpMap\Presentation\MapZoomChangedEventArgs.cs
  * SharpMap\Geometries\Polygon.cs
  * SharpMap\Geometries\Polygon.cs
  * SharpMap\Rendering\Rendering2D\Rectangle2D.cs
  * SharpMap\Geometries\Point.cs
  * SharpMap\CoordinateSystems\Projections\ProjectionComputationException.cs
  * SharpMap\Features\FeatureDataSet.cs
  * SharpMap\Geometries\Geometries3D\Point3D.cs
  * SharpMap\Geometries\Geometry.cs
  * SharpMap\Geometries\LineString.cs
  * SharpMap\Layers\LayersChangedEventArgs.cs
  * SharpMap\Features\FeatureTableCollection.cs

**Deleted "Using System.Runtime.Serialization" in**

  * SharpMap\Features\FeatureDataSet.cs
  * SharpMap\CoordinateSystems\Projections\ProjectionComputationException.cs
  * SharpMap\Rendering\Rendering2D\Symbol2D.cs
  * SharpMap\Data\Providers\Shapefile\DbfSchemaMismatchException.cs
  * SharpMap\Data\Providers\Shapefile\ShapeFileIsInvalidException.cs
  * SharpMap\Data\LayerDataLoadException.cs
  * SharpMap\Data\Providers\Shapefile\ShapeFileInvalidOperationException.cs
  * SharpMap\Indexing\ObsoleteIndexFileFormatException.cs
  * SharpMap\Converters\ParseException.cs
  * SharpMap\Data\Providers\Shapefile\InvalidDbaseFileException.cs
  * SharpMap\Layers\DuplicateLayerException.cs
  * SharpMap\Data\Providers\Shapefile\ShapeFileException.cs
  * SharpMap\Data\Providers\Shapefile\InvalidDbaseFileOperationException.cs
  * SharpMap\Data\SharpMapDataException.cs
  * SharpMap\Data\Providers\Shapefile\ShapeFileUnsupportedGeometryException.cs
  * SharpMap\Indexing\RTree\DynamicRTree.cs

Deleted "Using System.Rflection.Emit" in:

  * SharpMap\Features\FeatureDataView.cs
  * SharpMap\Features\FeatureDataSet.cs
  * SharpMap\Features\FeatureDataRow.cs
  * SharpMap\Features\FeatureMerger.cs
  * SharpMap\Features\FeatureDataTable.cs


Deleted "using System.Runtime.Serialization.Formatters.Binary;" in
  * SharpMap\Indexing\RTree\DynamicRTree.cs

Deleted "[global::System.Serializable]" in
  * SharpMap\Indexing\QuadTree\LinearQuadTreeEntryInsertStrategy.cs


Deleted interface "ISerializable" in
  * SharpMap\Rendering\Rendering2D\Symbol2D.cs
  * SharpMap\Indexing\RTree\DynamicRTree.cs


## Deleted Methods ##

**In SharpMap\Data\LayerDataLoadException.cs**
Deleted
> public LayerDataLoadException(SerializationInfo info, StreamingContext context)
> public override void GetObjectData(SerializationInfo info, StreamingContext context)


**In SharpMap\Features\FeatureDataSet.cs**

> protected FeatureDataSet(SerializationInfo info, StreamingContext context)


**In SharpMap\Rendering\Rendering2D\Symbol2D.cs**

> private Symbol2D(SerializationInfo info, StreamingContext context)
> public void GetObjectData(SerializationInfo info, StreamingContext context)


**In SharpMap\Indexing\RTree\DynamicRTree.cs**
> protected DynamicRTree(SerializationInfo info, StreamingContext context)
> > public void GetObjectData(SerializationInfo info, StreamingContext context)

**In SharpMap\Data\Providers\Shapefile\ShapeFileUnsupportedGeometryException.cs**

> public ShapeFileUnsupportedGeometryException(SerializationInfo info, StreamingContext context)


**In SharpMap\CoordinateSystems\Projections\ProjectionComputationException.cs**

> public ProjectionComputationException(SerializationInfo info, StreamingContext context)


**In SharpMap\Data\Providers\Shapefile\InvalidDbaseFileOperationException.cs**
> public InvalidDbaseFileOperationException(SerializationInfo info, StreamingContext context)


**SharpMap\Data\SharpMapDataException.cs**
> public SharpMapDataException(SerializationInfo info, StreamingContext context)


**In SharpMap\Data\Providers\Shapefile\ShapeFileException.cs**
> public ShapeFileException(SerializationInfo info, StreamingContext context)


**In SharpMap\Indexing\QuadTree\LinearQuadTreeEntryInsertStrategy.cs**

> protected QuadTreeIndexInsertOverflowException(

**In SharpMap\Layers\DuplicateLayerException.cs**
> protected DuplicateLayerException(SerializationInfo info, StreamingContext context)

**In SharpMap\Layers\DuplicateLayerException.cs**
> public override void GetObjectData(SerializationInfo info, StreamingContext context)


**In SharpMap\Data\Providers\Shapefile\InvalidDbaseFileException.cs**
> public InvalidDbaseFileException(SerializationInfo info, StreamingContext context)

**In SharpMap\Data\Providers\Shapefile\InvalidDbaseFileException.cs**
> public override void GetObjectData(SerializationInfo info, StreamingContext context)

**In SharpMap\Converters\ParseException.cs**
> protected ParseException(SerializationInfo info, StreamingContext context)

**In SharpMap\Indexing\ObsoleteIndexFileFormatException.cs**
> public ObsoleteIndexFileFormatException(SerializationInfo info, StreamingContext context)


**In SharpMap\Data\Providers\Shapefile\ShapeFileInvalidOperationException.cs**
> public ShapeFileInvalidOperationException(SerializationInfo info, StreamingContext context)

In public ShapeFileIsInvalidException(SerializationInfo info, StreamingContext context)
> public ShapeFileIsInvalidException(SerializationInfo info, StreamingContext context)


**In SharpMap\Data\Providers\Shapefile\DbfSchemaMismatchException.cs**
> public DbfSchemaMismatchException(SerializationInfo info, StreamingContext context)




## Deleted fields ##

**In SharpMap\Indexing\RTree\DynamicRTree.cs**
> private static readonly BinaryFormatter _keyFormatter = new BinaryFormatter();
Reason: No System.Runtime.Serialization.Formatters.Binary.BinaryFormatter_

**In SharpMap\Features\FeatureDataTable.cs**
> [Browsable(false)]


## Other Things ##

**In SharpMap\Layers\FeatureLayer.cs:**

**Problem:**
> There's no System.ComponentModel.BackgroundWorker() on the CF. This thread is used
when property VisibleRegion changes. It triggers the request of the data (that covers the new region) asynchronously (if AsyncQuery is true)


**In SharpMap\Layers\FeatureLayer.cs:**

Problem:
> There's no System.ComponentModel.BackgroundWorker() on the CF. This thread is used
when property VisibleRegion changes for requesting the data (that covers the new region) asynchronously (if AsyncQuery is true)

Workaround:
> new /Util/BackgroundWorker.cs that extends System.ComponentModel.Component


> FeatureLayer.cs does not change, but /Util/BackgroundWorker.cs is totally commented out if the build is not for CF




**In SharpMap\CoordinateSystems\Info.cs**
> Problem:
> `sb.AppendFormat("<CS_Info");`
> No overload for method 'AppendFormat' (StringBuilder) takes '1' arguments. In CF only overload is
> > StringBuilder.AppendFormat(System.IFormatProvider provider, string format, params object[.md](.md) args)

> Workaround:

> `sb.AppendFormat("<CS_Info");`

> Problem:
> `sb.AppendFormat(null," AuthorityCode=\"{0}\"", AuthorityCode);`
> > The same as above.


> Workaround:
> If the provider parameter is a null reference, format provider information is obtained from the current culture. So
we can use instead:
> `sb.AppendFormat(null," AuthorityCode=\"{0}\"", AuthorityCode);`


**In SharpMap\CoordinateSystems\AngularUnit.cs**

> Problem:
> The same as in Info.cs

> `sb.AppendFormat(", AUTHORITY[\"{0}\", \"{1}\"]", Authority, AuthorityCode);`

> Workaround:

> `sb.AppendFormat(", AUTHORITY[\"{0}\", \"{1}\"]", Authority, AuthorityCode);`





**In SharpMap\Data\Providers\Shapefile\DbaseReader.cs**

> Problem:
```
 if (DateTime.TryParseExact(Encoding.UTF8.GetString((_dbaseReader.ReadBytes(8))),
     "yyyyMMdd", DbaseConstants.StorageNumberFormat, DateTimeStyles.None, out date))
 {
     return date;
 }
 else
 {
     return DBNull.Value;
 }
```
> In CF Encoding.UTF8.GetString takes 3 arguments so we use instead
```
 try
 {
    brBytes = _dbaseReader.ReadBytes(8);
    return date = DateTime.ParseExact(Encoding.UTF7.GetString(brBytes,0,brBytes.Length),
                 "yyyyMMdd", DbaseConstants.StorageNumberFormat, DateTimeStyles.None);
 }
 catch (Exception e)
 {
        return DBNull.Value;
 }
```


**In SharpMap\CoordinateSystems\Projection.cs**
> Problem:

> `sb.AppendFormat("PROJECTION[\"{0}\"", Name);`

> Workaround:

> Use `sb.AppendFormat(null, "PROJECTION[\"{0}\"", Name);`


**In SharpMap\CoordinateSystems\HorizontalDatum.cs**
> Problem (3 of this kind):

> `sb.AppendFormat(", {0}", _wgs84ConversionInfo.WKT);`

> Workaround:

> `sb.AppendFormat(null,", {0}", _wgs84ConversionInfo.WKT);`


**In SharpMap\CoordinateSystems\GeographicCoordinateSystem.cs**
> Problem (4 of this kind):

> `sb.AppendFormat(", AUTHORITY[\"{0}\", \"{1}\"]", Authority, AuthorityCode);`

> Workaround:

> `sb.AppendFormat(null, ", AUTHORITY[\"{0}\", \"{1}\"]", Authority, AuthorityCode);`


**In SharpMap\CoordinateSystems\ProjectedCoordinateSystem.cs**
> Problem (5 of this kind):

> `sb.AppendFormat(", {0}", LinearUnit.Wkt);`

> Workaround:

> `sb.AppendFormat(null, ", {0}", LinearUnit.Wkt);`




**In SharpMap\CoordinateSystems\GeocentricCoordinateSystem.cs**
> Problem (4 of this kind):

> `sb.AppendFormat(string, args);`

> Workaround:

> `sb.AppendFormat(null, string, args);`




**In SharpMap\CoordinateSystems\Unit.cs**
> Problem (1 of this kind):

> `sb.AppendFormat(string, args);`

> Workaround:

> `sb.AppendFormat(null, string, args);`


**In SharpMap\CoordinateSystems\Projections\MapProjection.cs**
> Problem (5 of this kind):

> `sb.AppendFormat(string, args);`

> Workaround:

> `sb.AppendFormat(null, string, args);`


> Problem (2 of this kind):

> `throw new ArgumentOutOfRangeException("x",y," not a valid latitude in degrees.");`

> Workaround:

> `throw new ArgumentOutOfRangeException("x"," x:"+y + " not a valid latitude in degrees.");`


**In SharpMap\Presentation\Presenters\MapPresenter2D.cs**
> Problem (2 of this kind):

> `throw new ArgumentOutOfRangeException("value", value, "Invalid pixel aspect ratio.");`

> Workaround:

> `throw new ArgumentOutOfRangeException("value","value("+value+") Invalid pixel aspect ratio.");`

**In SharpMap\Rendering\Rendering2D\Size2D.cs**
> Problem (3 of this kind):

> `throw new ArgumentOutOfRangeException("row", row, "A Point2D has only 1 row.");`

> Workaround:

`throw new ArgumentOutOfRangeException("row", "row("+row+") A Point2D has only 1 row.");`


**In SharpMap\Indexing\BinaryTree\BinaryTree.cs**
> Problem:

> `Array.ForEach(items, Add);`
> > //Error:System.Array' does not contain a definition for 'ForEach'


> Workaround:
```
	foreach (ItemValue item in items)
                Add(item);
```


> Problem:
> `Trace.WriteLine(root.Item.ToString());`
> //Error 136 'System.Diagnostics.Trace' does not contain a definition for 'WriteLine'

> Workaround:

> Commented out


**In C:\Desarrollo\VisualStudio\proyects\SharpMapCFv2.0\SharpMap\Styles\StylePen.cs**
> Problem:

> `buffer.AppendFormat("N3", value);`
> //Error The best overloaded method match for 'System.Text.StringBuilder.AppendFormat(System.IFormatProvider, string, params object[.md](.md))' has some invalid arguments

> Workaround:

> `buffer.AppendFormat(null, "N3", value);`


**In SharpMap\Rendering\Rendering2D\Symbol2D.cs (medium)**
> Problem:

> `_symbolData.Dispose();` (Stream _symbolData)_

> In CF Stream, Dispose() is protected

> Workaround:

> let's close it

> `_symbolData.Close();`



**In SharpMap\Data\Providers\Shapefile\DbaseEncodingRegistry.cs**
> Problem:

> `return Encoding.GetEncoding(CultureInfo.TextInfo.MacCodePage);

> In CF Stream, TextInfo only has ANSICodePage

> Workaround:

> let's make TextInfo.ANSICodePage default

**In SharpMap\Rendering\Rendering2D\Point2D.cs**
> Problem (3 of this kind):

> `throw new ArgumentOutOfRangeException("column", row, "A Point2D has only 2 columns.");`
> > Error	No overload for method 'ArgumentOutOfRangeException' takes '3' arguments


> Workaround:

> `throw new ArgumentOutOfRangeException("column", "column ("+row+") A Point2D has only 2 columns.");`




**In C:\Desarrollo\VisualStudio\proyects\SharpMapCFv2.0\SharpMap\Map.cs**
> Problem:
```
         if (layerName.Contains(layerNamePart))
         {
             yield return layer;
         }

```

> Workaround:
> String.Contains return true if the value parameter occurs within this string, or if value
> > is the empty string (""); otherwise, false. So we can do
```
       if (layerNamePart.Equals("") || layerName.IndexOf(layerNamePart) != -1)
              yield return layer;
```


> Problem:

> `_layerProperties = new PropertyDescriptorCollection(propsArray, true);`

> Error No overload for method 'PropertyDescriptorCollection' takes '2' arguments

> WorkAround:

> `_layerProperties = new PropertyDescriptorCollection(propsArray);`


> Problem:

> `throw new ArgumentOutOfRangeException(string, value, string);`

> Error	No overload for method 'ArgumentOutOfRangeException' takes '3' arguments

> Workaround:

> `throw new ArgumentOutOfRangeException(string, string+" ("+value+") " + string);`




**In SharpMap\Data\Providers\Shapefile\DbaseWriter.cs**
> Problem:

> `_format.Insert(5, decimalPlaces).Insert(3, length);`

> //No second argument of StringBuilder.Insert is neither Byte(decimalPlaces) or int (length)

> Workaround:

> StringBuilder.Insert nserts the string representation of first argument into this
> instance at the specified character position.

> `_format.Insert(5, decimalPlaces.ToString()).Insert(3, length.ToString());`



**In SharpMap\Data\Providers\Shapefile\DbaseFile.cs**
> Problem:

> _dbaseFileStream = new FileStream(_filename, FileMode.OpenOrCreate, FileAccess.ReadWrite,
> exclusive ? FileShare.None : FileShare.Read, 4096, FileOptions.None);
> > //Error no FileStream method takes 6 arguments


> Workaround:

> `_dbaseFileStream = new FileStream(_filename, FileMode.OpenOrCreate,   FileAccess.ReadWrite, exclusive ? FileShare.None : FileShare.Read, 4096);`
> //Omit FileOptions.None argument





**In SharpMap\Data\Providers\Shapefile\DbaseHeader.cs**

> Problem:

> `String colName = header.FileEncoding.GetString(reader.ReadBytes(11)).Replace("\0","").Trim();`
> > //Encoding.GetString takes 3 arguments


> Workaround:

> `byte[.md](.md) brByte = reader.ReadBytes(11);

> String colName = header.FileEncoding.GetString(brByte, 0, brByte.Length).Replace("\0", "").Trim();`




**In SharpMap\Data\Providers\Shapefile\DbaseReader.cs**
> Problem ( like this):
```
 if (double.TryParse(temp, NumberStyles.Float, DbaseConstants.StorageNumberFormat, out dbl))
 {
    return dbl;
 }
 else
 {
    return DBNull.Value;
 }
```
> > //Error no TryParse in CF
> > //The TryParse method is like the Parse method, except the TryParse method does not  throw an exception if the conversion fails.


> Workaround:

```
  try{
         bl = double.Parse(temp, NumberStyles.Float, DbaseConstants.StorageNumberFormat);
         return dbl;
  }
  catch (Exception e){
     return DBNull.Value;
  }
```

**Problem (x like this):**

> `string temp = Encoding.UTF8.GetString(_dbaseReader.ReadBytes(dbf.Length)).Replace("\0", "").Trim();`

> //Encoding.GetString takes 3 arguments

> Workaround:

> `brBytes = _dbaseReader.ReadBytes(dbf.Length);`
> > `string temp = Encoding.UTF8.GetString(brBytes,0,brBytes.Length).Replace("\0","").Trim();`


**In SharpMap\Rendering\Rendering3D\ViewSize3D.cs**

> Problem:

`throw new ArgumentOutOfRangeException("element", element, "Index must be 0, 1 or 2 for a 3D size.");`
> //in cf takes 2 args

> Workaround:

> `throw new ArgumentOutOfRangeException("element","element ("+element+") Index must be 0, 1 or 2 for a 3D size.");`



**In SharpMap\Indexing\RTree\DynamicRTree.cs** **(HIGH)**

**Problem:**

> `_keyFormatter.Serialize(keyBuffer, entry.Value);`

> _keyFormatter Was commented because No System.Runtime.Serialization.Formatters.Binary.BinaryFormatter_

**Problem:**

> `entry.Value = (TValue)_keyFormatter.Deserialize(keyBuffer);`

> _keyFormatter Was commented because No System.Runtime.Serialization.Formatters.Binary.BinaryFormatter_


**In SharpMap\Converters\WellKnownText\WKTStreamTokenizer.cs**

> Problem:

> `Int64.TryParse(this.ReadDoubleQuotedWord(), out authorityCode);`

> No TryParse

> Workaround:

> `authorityCode = Int64.Parse(this.ReadDoubleQuotedWord());`


**In C:\Desarrollo\VisualStudio\proyects\SharpMapCFv2.0\SharpMap\Indexing\RTree\RTree.cs**
> Problem:

> `return (uint)Interlocked.Increment(ref _nextNodeId);`

> Needs an Int32 in CF, but gets a long (Int64)

> Workaround:

> Let's use an Int32 for _nextNodeId_

> `private long _nextNodeId = 0;`

> MaxInt: 2,147,483,647


**In SharpMap\CoordinateSystems\Ellipsoid.cs**
> Problem:

> `sb.AppendFormat(", AUTHORITY[\"{0}\", \"{1}\"]", Authority, AuthorityCode);`
> //In CF no overload takes these args

> Workaround:

> `sb.AppendFormat(null, ", AUTHORITY[\"{0}\", \"{1}\"]", Authority, AuthorityCode);`



**In SharpMap\CoordinateSystems\PrimeMeridian.cs**
> Problem:

> `sb.AppendFormat(", AUTHORITY[\"{0}\", \"{1}\"]", Authority, AuthorityCode);`

> //In CF no overload takes these args

> Workaround:

> `sb.AppendFormat(null, ", AUTHORITY[\"{0}\", \"{1}\"]", Authority, AuthorityCode);`




**In SharpMap\Data\Providers\ProviderSchemaHelper.cs**
> Problem:

> `Array.Exists(keyColumns, delegate(DataColumn col) { return col == column; })`
> //No Array.Exist in CF

> Workaround:
```
                bool isKey = false;
                foreach (DataColumn col in keyColumns){
                    if (col == column){
                        isKey = true;
                        break;
                    }
                }

                schema.Rows.Add(
                    column.ColumnName,
                    length,
                    column.Ordinal,
                    precision,
                    scale,
                    column.DataType,
                    column.AllowDBNull,
                    column.ReadOnly,
                    column.Unique,
                    isKey, //instead of Array.Exists(keyColumns,delegate(DataColumn col){return col==column;})
                    column.AutoIncrement,
                    false);

```





**In SharpMap\Rendering\Rendering2D\Rectangle2D.cs**
> Problem (2 of this kind):

> `throw new ArgumentOutOfRangeException("row", row, "A Rectangle2D has only 2 rows.");`

> //Error No overload for method 'ArgumentOutOfRangeException' takes '3' arguments

> Workaround:

> `throw new ArgumentOutOfRangeException("row", "row ("+row+") A Rectangle2D has only 2 rows.");`




**In SharpMap\Converters\WellKnownText\SpatialReference.cs** **(Medium)**
> Problem:

> `string file = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase)
> > + "\\SpatialRefSys.xml";`


> Workaround:
```
      System.Reflection.AssemblyName asn = System.Reflection.Assembly.GetExecutingAssembly().GetName();
      String codebase = asn.CodeBase.Substring(0, asn.CodeBase.LastIndexOf(asn.Name));
      string file = System.IO.Path.GetDirectoryName(codebase + "\\SpatialRefSys.xml");
```





**In SharpMap\Data\Providers\Shapefile\ShapeFileProvider.cs**
> Problem:
> > `CultureInfo culture = Thread.CurrentThread.CurrentCulture;`


> // culture info is not bound to a thread but to the device


> Workaround:

> ////How to: Get Culture Information http://msdn2.microsoft.com/en-us/library/ms172501.aspx
```
    System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
    CultureInfo culture = asm.GetName().CultureInfo;
```


**Problem:** **(Medium)**

> `Encoding encoding = Encoding.GetEncoding(culture.TextInfo.OEMCodePage);`

> //TextInfo has no OEMCodePage property

> Workaround:  **(NOT SURE OF THIS)**

> `Encoding encoding = Encoding.Unicode;`



**Problem:**
```
 if (layerName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0){
 	throw new ArgumentException("Parameter cannot have invalid filename characters", "layerName");
 }
```
> //Error, no GetInvalidFileNameChars() method for Path class

> Workaround:
```
 char[] badset = new char[41];
 for (int i = 0; i < 32; i++)
     badset[i] = (char)i;
 badset[32] = '"';
 badset[33] = '<';
 badset[34] = '>';
 badset[35] = '|';
 badset[36] = '?';
 badset[37] = ':';
 badset[38] = '/';
 badset[39] = '\\';
 badset[39] = '*';
  if (layerName.IndexOfAny(badset) >= 0)
     throw new ArgumentException("Parameter cannot have invalid filename characters", "layerName");
```

> //Invalid Filename Characters in CF http://www.z-space.com/kb/Article.aspx?ID=10145




**Problem**
> `string wkt = File.ReadAllText(projfile);`

> //Error no ReadAllText method for File class in CF

> Workaround:
```
  StreamReader sr = new StreamReader(projfile);
  string wkt = sr.ReadToEnd();
  sr.Close();

```




**Problem:**
> `Trace.TraceWarning("Coordinate system file '" + projfile+ "' found, but could not be parsed. WKT parser returned:" + ex.Message);`
> //Error, no TraceWarning for CF
> Workaround:

> `Trace.Assert(false, "Coordinate system file '" + projfile+ "' found, but could not be parsed. WKT parser returned:" + ex.Message);`



**Problem:**
> `_shapeFileStream = new FileStream(Filename, FileMode.OpenOrCreate, FileAccess.ReadWrite,exclusive ? FileShare.None : FileShare.Read, 4096, FileOptions.None);`

> //Error no FileStream? method takes 6 arguments

> Workaround:

> `_shapeFileStream = new FileStream(Filename, FileMode.OpenOrCreate, FileAccess.ReadWrite,exclusive ? FileShare.None : FileShare.Read, 4096);`



**In SharpMap\Utilities\IdleMonitor.cs**
> Problem:
> > `private EventWaitHandle _terminateEvent;`
> > //Error, no Threading.EventWaitHandle in CF

> Workaround:
> > OPENNETCF has this, and BackgroundWorker.
> > Added to /CFUtils folders /ComponentModel and /Threading


> `private OpenNETCF.Threading.EventWaitHandle _terminateEvent;`



**Problem:**
> (now _terminateEvent is an OpenNETCF.Threading.EventWaitHandle)_

> `_terminateEvent = new ManualResetEvent(false);`

> Workaround:

> `_terminateEvent = new OpenNETCF.Threading.EventWaitHandle(false, OpenNETCF.Threading.EventResetMode.ManualReset);`

> //In the .NET Framework version 2.0, ManualResetEvent derives from the new EventWaitHandle class. A ManualResetEvent is functionally equivalent to an EventWaitHandle created with EventResetMode.ManualReset (msdn)


**Problem (2 like this one):**
> `throw new ArgumentOutOfRangeException("value", value, "UserIdleThresholdInSeconds cannot be negative.");`

> Workaround:

> `throw new ArgumentOutOfRangeException("value","value ("+value+") UserIdleThresholdInSeconds cannot be negative.");`










## NOT Resolved ##

TO BE RESOLVED


**In SharpMap\Presentation\Presenters\MapPresenter2D.cs**
> Issue http://code.google.com/p/sharpmapcf/issues/detail?id=1

**Problem:**
> private void handleLayersChanged(object sender, ListChangedEventArgs e){..}
> is called when a layer (in a list) is called. But ListChangedEventArgs has no PropertyDescriptor, so how can be know which layer changed?
```
    case ListChangedType.ItemChanged:
        if(e.PropertyDescriptor.Name == Layer.EnabledProperty.Name)
        {
            renderAllLayers();   
        }
        break;
```


> Workaround:

> Let's always render all layers by now.



**In SharpMap\Presentation\Presenters\LayersPresenter.cs** **(Medium)**
> Issue http://code.google.com/p/sharpmapcf/issues/detail?id=1
> Problem:
> `if(e.PropertyDescriptor.Name == Layer.EnabledProperty.Name)`, where e is ListChangedEventArgs

> //ListChangedEventArgs has no PropertyDescriptor

> Workaround:

```
   ILayer layer = Map.Layers[e.NewIndex];
   if (layer.Enabled)
   {
      View.EnableLayer(layer.LayerName);
   }
   else
   {
       View.DisableLayer(layer.LayerName);
   }
```

> //Not good but this does no harm





**In SharpMap\Indexing\RTree\SelfOptimizingSpatialIndex.cs**

**Problem:**
```
 private readonly EventWaitHandle _userIdleEvent;
 private readonly EventWaitHandle _machineIdleEvent;
 private readonly EventWaitHandle _terminateEvent;
```
> //No EventWaitHandle in CF

> Workaround:
> //We use that provided in OpenNETCF
```
        private readonly OpenNETCF.Threading.EventWaitHandle _userIdleEvent;
        private readonly OpenNETCF.Threading.EventWaitHandle _machineIdleEvent;
        private readonly OpenNETCF.Threading.EventWaitHandle _terminateEvent;
```

**Problem:**
> As result of previous problem, these lines wont compile
```
  _userIdleEvent = new AutoResetEvent(false);
  _machineIdleEvent = new AutoResetEvent(false);
  _terminateEvent = new ManualResetEvent(false);
```

> Workaround:
```
  _userIdleEvent = new OpenNETCF.Threading.EventWaitHandle(false,OpenNETCF.Threading.EventResetMode.AutoReset);
  _machineIdleEvent = new OpenNETCF.Threading.EventWaitHandle(false,OpenNETCF.Threading.EventResetMode.AutoReset);
  _terminateEvent = new OpenNETCF.Threading.EventWaitHandle(false, OpenNETCF.Threading.EventResetMode.ManualReset);
```
> //In the .NET Framework version 2.0, AutoResetEvent derives from the new EventWaitHandle class. An AutoResetEvent is functionally equivalent to an EventWaitHandle created with EventResetMode.AutoReset
> //A ManualResetEvent is functionally equivalent to an EventWaitHandle created with EventResetMode.ManalReset


**Problem:**
> `WaitHandle.WaitAny(events, _periodMilliseconds, false);`
> > //There's no WaitAny for WaitHandle in CF


> Workaround: **Is this equivalent??**
```
   IntPtr[] lpHandles = { _userIdleEvent.Handle, _machineIdleEvent.Handle, _terminateEvent.Handle };
   OpenNETCF.Threading.NativeMethods.WaitForMultipleObjects(3, lpHandles, false, (uint)_periodMilliseconds);                
```


**Problem:**
```
  if (_restructureThread.IsAlive && !_restructureThread.Join(5000)){
	_restructureThread.Abort();
   }
```

> Workaround:  **Is this equivalent??**
```
   if (!_restructureThread.Join(5000)){
      _restructureThread.Abort();
   }
```
> > //Join(int) returns true if the thread has terminated; false if the thread has not terminated after the amount of time specified by the millisecondsTimeout parameter has elapsed.



**In SharpMap\Utilities\IdleMonitor.cs**

**Problem:**  **Not Sure of this**
```
            if (_pollIdleThread.IsAlive && !_pollIdleThread.Join(5000)){
                _pollIdleThread.Abort();
            }
```

> //No IsAlive in CF

> Workaround:
```
            if (!_pollIdleThread.Join(5000))
            {
                _pollIdleThread.Abort();
            }
```





**In SharpMap\Rendering\Rendering2D\BasicGeometryRenderer2D`1.cs** **(High, wrong)**
> Changed get of public static Symbol2D DefaultSymbol property by something quick and ugly (method because no VolatileRead/Write on CF):
```
get
{
   if (Thread.VolatileRead(ref _defaultSymbol) == null)
   {
	lock (_defaultSymbolSync)
	{
		if (Thread.VolatileRead(ref _defaultSymbol) == null)
		{
			Stream data = Assembly.GetExecutingAssembly()
				.GetManifestResourceStream("SharpMap.Styles.DefaultSymbol.png");
                        Symbol2D symbol = new Symbol2D(data, new Size2D(16, 16));
			Thread.VolatileWrite(ref _defaultSymbol, symbol);
		}
	}
   }

   return (_defaultSymbol as Symbol2D).Clone();
}
```



**In SharpMap\Utilities\IdleMonitor.cs** **(High wrong)**
> Changed
> `private void checkIdleness()` method because no VolatileRead/Write on CF

for this ugly thing:
```
  private void checkIdleness(){
       while (_terminating == 0){
           int userIdleThreshold =  _userIdleThresholdSeconds;
           if (IsUserIdle(userIdleThreshold))
                    onUserIdle();
           else
                   onUserBusy();

           int machineIdleThreshold = _machineIdleThresholdSeconds;

           if (IsMachineIdle(machineIdleThreshold, MachineUtilizationConsideredIdle))
                    onMachineIdle();
           else
                 onMachineBusy();

           int sleepTime = _checkIdleFrequencyInSeconds;
           Thread.Sleep(sleepTime * 1000);
      }
   }

```

**In SharpMap\Features\FeatureDataView.cs** **(High, probably wrong)**
> Problem:

> `object rowPredicateFilter = Activator.CreateInstance(rowPredicateFilterType,BindingFlags.Instance | BindingFlags.NonPublic, null, args, null);`
> //In CF CreateInstance only takes 1 parameter

> Workaround:
> > `object rowPredicateFilter = Activator.CreateInstance(rowPredicateFilterType);`


**In SharpMap\Features\FeatureDataView.cs** **(Very High)**

> Commented out method

> `private static SetIndex2Delegate GenerateSetIndex2Delegate()`



