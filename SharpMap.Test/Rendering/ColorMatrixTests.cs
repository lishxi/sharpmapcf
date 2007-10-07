using System;
using NPack;
using NPack.Interfaces;
using NUnit.Framework;

using SharpMap.Rendering;

namespace SharpMap.Tests.Rendering
{
    #region ColorMatrix
    [TestFixture]
    public class ColorMatrixTests
    {
        private static readonly double _e = 0.0005;

        [Test]
        public void ResetTest()
        {
            ColorMatrix m1 = new ColorMatrix(1, 1, 1, 1, 0, 0, 0);

            m1.Reset();

            Assert.AreEqual(m1, ColorMatrix.Identity);
        }

        [Test]
        public void InvertTest()
        {
            ColorMatrix m1 = new ColorMatrix(0.5, 0.5, 0.5, 0.5, 10, 20, 30);
            IMatrix<DoubleComponent> expected = new Matrix<DoubleComponent>(MatrixFormat.RowMajor, new DoubleComponent[][]
                {
                   new DoubleComponent[] {2, 0, 0, 0, 0}, 
                   new DoubleComponent[] {0, 2, 0, 0, 0}, 
                   new DoubleComponent[] {0, 0, 2, 0, 0}, 
                   new DoubleComponent[] {0, 0, 0, 2, 0}, 
                   new DoubleComponent[] {-20, -40, -60, 0, 1}
                });

            IMatrix<DoubleComponent> m1Inverse = m1.Inverse;

            for (int i = 0; i < m1.RowCount; i++)
            {
                for (int j = 0; j < m1.ColumnCount; j++)
                {
                    Assert.AreEqual((double)expected[i, j], (double)m1Inverse[i, j], _e);
                }
            }
        }

        [Test]
        public void IsInvertableTest()
        {
            ColorMatrix m1 = new ColorMatrix(1, 1, 1, 1, 0, 0, 0);
            Assert.IsTrue(m1.IsInvertible);
        }

        [Test]
        public void ElementsTest1()
        {
            ColorMatrix m1 = ColorMatrix.Identity;
            ColorMatrix m2 = new ColorMatrix(0.5, 0.5, 0.5, 1, 0, 0, 0);

            Assert.AreEqual(5, m1.RowCount);
            Assert.AreEqual(5, m2.ColumnCount);

            DoubleComponent[][] expected = new DoubleComponent[][] 
                { 
                    new DoubleComponent[] { 0.5, 0, 0, 0, 0 }, 
                    new DoubleComponent[] { 0, 0.5, 0, 0, 0 }, 
                    new DoubleComponent[] { 0, 0, 0.5, 0, 0 }, 
                    new DoubleComponent[] { 0, 0, 0, 1, 0 }, 
                    new DoubleComponent[] { 0, 0, 0, 0, 1 } 
                };

            DoubleComponent[][] actual = m2.Elements;

            Assert.AreEqual(expected[0][0], actual[0][0]);
            Assert.AreEqual(expected[0][1], actual[0][1]);
            Assert.AreEqual(expected[0][2], actual[0][2]);
            Assert.AreEqual(expected[1][0], actual[1][0]);
            Assert.AreEqual(expected[1][1], actual[1][1]);
            Assert.AreEqual(expected[1][2], actual[1][2]);
            Assert.AreEqual(expected[2][0], actual[2][0]);
            Assert.AreEqual(expected[2][1], actual[2][1]);
            Assert.AreEqual(expected[2][2], actual[2][2]);

            m1.Elements = expected;
            Assert.AreEqual(m1, m2);
            Assert.IsTrue(m1.Equals(m2 as IMatrix<DoubleComponent>));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ElementsTest2()
        {
            ColorMatrix m1 = ColorMatrix.Identity;
            m1.Elements = null;
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ElementsTest3()
        {
            ColorMatrix m1 = ColorMatrix.Identity;
            m1.Elements = new DoubleComponent[][]
                {
                    new DoubleComponent[] { 1, 2, 3 }, 
                    new DoubleComponent[] { 2, 3, 4 }
                };
        }

        [Test]
        [Ignore("Test not yet implemented")]
        public void MultiplyTest()
        {
            ColorMatrix m1 = ColorMatrix.Identity;
        }

        [Test]
        [Ignore("Test not yet implemented")]
        public void ScaleTest1()
        {
            ColorMatrix m1 = new ColorMatrix(0, 0, 0, 0, 0, 0, 0);
            ColorMatrix m2 = ColorMatrix.Identity;
        }

        [Test]
        [Ignore("Test not yet implemented")]
        public void ScaleTest2()
        {
            ColorMatrix m1 = ColorMatrix.Identity;

            // Scale by a vector for which multiplication isn't defined...
        }

        [Test]
        [Ignore("Test not yet implemented")]
        public void TranslateTest1()
        {
            ColorMatrix m1 = ColorMatrix.Identity;
        }

        [Test]
        [Ignore("Test not yet implemented")]
        public void TranslateTest2()
        {
            ColorMatrix m1 = ColorMatrix.Identity;
            // Scale by a vector for which multiplicatio isn't defined...
        }

        [Test]
        [Ignore("Test not yet implemented")]
        public void TransformTest1()
        {
            ColorMatrix m1 = ColorMatrix.Identity;
        }

        [Test]
        [Ignore("Test not yet implemented")]
        public void Transform2Test2()
        {
            ColorMatrix m1 = ColorMatrix.Identity;
            // Scale by a vector for which multiplicatio isn't defined...
        }
    }
    #endregion
}
