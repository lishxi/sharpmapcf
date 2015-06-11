# Introduction #

These are the steps followed trying to port SharpMap v2.0 to the .NET Compact Framewok. It's based on the work made by Roger Bedell for the older version of SharpMap.
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
  * Removing files/unsuported features
  * Common part with the older port
  * New features to update

## External references ##
References to assemblies to add to the VisualStudio project:

  * System.Drawing
  * System.Data.SqlClient
  * NetTopologySuite
  * ProjNET
  * OGR (for SharpMap.Extensions\Data\Providers\OgrProvider.cs: Using OSGeo.OGR)
  * GeoAPI for Classes SharpMap.Converters.NTS.GeometryConverter (SharpMap.Extensions\Data\Providers\NtsProvider.cs) and SharpMap.Data.Providers.NtsProvider (SharpMap.Extensions\Data\Providers\NtsGeometryConverter.cs)
  * GDAL (for SharpMap.Extensions\Layers\GdalRasterLayer.cs) (?)
> > Add "Using OSGeo.GDAL"
> > Change all references from GDAL.xxx to OSGeo.GDAL.XXX

## Removing files/unsuported features ##
Files that will be removed:

  * SharpMap.Data.Providers.Oracle.cs: SharpMap.Extensions\Data\Providers\Oracle.cs No Oracle client in CF.
  * System.Web doesn't exist in CF. A number of items depend on this.
    * SharpMap\Web\HttpHandler.cs.
    * SharpMap\Web\Wms\WmsServer.cs
    * SharpMap\Web\Wms\WmsException.cs
  * SharpMap\Utilities\Surrogates.cs (No System.Runtime.Serialization)
  * SharpMap\Data\Providers\OleDbPoint.cs Had to remove OleDB provider, no System.Data.OleDb in CF

## Common part with the older port ##
Commenting all "[Serializable](Serializable.md)" appearances in
  * SharpMap.Geometries.BoundingBox.cs
  * SharpMap.Geometries.Point.cs
  * SharpMap.Data.Providers.PostGIS.cs
  * SharpMap.Data.FeatureDataSet.cs
  * SharpMap.Geometries.LineString.cs
  * SharpMap.Geometries.Point3D.cs
  * SharpMap.Geometries.MultiLineString.cs
  * SharpMap.Utilities.Indexing.BinaryTree.cs
  * SharpMap.Data.Providers.MsSql.cs
  * SharpMap.Data.Providers.MsSqlSpatial.cs
  * SharpMap.Data.Providers.PostGIS2.cs
  * SharpMap.Geometries.MultiPolygon.cs
  * SharpMap.Geometries.Polygon.cs
  * SharpMap.Styles.Style.cs
  * SharpMap.Web.Wms.Client.cs



**In VectorRenderer.cs:**

> Substitute `DrawLabel(Graphics, PointF,...)` for a compatible version
#  #
> Substitute `clipPolygon(PointF,...)` for a compatible version
#  #
> Substitute `void DrawPoint(,,,PointF,..)` for a compatible version (REVISAR)
#  #
> Substitute `void DrawMultiPoint(,,,PointF,..)` for a compatible version (NEW)


**In SharpMap.Geometries.LineString.cs:**
> Substitute `PointF[] TransformToImage(Map)` for a compatible version
> > > This one depends on SharpMap.Utilities.Transform.WorldtoMap


**In SharpMap.Utilities.Transform.cs:**

> Substitute `PointF WorldtoMap(Point, Map)` for a compatible version
#  #
> Substitute `PointF MapToWorld(PointF, Map)` for a compatible version

**In SharpMap.Geometries.Point.cs:**
> Substitute `public System.Drawing.PointF TransformToImage(Map map)` for a compatible version


**In SharpMap.Data.Providers.OgrProvider.cs:**
> Change `Using OGR` for `Using OSGeo.OGR`; and all references to OGR for `OSGeo.OGR`

**In SharpMap.Layers.VectorLayer.cs:**
> Comment `private System.Drawing.Drawing2D.SmoothingMode _SmoothingMode;`
> No System.Drawing.Drawing2D in CF

**In SharpMap.Data.FeatureDataSet.cs:**
> Lost `protected FeatureDataSet(SerializationInfo info, StreamingContext context) `
#  #
> Commented Attribute `[System.ComponentModel.Browsable(false)]` of `Count` property. "//No Browsable in CF. Lost functionality?"



**In SharpMap.Styles.VectorStyle.cs**
> Change `_SymbolOffset` from PointF to Point
#  #
> Change `System.Drawing.Brush Fill`

**In SharpMap.Rendering.Label.cs**
> Change `public Label(string text, System.Drawing.PointF labelpoint, float rotation, int priority, LabelBox collisionbox, SharpMap.Styles.LabelStyle style)`
#  #
> > to     `public Label(string text, System.Drawing.Point labelpoint, float rotation, int priority, LabelBox collisionbox, SharpMap.Styles.LabelStyle style)`
#  #

> Change `_LabelPoint` from PointF to Point

**In SharpMap.Geometries.Polygon.cs:**
> Change `public System.Drawing.PointF[] TransformToImage(SharpMap.Map map)`
> to     `public System.Drawing.Point[] TransformToImage(SharpMap.Map map)`

**In SharpMap.Rendering.Thematics.ColorBlend.cs:**
> In ColorBlend.GetColor() change
> > `return Color.FromArgb(A, R, G, B);`

> for
> > `return System.Drawing.Color.FromArgb((int)(R * 0x10000 + G * 0x100 + B));`
**To be revised**
#  #

> Commented `public System.Drawing.Drawing2D.LinearGradientBrush ToBrush(Rectangle rectangle, float angle)`
> //No LinearGradientBrush in CF Lost Functionality


**In SharpMap.Styles.LabelStyle.cs:**
> Change `private System.Drawing.PointF _Offset;`
for  		 `private System.Drawing.Point _Offset;`
#  #
> Change `public System.Drawing.PointF Offset`
for 		 `public System.Drawing.Point Offset`
#  #
> Change `private System.Drawing.SizeF _CollisionBuffer;`
#  #
for `private System.Drawing.Size _CollisionBuffer;`  //System.Drawing.SizeF exists in CF

**In SharpMap.Layers.LabelLayer.cs:**
> Commented
#  #
> > `private System.Drawing.Drawing2D.SmoothingMode _SmoothingMode;`
#  #
> > `public System.Drawing.Drawing2D.SmoothingMode SmoothingMode`
#  #
> > > `private System.Drawing.Text.TextRenderingHint _TextRenderingHint;`
#  #
> > > `public System.Drawing.Text.TextRenderingHint TextRenderingHint`
#  #

> And lines which uses those.
#  #
> Commented
#  #
> > `float.TryParse(feature[this.RotationColumn].ToString(), System.Globalization.NumberStyles.Any,SharpMap.Map.numberFormat_EnUS, out rotation);`
#  #

> Changed `CreateLabel(...)`


**In SharpMap.Layers.WmsLayer.cs:**
> Totally commented (To be revised)

**In SharpMap.Map.Map.cs:**
> Commented `_MapTransform` and `MapTransformInverted`
#  #
> Changed `public System.Drawing.PointF WorldToImage(SharpMap.Geometries.Point p)`
#  #
> > to      `public System.Drawing.Point WorldToImage(SharpMap.Geometries.Point p)`
#  #

> Changed `public SharpMap.Geometries.Point ImageToWorld(System.Drawing.PointF p)`
#  #
> to      `public SharpMap.Geometries.Point ImageToWorld(System.Drawing.Point p)`
#  #
> Commented `g.Transform = this.MapTransform;`
#  #
> and       `g.PageUnit = System.Drawing.GraphicsUnit.Pixel;`
#  #
> Changed `if (l.LayerName.Contains(layername))`
#  #
> for     `if (l.LayerName.IndexOf(layername) != -1)`
#  #
> Commented 	`public System.Drawing.Drawing2D.Matrix MapTransform`
> cause no System.Drawing.Drawing2D.Matrix in CF

**In SharpMap.Layers.VectorLayer.cs:**
> disabled `private System.Drawing.Drawing2D.SmoothingMode _SmoothingMode;`
#  #
> > `SmoothingMode`


## New features to update ##

**SharpMap.Extensions/Data/Providers/PostGIS.cs has been fully commented**
**SharpMap.Extensions/Data/Providers/PostGIS2.cs has been fully commented**

**SharpMap.Web.Wms.Capabilities.cs:**

> Commented the method
> > `private static XmlNode GenerateCapabilityNode(SharpMap.Map map, XmlDocument capabilities)` (No System.Web)


> Commented the call to previous method in `GetCapabilities()`


**SharpMap.Utilities.SpatialIndexing.cs:**
```
		public static QuadTree FromFile(string filename){ 
				...
				fs.Dispose(); //Cannot access method "Dispose" here due its protection level
		}
```

**SharpMap.Rendering.VectorRenderer.cs:**
> `private static System.Drawing.Bitmap defaultsymbol`

**SharpMap.Web.Cahe.cs:**
> Commented its only method `InsertIntoCache()`

**SharpMap.Web.Wms.Client.cs:**
> Changes to `public Client(string url, System.Net.WebProxy proxy)`
#  #
> > `strReq.AppendFormat("SERVICE=WMS&");` changed for
#  #
> > > `strReq.Append("SERVICE=WMS&");` (Only StringBuilder.AppendFormat(System.IFormatProvider provider, string format, params object[.md](.md) args) in CF)

**SharpMap.Utilities.Indexing.BinaryTree.cs:**

> Commented `Trace.WriteLine(root.Item.ToString()); in TraceInOrder(Node<T, U> root)`
#  #
> Changed `Array.ForEach(items, Add);` in  `Add(params ItemValue[] items)`
> for:
```
		     foreach (ItemValue item in items)
		         Add(item);
```

**SharpMap.Extensions/Layers/GdalRasterLayer.cs:**
> Change all references from OSGeo.GDAL.gdal.xxx  to OSGeo.GDAL.Gdal
> > (as defined in library SharpMap2\ExternalReferences\References4SharpMap.Extensions\gdal\_csharp.dll)

**Commented all Namespace**

**SharpMap.Converters.WellKnownText.WtkStreamTokenizer.cs:**

> Changed `long.TryParse(this.ReadDoubleQuotedWord(),out authorityCode);`	No TryParse on CF
#  #
**To be revised**:
> > `TryParse(String s, out double value)` returns true if s was converted successfully; otherwise, false. But can we be sure that an exception is thrown if s is not converted successfully?

**SharpMap.Converters.WellKnownText.SpatialReference.cs:**
**To be revised**: Changed `SridToWkt(int srid)` Not sure if equivalent

**SharpMap/UnitTests/TestWmsCapabilityParser.cs**

> Commented `AddLayerFail()` and `AddLayerOK()`

**SharpMap.Web.Wms.Client.cs:**
> Changed in `ParseLayer()` all calls to double.TryParse;	No TryParse on CF
#  #
**To be revised**:
> > TryParse(String s, out double value) returns true if s was converted successfully; otherwise, false.	 But can we be sure that an exception is thrown if s is not converted successfully?

**SharpMap.Utilities.Providers.cs:**
**To be revised** Commented `public static Collection<Type> GetProviders()`



**SharpMap.Data.Providers.Shapefile.cs:**

> Comment all code that references System.Web
#  #
> Commented **BUT NOT SOLVED**:
> ´System.Diagnostics.Trace.TraceWarning("Coordinate system file '" + projfile + "' found, but could not be parsed. WKT parser returned:" + ex.Message);´

**Changes made to these lines have to be revised**
> `string wkt = System.IO.File.ReadAllText(projfile);`
> `heur.maxdepth = (int)Math.Ceiling(Math.Log(this.GetFeatureCount(), 2));`










































