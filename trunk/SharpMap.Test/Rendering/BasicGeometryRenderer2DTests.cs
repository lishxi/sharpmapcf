using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpMap.Data;
using SharpMap.Geometries;
using SharpMap.Rendering;
using SharpMap.Rendering.Rendering2D;
using SharpMap.Styles;
using IVectorD = NPack.Interfaces.IVector<NPack.DoubleComponent>;
using IMatrixD = NPack.Interfaces.IMatrix<NPack.DoubleComponent>;
using ITransformMatrixD = NPack.Interfaces.ITransformMatrix<NPack.DoubleComponent>;

namespace SharpMap.Tests.Rendering
{
	#region BasicGeometryRenderer2D

	[TestFixture]
	public class BasicGeometryRenderer2DTests
	{
        #region Test stub types
        private struct RenderObject
        {
            public double[][] RenderedPaths;
        }

        private class TestVectorRenderer : VectorRenderer2D<RenderObject>
        {
            public override IEnumerable<RenderObject> RenderPaths(IEnumerable<GraphicsPath2D> paths, 
                StylePen outline, StylePen highlightOutline, StylePen selectOutline)
            {
            	foreach (GraphicsPath2D path in paths)
				{
					RenderObject ro = new RenderObject();

					ro.RenderedPaths = new double[path.Figures.Count][];

					for (int figureIndex = 0; figureIndex < path.Figures.Count; figureIndex++)
					{
						ro.RenderedPaths[figureIndex] = new double[path.Figures[figureIndex].Points.Count * 2];

						for (int pointIndex = 0; pointIndex < path.Figures[figureIndex].Points.Count; pointIndex++)
						{
							Point2D point = path.Figures[figureIndex].Points[pointIndex];
							ro.RenderedPaths[figureIndex][pointIndex * 2] = point.X;
							ro.RenderedPaths[figureIndex][pointIndex * 2 + 1] = point.Y;
						}
					}

					yield return ro;
            	}
            }

			public override IEnumerable<RenderObject> RenderPaths(IEnumerable<GraphicsPath2D> paths, 
                StyleBrush fill, StyleBrush highlightFill, StyleBrush selectFill, StylePen outline, 
                StylePen highlightOutline, StylePen selectOutline)
            {
				foreach (GraphicsPath2D path in paths)
				{
					RenderObject ro = new RenderObject();

					ro.RenderedPaths = new double[path.Figures.Count][];

					for (int figureIndex = 0; figureIndex < path.Figures.Count; figureIndex++)
					{
						ro.RenderedPaths[figureIndex] = new double[path.Figures[figureIndex].Points.Count * 2];

						for (int pointIndex = 0; pointIndex < path.Figures[figureIndex].Points.Count; pointIndex++)
						{
							Point2D point = path.Figures[figureIndex].Points[pointIndex];
							ro.RenderedPaths[figureIndex][pointIndex * 2] = point.X;
							ro.RenderedPaths[figureIndex][pointIndex * 2 + 1] = point.Y;
						}
					}

					yield return ro;
				}
            }

			public override IEnumerable<RenderObject> RenderSymbols(IEnumerable<Point2D> locations, Symbol2D symbolData)
			{
				foreach (Point2D point in locations)
				{
					RenderObject ro = new RenderObject();

					ro.RenderedPaths = new double[][] { new double[] { point.X, point.Y } };

					yield return ro;
				}
            }

			public override IEnumerable<RenderObject> RenderSymbols(IEnumerable<Point2D> locations, 
                Symbol2D symbolData, ColorMatrix highlight, ColorMatrix select)
			{
				foreach (Point2D point in locations)
				{
					RenderObject ro = new RenderObject();

					ro.RenderedPaths = new double[][] { new double[] { point.X, point.Y } };

					yield return ro;
				}
            }

			public override IEnumerable<RenderObject> RenderSymbols(IEnumerable<Point2D> locations, Symbol2D symbolData, Symbol2D highlightSymbolData,
                                                      Symbol2D selectSymbolData)
            {
				foreach (Point2D point in locations)
				{
					RenderObject ro = new RenderObject();

					ro.RenderedPaths = new double[][] { new double[] { point.X, point.Y } };

					yield return ro;
				}
            }
        } 
        #endregion

		[Test]
		public void RenderFeatureTest()
		{
			IFeatureLayerProvider provider = DataSourceHelper.CreateGeometryDatasource();
			TestVectorRenderer vectorRenderer = new TestVectorRenderer();
            Matrix2D toView = new Matrix2D(10, 0, 0, 10, 0, 0);
            BasicGeometryRenderer2D<RenderObject> geometryRenderer = new BasicGeometryRenderer2D<RenderObject>(vectorRenderer, toView);

			FeatureDataTable features = new FeatureDataTable();
			provider.ExecuteIntersectionQuery(provider.GetExtents(), features);

			foreach (FeatureDataRow feature in features)
			{
				Geometry g = feature.Geometry;
				List<RenderObject> renderedObjects = new List<RenderObject>(geometryRenderer.RenderFeature(feature));

                for (int i = 0; i < renderedObjects.Count; i++)
                {
                    RenderObject ro = renderedObjects[i];
                }
			}
		}

		[Test]
		[Ignore("Test not yet implemented")]
		public void DrawMultiLineStringTest()
		{
		}

		[Test]
		[Ignore("Test not yet implemented")]
		public void DrawLineStringTest()
		{
		}

		[Test]
		[Ignore("Test not yet implemented")]
		public void DrawMultiPolygonTest()
		{
		}

		[Test]
		[Ignore("Test not yet implemented")]
		public void DrawPolygonTest()
		{
		}

		[Test]
		[Ignore("Test not yet implemented")]
		public void DrawPointTest()
		{
		}

		[Test]
		[Ignore("Test not yet implemented")]
		public void DrawMultiPointTest()
		{
		}

		[Test]
		[Ignore("Test not yet implemented")]
		[ExpectedException(typeof(NotSupportedException))]
		public void UnsupportedGeometryTest()
		{
		}
	}

	#endregion
}