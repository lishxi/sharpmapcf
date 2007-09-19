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
using System.Collections.Generic;
using System.Text;

using SharpMap.Utilities;
using System.Globalization;

namespace SharpMap.Geometries
{
    /// <summary>
    /// Bounding box type with double precision.
    /// </summary>
    /// <remarks>
    /// The BoundingBox represents a 2D box whose sides are parallel to the two axes of the coordinate system.
    /// </remarks>
    [Serializable]
    public struct BoundingBox : IEquatable<BoundingBox>
    {
        private static readonly BoundingBox _empty = new BoundingBox();

        /// <summary>
        /// Gets an empty BoundingBox.
        /// </summary>
        public static BoundingBox Empty
        {
            get { return _empty; }
        }

        private double _xMin, _yMin, _xMax, _yMax;
        private bool _hasValue;

        #region Constructors
        /// <summary>
        /// Initializes a bounding box.
        /// </summary>
        /// <remarks>
        /// In case min values are larger than max values, the parameters will be swapped to ensure correct min/max boundary.
        /// </remarks>
        /// <param name="minX">left</param>
        /// <param name="minY">bottom</param>
        /// <param name="maxX">right</param>
        /// <param name="maxY">top</param>
        public BoundingBox(double minX, double minY, double maxX, double maxY)
        {
            _xMin = minX;
            _yMin = minY;
            _xMax = maxX;
            _yMax = maxY;
            _hasValue = true;
            checkMinMax();
        }

        /// <summary>
        /// Initializes a bounding box.
        /// </summary>
        /// <param name="lowerLeft">Lower left corner.</param>
        /// <param name="upperRight">Upper right corner.</param>
        public BoundingBox(Point lowerLeft, Point upperRight)
            : this(0, 0, 0, 0)
        {
            _hasValue = true;

            if (lowerLeft == null || lowerLeft.IsEmpty() || upperRight == null || upperRight.IsEmpty())
            {
                return;
            }

            _xMin = lowerLeft.X;
            _yMin = lowerLeft.Y;
            _xMax = upperRight.X;
            _yMax = upperRight.Y;
            checkMinMax();
        }

        /// <summary>
        /// Initializes a bounding box.
        /// </summary>
        /// <param name="boxes">BoundingBox instances to join.</param>
        public BoundingBox(params BoundingBox[] boxes)
            : this(0, 0, 0, 0)
        {
            _hasValue = false;

            if (boxes == null || boxes.Length == 0)
            {
                return;
            }

            foreach (BoundingBox box in boxes)
            {
                ExpandToInclude(box);
            }
        }

        /// <summary>
        /// Initializes a new BoundingBox based on the bounds from a set of geometries.
        /// </summary>
        /// <param name="objects">List of <see cref="Geometry"/> objects to compute the BoundingBox for.</param>
        public BoundingBox(IEnumerable<Geometry> objects)
            : this(0, 0, 0, 0)
        {
            _hasValue = false;

            if (objects == null)
            {
                return;
            }

            checkMinMax();

            foreach (Geometry g in objects)
            {
                ExpandToInclude(g);
            }
        }

        /// <summary>
        /// Initializes a new BoundingBox based on the bounds from a set of bounding boxes.
        /// </summary>
        /// <param name="objects">list of <see cref="BoundingBox"/> objects to compute the BoundingBox for.</param>
        public BoundingBox(IEnumerable<BoundingBox> objects)
            : this(0, 0, 0, 0)
        {
            _hasValue = false;

            if (objects == null)
            {
                return;
            }

            foreach (BoundingBox box in objects)
            {
                ExpandToInclude(box);
            }
        }
        #endregion Constructors

        #region Metrics Properties

        /// <summary>
        /// Gets or sets the lower left corner.
        /// </summary>
        public Point Min
        {
            get
            {
                if (IsEmpty)
                {
                    return Point.Empty;
                }

                return new Point(_xMin, _yMin);
            }
            private set
            {
                if (value == null)
                {
                    IsEmpty = true;
                    return;
                }

                _xMin = value.X;
                _yMin = value.Y;
            }
        }

        /// <summary>
        /// Gets or sets the upper right corner.
        /// </summary>
        public Point Max
        {
            get
            {
                if (IsEmpty)
                {
                    return Point.Empty;
                }

                return new Point(_xMax, _yMax);
            }
            private set
            {
                if (value == null)
                {
                    IsEmpty = true;
                    return;
                }

                _xMax = value.X;
                _yMax = value.Y;
            }
        }

        /// <summary>
        /// Gets the lower left corner.
        /// </summary>
        public Point LowerLeft
        {
            get { return Min; }
        }

        /// <summary>
        /// Gets the lower right corner.
        /// </summary>
        public Point LowerRight
        {
            get
            {
                if (IsEmpty)
                {
                    return Point.Empty;
                }

                return new Point(_xMax, _yMin);
            }
        }

        /// <summary>
        /// Gets the upper left corner.
        /// </summary>
        public Point UpperLeft
        {
            get
            {
                if (IsEmpty)
                {
                    return Point.Empty;
                }

                return new Point(_xMin, _yMax);
            }
        }

        /// <summary>
        /// Gets the upper right corner.
        /// </summary>
        public Point UpperRight
        {
            get { return Max; }
        }

        /// <summary>
        /// Returns true if BoundingBox is empty, false otherwise.
        /// </summary>
        public bool IsEmpty
        {
            get { return !_hasValue; }
            private set
            {
                if (value == true)
                {
                    _xMin = _yMin = _xMax = _yMax = 0;
                }

                _hasValue = !value;
            }
        }

        /// <summary>
        /// Gets the left boundary.
        /// </summary>
        public Double Left
        {
            get { return _xMin; }
            private set { _xMin = value; }
        }

        /// <summary>
        /// Gets the right boundary.
        /// </summary>
        public Double Right
        {
            get { return _xMax; }
            private set { _xMax = value; }
        }

        /// <summary>
        /// Gets the top boundary.
        /// </summary>
        public Double Top
        {
            get { return _yMax; }
            private set { _yMax = value; }
        }

        /// <summary>
        /// Gets the bottom boundary.
        /// </summary>
        public Double Bottom
        {
            get { return _yMin; }
            private set { _yMin = value; }
        }

        /// <summary>
        /// Returns the width of the bounding box.
        /// </summary>
        /// <returns>
        /// Width of this <see cref="BoundingBox"/>. 
        /// Returns <see cref="Double.NaN"/> if <see cref="IsEmpty"/> is true.
        /// </returns>
        public double Width
        {
            get
            {
                if (IsEmpty)
                {
                    return Double.NaN;
                }

                return Math.Abs(_xMax - _xMin);
            }
        }
        /// <summary>
        /// Returns the height of the bounding box.
        /// </summary>
        /// <returns>Height of this <see cref="BoundingBox"/>. Returns <see cref="Double.NaN"/> if <see cref="IsEmpty"/> is true.</returns>
        public double Height
        {
            get
            {
                if (IsEmpty)
                {
                    return Double.NaN;
                }

                return Math.Abs(_yMax - _yMin);
            }
        }

        #endregion

        #region Spatial Relationships

        #region Borders
        /// <summary>
        /// Determines if two boxes share, at least partially, a common border, within the 
        /// <see cref="Tolerance.Global">global tolerance</see>.
        /// </summary>
        /// <param name="box">The box to check.</param>
        /// <returns>
        /// True if <paramref name="box"/> shares a commons border, false if not, 
        /// or if either or both are empty.
        /// </returns>
        public bool Borders(BoundingBox box)
        {
            return Borders(box, Tolerance.Global);
        }

        /// <summary>
        /// Determines if two boxes share, at least partially, a common border.
        /// </summary>
        /// <param name="box">The box to check.</param>
        /// <param name="tolerance">The tolerance to use in comparing.</param>
        /// <returns>
        /// True if <paramref name="box"/> shares a commons border, false if not, or if either or both are empty.
        /// </returns>
        public bool Borders(BoundingBox box, Tolerance tolerance)
        {
            return this.Left == box.Left || this.Bottom == box.Bottom || this.Right == box.Right || this.Top == box.Top;
        }
        #endregion Borders

        #region Contains
        /// <summary>
        /// Returns true if this instance contains the <see cref="BoundingBox"/>, 
        /// within the <see cref="Tolerance.Global">global tolerance</see>.
        /// </summary>
        /// <param name="box"><see cref="BoundingBox"/></param>
        /// <returns>True this <see cref="BoundingBox"/> contains the <paramref name="box">argument</paramref>.</returns>
        public bool Contains(BoundingBox box)
        {
            return Contains(box, Tolerance.Global);
        }

        /// <summary>
        /// Returns true if this instance contains the <see cref="BoundingBox"/>., 
        /// within the <paramref name="tolerance">given tolerance</paramref>
        /// </summary>
        /// <param name="box"><see cref="BoundingBox"/></param>
        /// <param name="tolerance"><see cref="Tolerance"/> to use to compare values.</param>
        /// <returns>True this <see cref="BoundingBox"/> contains the <paramref name="box">argument</paramref>.</returns>
        public bool Contains(BoundingBox box, Tolerance tolerance)
        {
            if (this.IsEmpty || box.IsEmpty)
            {
                return false;
            }

            if (tolerance == null)
            {
                tolerance = Tolerance.Global;
            }

            return tolerance.LessOrEqual(this.Left, box.Left) &&
                tolerance.GreaterOrEqual(this.Top, box.Top) &&
                tolerance.GreaterOrEqual(this.Right, box.Right) &&
                tolerance.LessOrEqual(this.Bottom, box.Bottom);
        }

        /// <summary>
        /// Checks whether a <see cref="Point"/> borders or lies within the bounding box, 
        /// within the <see cref="Tolerance.Global">global tolerance</see>.
        /// </summary>
        /// <param name="p"><see cref="Point"/></param>
        /// <returns>True if <paramref name="p">the point</paramref> borders or is within this <see cref="BoundingBox"/>.</returns>
        public bool Contains(Point p)
        {
            return Contains(p, Tolerance.Global);
        }

        /// <summary>
        /// Checks whether a <see cref="Point"/> borders or lies within the bounding box, 
        /// within the <paramref name="tolerance">given tolerance</paramref>.
        /// </summary>
        /// <param name="p"><see cref="Point"/></param>
        /// <param name="tolerance"><see cref="Tolerance"/> to use to compare values.</param>
        /// <returns>True if <paramref name="p">the point</paramref> borders or is within this <see cref="BoundingBox"/>.</returns>
        public bool Contains(Point p, Tolerance tolerance)
        {
            if (p == null || this.IsEmpty || p.IsEmpty())
            {
                return false;
            }

            if (tolerance == null)
            {
                tolerance = Tolerance.Global;
            }

            if (tolerance.Less(this.Right, p.X))
            {
                return false;
            }
            if (tolerance.Greater(this.Left, p.X))
            {
                return false;
            }
            if (tolerance.Less(this.Top, p.Y))
            {
                return false;
            }
            if (tolerance.Greater(this.Bottom, p.Y))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if this instance contains the <paramref name="geometry">, 
        /// within the <see cref="Tolerance.Global">global tolerance</see>.
        /// </summary>
        /// <param name="geometry">Geometry to test if this <see cref="BoundingBox"/> contains.</param>
        /// <returns>True if this <see cref="BoundingBox"/> contains the 
        /// <paramref name="geometry">geometry</paramref>.</returns>
        public bool Contains(Geometry geometry)
        {
            return Contains(geometry, Tolerance.Global);
        }

        /// <summary>
        /// Returns true if this instance contains the <paramref name="geometry">, 
        /// within the <paramref name="tolerance">given tolerance</paramref>.
        /// </summary>
        /// <param name="geometry">Geometry to test if this <see cref="BoundingBox"/> contains.</param>
        /// <param name="tolerance"><see cref="Tolerance"/> to use to compare values.</param>
        /// <returns>True if this <see cref="BoundingBox"/> contains the 
        /// <paramref name="geometry">geometry</paramref>.</returns>
        public bool Contains(Geometry geometry, Tolerance tolerance)
        {
            if (geometry == null)
            {
                return false;
            }

            if (tolerance == null)
            {
                tolerance = Tolerance.Global;
            }

            if (geometry is Point)
            {
                return Contains(geometry as Point, tolerance);
            }

#warning Bounding box intersection is incorrect here...
            // TODO: Replace bounding box intersection with actual geometric intersection when NTS implemented
            if (geometry == null)
            {
                return false;
            }

            return Contains(geometry.GetBoundingBox(), tolerance);
        }
        #endregion Contains

        #region Intersects
        /// <summary>
        /// Determines whether the <see cref="BoundingBox"/> instance intersects the 
        /// <paramref name="box">argument</paramref>, 
        /// within the <see cref="Tolerance.Global">global tolerance</see>.
        /// </summary>
        /// <param name="box"><see cref="BoundingBox"/> to check intersection with.
        /// </param>
        /// <returns>
        /// True if the <paramref name="box">argument</paramref> 
        /// touches this <see cref="BoundingBox"/> instance in any way.
        /// </returns>
        public bool Intersects(BoundingBox box)
        {
            return Intersects(box, Tolerance.Global);
        }

        /// <summary>
        /// Determines whether the <see cref="BoundingBox"/> instance intersects the
        /// <paramref name="box">argument</paramref>, 
        /// within the <paramref name="tolerance">given tolerance</paramref>.
        /// </summary>
        /// <param name="box">
        /// <see cref="BoundingBox"/> to check intersection with.
        /// </param>
        /// <param name="tolerance">
        /// <see cref="Tolerance"/> to use to compare values.
        /// </param>
        /// <returns>
        /// True if the <paramref name="box">argument</paramref> touches 
        /// this <see cref="BoundingBox"/> instance in any way.
        /// </returns>
        public bool Intersects(BoundingBox box, Tolerance tolerance)
        {
            if (tolerance == null)
            {
                tolerance = Tolerance.Global;
            }

            return Touches(box, tolerance);
        }

        /// <summary>
        /// Returns true if this <see cref="BoundingBox"/> instance intersects 
        /// the <paramref name="geometry">argument</paramref>, 
        /// within the <see cref="Tolerance.Global">global tolerance</see>.
        /// </summary>
        /// <param name="geometry">
        /// <see cref="Geometry"/> to check intersection with.
        /// </param>
        /// <returns>
        /// True if this BoundingBox intersects the 
        /// <paramref name="geometry">geometry</paramref>.
        /// </returns>
        public bool Intersects(Geometry geometry)
        {
            return Intersects(geometry, Tolerance.Global);
        }

        /// <summary>
        /// Returns true if this <see cref="BoundingBox"/> instance intersects the 
        /// <paramref name="geometry">argument</paramref>, 
        /// within the <paramref name="tolerance">given tolerance</paramref>.
        /// </summary>
        /// <param name="geometry">
        /// <see cref="Geometry"/> to check intersection with.
        /// </param>
        /// <param name="tolerance">
        /// <see cref="Tolerance"/> to use to compare values.
        /// </param>
        /// <returns>
        /// True if this BoundingBox intersects the 
        /// <paramref name="geometry">geometry</paramref>.
        /// </returns>
        public bool Intersects(Geometry geometry, Tolerance tolerance)
        {
            if (tolerance == null)
            {
                tolerance = Tolerance.Global;
            }

            return Touches(geometry, tolerance);
        }
        #endregion Intersects

        #region Overlaps
        /// <summary>
        /// Returns true if this <see cref="BoundingBox" /> overlaps the passed <paramref name="b">BoundingBox</paramref>, 
        /// within the <see cref="Tolerance.Global">global tolerance</see>.
        /// </summary>
        /// <remarks>
        /// A <see cref="BoundingBox"/> can touch and not overlap. If the passed <paramref name="b">bounding box</paramref> and 
        /// this <see cref="BoundingBox"/> share a common edge or a common point, the <see cref="Touches"/> method will
        /// return true, but <see cref="Overlaps"/> will return false.
        /// </remarks>
        /// <param name="b"><see cref="BoundingBox"/> to test if it overlaps this <see cref="BoundingBox"/>.</param>
        /// <returns>True if the <paramref name="b">bounding box</paramref> overlaps.</returns>
        public bool Overlaps(BoundingBox b)
        {
            return Overlaps(b, Tolerance.Global);
        }

        /// <summary>
        /// Returns true if this <see cref="BoundingBox"/> overlaps the passed <paramref name="b">BoundingBox</paramref>, 
        /// within the <paramref name="tolerance">given tolerance</paramref>.
        /// </summary>
        /// <remarks>
        /// A <see cref="BoundingBox"/> can touch and not overlap. If the passed <paramref name="b">bounding box</paramref> and 
        /// this <see cref="BoundingBox"/> share a common edge or a common point, the <see cref="Touches"/> method will
        /// return true, but <see cref="Overlaps"/> will return false.
        /// </remarks>
        /// <param name="b"><see cref="BoundingBox"/> to test if it overlaps this <see cref="BoundingBox"/>.</param>
        /// <param name="tolerance"><see cref="Tolerance"/> to use to compare values.</param>
        /// <returns>True if the <paramref name="b">bounding box</paramref> overlaps.</returns>
        public bool Overlaps(BoundingBox b, Tolerance tolerance)
        {
            if (tolerance == null)
            {
                tolerance = Tolerance.Global;
            }

            if (this.IsEmpty || b.IsEmpty)
            {
                return false;
            }

            return Contains(b) ||
                   !(tolerance.GreaterOrEqual(b.Left, Right) ||
                     tolerance.LessOrEqual(b.Right, Left) ||
                     tolerance.LessOrEqual(b.Top, Bottom) ||
                     tolerance.GreaterOrEqual(b.Bottom, Top));
        }

        /// <summary>
        /// Returns true if this <see cref="BoundingBox"/> overlaps the <paramref name="p">point</paramref>, 
        /// within the <see cref="Tolerance.Global">global tolerance</see>.
        /// </summary>
        /// <remarks>
        /// A <see cref="Point"/> can touch and not overlap. If the <paramref name="p">point</paramref> and 
        /// the <see cref="BoundingBox"/> share a common point, the <see cref="Touches"/> method will
        /// return true, but <see cref="Overlaps"/> will return false. For <see cref="Point">points</see>,
        /// <see cref="Contains"/> will return true if Overlaps returns true.
        /// </remarks>
        /// <param name="p"><see cref="Point"/> to test if it overlaps this <see cref="BoundingBox"/>.</param>
        /// <returns>True if the <paramref name="p">point</paramref> overlaps.</returns>
        public bool Overlaps(Point p)
        {
            return Overlaps(p, Tolerance.Global);
        }


        /// <summary>
        /// Returns true if this <see cref="BoundingBox"/> overlaps the <paramref name="p">point</paramref>, 
        /// within the <paramref name="tolerance">given tolerance</paramref>.
        /// </summary>
        /// <remarks>
        /// A <see cref="Point"/> can touch and not overlap. If the <paramref name="p">point</paramref> and 
        /// the <see cref="BoundingBox"/> share a common point, the <see cref="Touches"/> method will
        /// return true, but <see cref="Overlaps"/> will return false. For <see cref="Point">points</see>,
        /// <see cref="Contains"/> will return true if Overlaps returns true.
        /// </remarks>
        /// <param name="p"><see cref="Point"/> to test if it overlaps this <see cref="BoundingBox"/>.</param>
        /// <param name="tolerance"><see cref="Tolerance"/> to use to compare values.</param>
        /// <returns>True if the <paramref name="p">point</paramref> overlaps.</returns>
        public bool Overlaps(Point p, Tolerance tolerance)
        {
            if (tolerance == null)
            {
                tolerance = Tolerance.Global;
            }

            if (p == null || p.IsEmpty())
            {
                return false;
            }

            return Overlaps(p.GetBoundingBox());
        }

        /// <summary>
        /// Returns true if this <see cref="BoundingBox"/> overlaps the <paramref name="geometry">geometry</paramref>, 
        /// within the <see cref="Tolerance.Global">global tolerance</see>.
        /// </summary>
        /// <remarks>
        /// A <see cref="Geometry"/> can touch and not overlap. If the <paramref name="geometry">geometry</paramref> and 
        /// the <see cref="BoundingBox"/> share a common edge or a point, the <see cref="Touches"/> method will
        /// return true, but <see cref="Overlaps"/> will return false. For <see cref="Point">points</see>,
        /// <see cref="Contains"/> will return true if Overlaps returns true.
        /// </remarks>
        /// <param name="geometry"><see cref="Geometry"/> to test if it overlaps this <see cref="BoundingBox"/>.</param>
        /// <returns>True if the <paramref name="geometry">geometry</paramref> overlaps.</returns>
        public bool Overlaps(Geometry geometry)
        {
            return Overlaps(geometry, Tolerance.Global);
        }

        /// <summary>
        /// Returns true if this <see cref="BoundingBox"/> overlaps the <paramref name="geometry">geometry</paramref>, 
        /// within the <paramref name="tolerance">given tolerance</paramref>.
        /// </summary>
        /// <remarks>
        /// A <see cref="Geometry"/> can touch and not overlap. If the <paramref name="geometry">geometry</paramref> and 
        /// the <see cref="BoundingBox"/> share a common edge or a point, the <see cref="Touches"/> method will
        /// return true, but <see cref="Overlaps"/> will return false. For <see cref="Point">points</see>,
        /// <see cref="Contains"/> will return true if Overlaps returns true.
        /// </remarks>
        /// <param name="geometry"><see cref="Geometry"/> to test if it overlaps this <see cref="BoundingBox"/>.</param>
        /// <param name="tolerance"><see cref="Tolerance"/> to use to compare values.</param>
        /// <returns>True if the <paramref name="geometry">geometry</paramref> overlaps.</returns>
        public bool Overlaps(Geometry geometry, Tolerance tolerance)
        {
            if (geometry == null)
            {
                return false;
            }

            if (tolerance == null)
            {
                tolerance = Tolerance.Global;
            }

            if (geometry is Point)
            {
                return Overlaps(geometry as Point, tolerance);
            }

#warning Bounding box intersection is incorrect here...
            // TODO: Replace bounding box intersection with actual geometric intersection when NTS implemented
            if (geometry == null)
            {
                return false;
            }

            return Overlaps(geometry.GetBoundingBox(), tolerance);
        }
        #endregion Overlaps

        #region Touches

        /// <summary>
        /// Returns true if this <see cref="BoundingBox"/> instance touches the 
        /// <paramref name="box">argument</paramref>, 
        /// within the <see cref="Tolerance.Global">global tolerance</see>.
        /// </summary>
        /// <param name="box">
        /// <see cref="BoundingBox"/> to check if this BoundingBox touches.
        /// </param>
        /// <returns>
        /// True if <paramref name="box"/> touches.
        /// </returns>
        public bool Touches(BoundingBox box)
        {
            return Touches(box, Tolerance.Global);
        }

        /// <summary>
        /// Returns true if this <see cref="BoundingBox"/> instance touches the 
        /// <paramref name="box">argument</paramref>, 
        /// within the <paramref name="tolerance">given tolerance</paramref>.
        /// </summary>
        /// <param name="box">
        /// <see cref="BoundingBox"/> to check if this BoundingBox touches.
        /// </param>
        /// <param name="tolerance">
        /// <see cref="Tolerance"/> to use to compare values.
        /// </param>
        /// <returns>
        /// True if <paramref name="box"/> touches.
        /// </returns>
        public bool Touches(BoundingBox box, Tolerance tolerance)
        {
            if (tolerance == null)
            {
                tolerance = Tolerance.Global;
            }

            if (this.IsEmpty || box.IsEmpty)
            {
                return false;
            }

            return !(tolerance.Greater(box.Left, Right) ||
                     tolerance.Less(box.Right, Left) ||
                     tolerance.Less(box.Top, Bottom) ||
                     tolerance.Greater(box.Bottom, Top));
        }

        /// <summary>
        /// Returns true if this <see cref="BoundingBox"/> instance touches the
        /// <paramref name="p">argument</paramref>, 
        /// within the <see cref="Tolerance.Global">global tolerance</see>.
        /// </summary>
        /// <param name="p">
        /// <see cref="Point"/> to check if this BoundingBox instance touches.
        /// </param>
        /// <returns>
        /// True if the <paramref name="p">point</paramref> touches.
        /// </returns>
        public bool Touches(Point p)
        {
            return Touches(p, Tolerance.Global);
        }

        /// <summary>
        /// Returns true if this <see cref="BoundingBox"/> instance touches the 
        /// <paramref name="p">argument</paramref>, 
        /// within the <paramref name="tolerance">given tolerance</paramref>.
        /// </summary>
        /// <param name="p">
        /// <see cref="Point"/> to check if this BoundingBox instance touches.
        /// </param>
        /// <param name="tolerance">
        /// <see cref="Tolerance"/> to use to compare values.
        /// </param>
        /// <returns>
        /// True if the <paramref name="p">point</paramref> touches.
        /// </returns>
        public bool Touches(Point p, Tolerance tolerance)
        {
            if (tolerance == null)
            {
                tolerance = Tolerance.Global;
            }

            if (p == null || p.IsEmpty())
            {
                return false;
            }

            return Touches(p.GetBoundingBox(), tolerance);
        }

        /// <summary>
        /// Returns true if this <see cref="BoundingBox"/> instance touches the 
        /// <paramref name="geometry">argument</paramref>, 
        /// within the <see cref="Tolerance.Global">global tolerance</see>.
        /// </summary>
        /// <param name="geometry">
        /// <see cref="Geometry"/> to test if it touches this <see cref="BoundingBox"/>.
        /// </param>
        /// <returns>
        /// True if the <paramref name="geometry">geometry</paramref> touches.
        /// </returns>
        public bool Touches(Geometry geometry)
        {
            return Touches(geometry, Tolerance.Global);
        }

        /// <summary>
        /// Returns true if this <see cref="BoundingBox"/> instance touches the 
        /// <paramref name="geometry">argument</paramref>, 
        /// within the <paramref name="tolerance">given tolerance</paramref>.
        /// </summary>
        /// <param name="geometry">
        /// <see cref="Geometry"/> to test if it touches this <see cref="BoundingBox"/>.
        /// </param>
        /// <param name="tolerance">
        /// <see cref="Tolerance"/> to use to compare values.
        /// </param>
        /// <returns>
        /// True if the <paramref name="geometry">geometry</paramref> touches.
        /// </returns>
        public bool Touches(Geometry geometry, Tolerance tolerance)
        {
            if (tolerance == null)
            {
                tolerance = Tolerance.Global;
            }

            if (geometry is Point)
            {
                return Touches(geometry as Point, tolerance);
            }

#warning Bounding box intersection is incorrect here...
            // TODO: Replace bounding box intersection with actual geometric intersection when NTS implemented
            if (geometry == null)
            {
                return false;
            }

            return Touches(geometry.GetBoundingBox(), tolerance);
        }
        #endregion Touches

        #region Within
        /// <summary>
        /// Returns true if this <see cref="BoundingBox"/> instance is completely within (does not border) the <paramref name="box">argument</paramref>, 
        /// within the <see cref="Tolerance.Global">global tolerance</see>.
        /// </summary>
        /// <param name="box"><see cref="BoundingBox"/></param>
        /// <returns>True it contains</returns>
        public bool Within(BoundingBox box)
        {
            return Within(box, Tolerance.Global);
        }

        /// <summary>
        /// Returns true if this <see cref="BoundingBox"/> instance is completely within (does not border) the <paramref name="box">argument</paramref>, 
        /// within the <paramref name="tolerance">given tolerance</paramref>.
        /// </summary>
        /// <param name="box"><see cref="BoundingBox"/></param>
        /// <param name="tolerance"><see cref="Tolerance"/> to use to compare values.</param>
        /// <returns>True it contains</returns>
        public bool Within(BoundingBox box, Tolerance tolerance)
        {
            if (this.IsEmpty || box.IsEmpty)
            {
                return false;
            }

            if (tolerance == null)
            {
                tolerance = Tolerance.Global;
            }

            return tolerance.LessOrEqual(this.Left, box.Left) &&
                 tolerance.GreaterOrEqual(this.Top, box.Top) &&
                 tolerance.GreaterOrEqual(this.Right, box.Right) &&
                 tolerance.LessOrEqual(this.Bottom, box.Bottom);
        }

        /// <summary>
        /// Returns true if this <see cref="BoundingBox"/> instance is completely within (does not border) 
        /// the <paramref name="p">argument</paramref>, 
        /// within the <see cref="Tolerance.Global">global tolerance</see>.
        /// </summary>
        /// <param name="geometry">Point to test if it is within this <see cref="BoundingBox"/> instance.</param>
        /// <returns>True if this <see cref="BoundingBox"/> is within the <paramref name="p">point</paramref>.</returns>
        public bool Within(Point p)
        {
            return Within(p, Tolerance.Global);
        }


        /// <summary>
        /// Returns true if this <see cref="BoundingBox"/> instance is completely within (does not border) 
        /// the <paramref name="p">argument</paramref>, 
        /// within the <paramref name="tolerance">given tolerance</paramref>.
        /// </summary>
        /// <param name="geometry">Point to test if it is within this <see cref="BoundingBox"/> instance.</param>
        /// <param name="tolerance"><see cref="Tolerance"/> to use to compare values.</param>
        /// <returns>True if this <see cref="BoundingBox"/> is within the <paramref name="p">point</paramref>.</returns>
        public bool Within(Point p, Tolerance tolerance)
        {
            if (p == null || this.IsEmpty || p.IsEmpty())
            {
                return false;
            }

            if (tolerance == null)
            {
                tolerance = Tolerance.Global;
            }

            if (tolerance.LessOrEqual(this.Right, p.X))
            {
                return false;
            }
            if (tolerance.GreaterOrEqual(this.Left, p.X))
            {
                return false;
            }
            if (tolerance.LessOrEqual(this.Top, p.Y))
            {
                return false;
            }
            if (tolerance.GreaterOrEqual(this.Bottom, p.Y))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns true if this <see cref="BoundingBox"/> instance is completely within 
        /// (does not border) the <paramref name="geometry">argument</paramref>, 
        /// within the <see cref="Tolerance.Global">global tolerance</see>.
        /// </summary>
        /// <param name="geometry">Geometry to test if it is within this <see cref="BoundingBox"/> instance.</param>
        /// <returns>True if this <see cref="BoundingBox"/> is within the 
        /// <paramref name="geometry">geometry</paramref>.</returns>
        public bool Within(Geometry geometry)
        {
            return Within(geometry, Tolerance.Global);
        }

        /// <summary>
        /// Returns true if this <see cref="BoundingBox"/> instance is completely within 
        /// (does not border) the <paramref name="geometry">argument</paramref>, 
        /// within the <paramref name="tolerance">given tolerance</paramref>.
        /// </summary>
        /// <param name="geometry">Geometry to test if it is within this <see cref="BoundingBox"/> instance.</param>
        /// <param name="tolerance"><see cref="Tolerance"/> to use to compare values.</param>
        /// <returns>True if this <see cref="BoundingBox"/> is within the 
        /// <paramref name="geometry">geometry</paramref>.</returns>
        public bool Within(Geometry geometry, Tolerance tolerance)
        {
            if (geometry == null)
            {
                return false;
            }

            if (tolerance == null)
            {
                tolerance = Tolerance.Global;
            }

            if (geometry is Point)
            {
                return Contains(geometry as Point, tolerance);
            }

#warning Bounding box intersection is incorrect here...
            // TODO: Replace bounding box intersection with actual geometric intersection when NTS implemented
            if (geometry == null)
            {
                return false;
            }

            return Within(geometry.GetBoundingBox(), tolerance);
        }
        #endregion Within
        #endregion

        #region GetArea and GetIntersectingArea
        /// <summary>
        /// Returns the area of the <see cref="BoundingBox"/>.
        /// </summary>
        /// <returns>Area of box</returns>
        public double GetArea()
        {
            return Width * Height;
        }

        /// <summary>
        /// Gets the intersecting area between two boundingboxes
        /// </summary>
        /// <param name="r">BoundingBox</param>
        /// <returns>Area</returns>
        public double GetIntersectingArea(BoundingBox r)
        {
            //uint cIndex;
            //for (cIndex = 0; cIndex < 2; cIndex++)
            //    if (Min[cIndex] > r.Max[cIndex] || Max[cIndex] < r.Min[cIndex]) return 0.0;

            if (!r.Intersects(this))
            {
                return 0.0;
            }

            //double ret = 1.0;
            //double f1, f2;

            //for (cIndex = 0; cIndex < 2; cIndex++)
            //{
            //    f1 = Math.Max(Min[cIndex], r.Min[cIndex]);
            //    f2 = Math.Min(Max[cIndex], r.Max[cIndex]);
            //    ret *= f2 - f1;
            //}

            return (Math.Min(r.Right, Right) - Math.Max(r.Left, Left)) * (Math.Min(r.Top, Top) - Math.Max(r.Bottom, Bottom));
        }
        #endregion

        #region Join
        /// <summary>
        /// Computes the joined BoundingBox of this instance and another BoundingBox.
        /// </summary>
        /// <param name="box">BoundingBox to join with</param>
        /// <returns>A new BoundingBox containing both BoundingBox instances.</returns>
        public BoundingBox Join(BoundingBox box)
        {
            if (box == BoundingBox.Empty)
            {
                return this;
            }
            else if (this == BoundingBox.Empty)
            {
                return box;
            }
            else
            {
                return new BoundingBox(Math.Min(Left, box.Left), Math.Min(Bottom, box.Bottom),
                                       Math.Max(Right, box.Right), Math.Max(Top, box.Top));
            }
        }

        /// <summary>
        /// Computes the joined BoundingBox of two BoundingBox instances.
        /// </summary>
        /// <param name="box1"></param>
        /// <param name="box2"></param>
        /// <returns></returns>
        public static BoundingBox Join(BoundingBox box1, BoundingBox box2)
        {
            return box1.Join(box2);
        }

        /// <summary>
        /// Computes the joined <see cref="BoundingBox"/> of an array of BoundingBox instances.
        /// </summary>
        /// <param name="boxes">BoundingBox instances to join.</param>
        /// <returns>Combined BoundingBox.</returns>
        public static BoundingBox Join(params BoundingBox[] boxes)
        {
            return new BoundingBox(boxes);
        }
        #endregion

        #region Grow and Shrink
        /// <summary>
        /// Returns a <see cref="BoundingBox"/> with a size decreased over this BoundingBox by the given 
        /// amount in all directions.
        /// </summary>
        /// <param name="amount">Amount to decrease in all directions.</param>
        public BoundingBox Shrink(double amount)
        {
            return Grow(-amount);
        }

        /// <summary>
        /// Returns a <see cref="BoundingBox"/> with a size decreased over this BoundingBox by the 
        /// given amount in horizontal and vertical directions.
        /// </summary>
        /// <param name="amountInX">Amount to decrease in horizontal direction.</param>
        /// <param name="amountInY">Amount to decrease in vertical direction.</param>
        public BoundingBox Shrink(double amountInX, double amountInY)
        {
            return Grow(-amountInX, -amountInY);
        }

        /// <summary>
        /// Returns a <see cref="BoundingBox"/> with a size decreased over this BoundingBox by the given 
        /// amount in each specific direction.
        /// </summary>
        /// <param name="amountBottom">Amount to decrease the bottom edge by.</param>
        /// <param name="amountLeft">Amount to decrease the left edge by.</param>
        /// <param name="amountRight">Amount to decrease the right edge by.</param>
        /// <param name="amountTop">Amount to decrease the top edge by.</param>
        public BoundingBox Shrink(double amountTop, double amountRight, double amountBottom, double amountLeft)
        {
            return Grow(-amountTop, -amountRight, -amountBottom, -amountLeft);
        }

        /// <summary>
        /// Returns a <see cref="BoundingBox"/> with a size increased over this BoundingBox by the given 
        /// amount in all directions.
        /// </summary>
        /// <param name="amount">Amount to increase in all directions.</param>
        public BoundingBox Grow(double amount)
        {
            return Grow(amount, amount);
        }

        /// <summary>
        /// Returns a <see cref="BoundingBox"/> with a size increased over this BoundingBox by the 
        /// given amount in horizontal and vertical directions.
        /// </summary>
        /// <param name="amountInX">Amount to increase in horizontal direction.</param>
        /// <param name="amountInY">Amount to increase in vertical direction.</param>
        public BoundingBox Grow(double amountInX, double amountInY)
        {
            return Grow(amountInY, amountInX, amountInY, amountInX);
        }

        /// <summary>
        /// Returns a <see cref="BoundingBox"/> with a size increased over this BoundingBox by the given 
        /// amount in each specific direction.
        /// </summary>
        /// <param name="amountBottom">Amount to increase the bottom edge by.</param>
        /// <param name="amountLeft">Amount to increase the left edge by.</param>
        /// <param name="amountRight">Amount to increase the right edge by.</param>
        /// <param name="amountTop">Amount to increase the top edge by.</param>
        public BoundingBox Grow(double amountTop, double amountRight, double amountBottom, double amountLeft)
        {
            BoundingBox box = this; // make a copy
            box.Left -= amountLeft;
            box.Bottom -= amountBottom;
            box.Right += amountRight;
            box.Top += amountTop;
            box.checkMinMax();
            return box;
        }
        #endregion

        #region Offset
        /// <summary>
        /// Moves/translates the <see cref="BoundingBox"/> along the the specified vector.
        /// </summary>
        /// <param name="vector">Offset vector.</param>
        public void Offset(Point vector)
        {
            _xMin += vector.X;
            _xMax += vector.X;
            _yMin += vector.Y;
            _yMax += vector.Y;
        }
        #endregion Offset

        #region ExpandToInclude
        /// <summary>
        /// Expands the <see cref="BoundingBox"/> instance to contain the space contained by <paramref name="box"/>.
        /// </summary>
        /// <param name="box"><see cref="BoundingBox"/> to enlarge box to contain.</param>
        public void ExpandToInclude(BoundingBox box)
        {
            if (box.Left < Left || IsEmpty)
            {
                Left = box.Left;
            }
            if (box.Bottom < Bottom || IsEmpty)
            {
                Bottom = box.Bottom;
            }
            if (box.Right > Right || IsEmpty)
            {
                Right = box.Right;
            }
            if (box.Top > Top || IsEmpty)
            {
                Top = box.Top;
            }

            IsEmpty = false;
        }

        /// <summary>
        /// Expands the <see cref="BoundingBox"/> instance to contain geometry <paramref name="geometry"/>.
        /// </summary>
        /// <param name="geometry"><see cref="Geometry"/> to enlarge box to contain.</param>
        public void ExpandToInclude(Geometry geometry)
        {
            ExpandToInclude(geometry.GetBoundingBox());
        }
        #endregion ExpandToInclude

        #region Split
        /// <summary>
        /// Splits the BoundingBox where it intersects with the <paramref name="point"/>.
        /// </summary>
        /// <param name="point">Point to perform split at.</param>
        /// <remarks>
        /// A BoundingBox instance may be split into 1, 2 or 4 new BoundingBox instances by a point
        /// depending on if the point is outside, on the edge of, or inside the BoundingBox respectively.
        /// </remarks>
        /// <returns>An enumeration of BoundingBox instances subdivided by the point.</returns>
        public IEnumerable<BoundingBox> Split(Point point)
        {
            if (!Contains(point))
            {
                return new BoundingBox[0];
            }

            List<BoundingBox> splits = new List<BoundingBox>(4);

            BoundingBox b1 = new BoundingBox(Left, point.Y, point.X, Top);
            BoundingBox b2 = new BoundingBox(point.X, point.Y, Right, Top);
            BoundingBox b3 = new BoundingBox(Left, Bottom, point.X, point.Y);
            BoundingBox b4 = new BoundingBox(point.X, Bottom, Right, point.Y);

            if (b1.GetArea() > 0)
            {
                splits.Add(b1);
            }

            if (b2.GetArea() > 0)
            {
                splits.Add(b2);
            }

            if (b3.GetArea() > 0)
            {
                splits.Add(b3);
            }

            if (b4.GetArea() > 0)
            {
                splits.Add(b4);
            }

            return splits;
        }

        #endregion

        #region Distance
        /// <summary> 
        /// Computes the minimum distance between this and another <see cref="BoundingBox"/>.
        /// The distance between overlapping bounding boxes is 0.  Otherwise, the
        /// distance is the Euclidean distance between the closest points.
        /// </summary>
        /// <param name="box">BoundingBox to calculate distance to.</param>
        /// <returns>The distance between this and another <see cref="BoundingBox"/>. 
        /// Returns <see cref="Double.NaN"/> if either <see cref="BoundingBox"/>'s <see cref="BoundingBox.IsEmpty"/>
        /// property is true.</returns>
        public double Distance(BoundingBox box)
        {
            if (this.IsEmpty || box.IsEmpty)
                return Double.NaN;

            double ret = 0.0;

            if (Contains(box))
            {
                return ret;
            }

            ret += box.Right < Left ? Math.Pow(Left - box.Right, 2) : Math.Pow(box.Left - Right, 2);
            ret += box.Top < Bottom ? Math.Pow(Bottom - box.Top, 2) : Math.Pow(box.Bottom - Top, 2);

            //for (uint cIndex = 0; cIndex < 2; cIndex++)
            //{
            //    if (p[cIndex] < Min[cIndex]) ret += Math.Pow(Min[cIndex] - p[cIndex], 2.0);
            //    else if (p[cIndex] > Max[cIndex]) ret += Math.Pow(p[cIndex] - Max[cIndex], 2.0);
            //}

            return Math.Sqrt(ret);
        }

        /// <summary>
        /// Computes the minimum distance between this BoundingBox and a <see cref="Point"/>.
        /// </summary>
        /// <param name="p"><see cref="Point"/> to calculate distance to.</param>
        /// <returns>Minimum distance.</returns>
        public double Distance(Point p)
        {
            return Distance(p.GetBoundingBox());
        }

        #endregion

        #region GetCentroid
        /// <summary>
        /// Returns the center of the BoundingBox.
        /// </summary>
        public Point GetCentroid()
        {
            if (IsEmpty)
            {
                return Point.Empty;
            }

            return (this.Min + this.Max) * 0.5f;
        }
        #endregion

        #region ToGeometry
        /// <summary>
        /// Computes a <see cref="Geometry"/> with the same verticies
        /// as the BoundingBox instance.
        /// </summary>
        /// <returns>
        /// A <see cref="Polygon"/> with the exact same shape and 
        /// area as the BoundingBox, if the BoundingBox is not empty,
        /// or <see cref="Point.Empty"/> if it is.
        /// </returns>
        public Geometry ToGeometry()
        {
            if (IsEmpty)
            {
                return Point.Empty;
            }

            Point[] verticies = new Point[] { LowerLeft, UpperLeft, UpperRight, LowerRight, LowerLeft };
            return new Polygon(new LinearRing(verticies));
        }
        #endregion

        #region Equality
        /// <summary>
        /// Checks whether the values of this instance is equal to the values of another instance, within the 
        /// <see cref="Tolerance.Global">global tolerance</see>.
        /// </summary>
        /// <param name="other"><see cref="BoundingBox"/> to compare to.</param>
        /// <returns>True if equal within <see cref="Tolerance.Global"/>.</returns>
        public bool Equals(BoundingBox other)
        {
            return Equals(other, Tolerance.Global);
        }

        /// <summary>
        /// Checks whether the values of this instance is equal to the values of another instance, within the 
        /// <paramref name="tolerance">given tolerance</paramref>.
        /// </summary>
        /// <param name="other"><see cref="BoundingBox"/> to compare to.</param>
        /// <param name="tolerance">The <see cref="Tolerance"/> to use to compare.</param>
        /// <returns>True if equal within <paramref name="tolerance"/>.</returns>
        public bool Equals(BoundingBox other, Tolerance tolerance)
        {
            // Check empty
            if (this.IsEmpty == true && other.IsEmpty == true)
            {
                return true;
            }

            if (tolerance == null)
            {
                tolerance = Tolerance.Global;
            }

            if (this.IsEmpty || other.IsEmpty)
            {
                return false;
            }

            return tolerance.Equal(this.Left, other.Left) &&
                tolerance.Equal(this.Right, other.Right) &&
                tolerance.Equal(this.Top, other.Top) &&
                tolerance.Equal(this.Bottom, other.Bottom);
        }

        #endregion Equality

        #region Object Overrides
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is BoundingBox))
            {
                return false;
            }

            BoundingBox box = (BoundingBox)obj;
            return Equals(box);
        }

        /// <summary>
        /// Returns a hash code for the specified object
        /// </summary>
        /// <returns>A hash code for the specified object</returns>
        public override int GetHashCode()
        {
            return (int)(_xMin + _yMin + _xMax + _yMax);
        }

        /// <summary>
        /// Returns a string representation of the <see cref="BoundingBox"/> as "(MinX, MinY) (MaxX, MaxY)".
        /// </summary>
        /// <returns>Lower Left: (MinX, MinY) Upper Right: (MaxX, MaxY).</returns>
        public override string ToString()
        {
            if (this == BoundingBox.Empty)
            {
                return "[BoundingBox] Empty";
            }

            return String.Format(CultureInfo.CurrentCulture, "[BoundingBox] Lower Left: ({0:N}, {1:N}) Upper Right: ({2:N}, {3:N})", Left, Bottom, Right, Top);
        }
        #endregion Object Overrides

        #region Value Operators
        public static bool operator ==(BoundingBox box1, BoundingBox box2)
        {
            return box1.Equals(box2);
        }

        public static bool operator !=(BoundingBox box1, BoundingBox box2)
        {
            return !box1.Equals(box2);
        }
        #endregion

        #region Intersection Static Method
        /// <summary>
        /// Returns the intersection of two <see cref="BoundingBox"/> instances as a BoundingBox.
        /// </summary>
        /// <param name="b1">The first <see cref="BoundingBox"/> to intersect.</param>
        /// <param name="b2">The second BoundingBox to intersect.</param>
        /// <returns>The BoundingBox which represents the shared area between <paramref name="b1"/> and <paramref name="b2"/>.</returns>
        public static BoundingBox Intersection(BoundingBox b1, BoundingBox b2)
        {
            if (!b1.Intersects(b2))
            {
                return BoundingBox.Empty;
            }

            return new BoundingBox(
                b1.Left < b2.Left ? b2.Left : b1.Left,
                b1.Bottom < b2.Bottom ? b2.Bottom : b1.Bottom,
                b1.Right > b2.Right ? b2.Right : b1.Right,
                b1.Top > b2.Top ? b2.Top : b1.Top);
        }
        #endregion Intersection Static Method

        #region Private Helper Methods
        /// <summary>
        /// Checks whether min values are actually smaller than max values and in that case swaps them.
        /// </summary>
        /// <returns>True if the bounding was changed, false otherwise.</returns>
        private bool checkMinMax()
        {
            bool wasSwapped = false;

            if (_xMin > _xMax)
            {
                double tmp = _xMin;
                _xMin = _xMax;
                _xMax = tmp;
                wasSwapped = true;
            }

            if (_yMin > _yMax)
            {
                double tmp = _yMin;
                _yMin = _yMax;
                _yMax = tmp;
                wasSwapped = true;
            }

            return wasSwapped;
        }
        #endregion Private Helper Methods
    }
}
