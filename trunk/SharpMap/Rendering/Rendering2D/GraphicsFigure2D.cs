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
using System.Collections.Generic;

namespace SharpMap.Rendering.Rendering2D
{
    /// <summary>
    /// A graphical figure in 2 dimensions.
    /// </summary>
    public class GraphicsFigure2D : GraphicsFigure<Point2D, Rectangle2D>
	{
		public GraphicsFigure2D(IEnumerable<Point2D> points)
			: base(points) { }

        public GraphicsFigure2D(IEnumerable<Point2D> points, bool isClosed)
            : base(points, isClosed) { }

        protected override Rectangle2D ComputeBounds()
        {
            double left = Double.MaxValue;
            double top = Double.MaxValue;
            double right = Double.MinValue;
            double bottom = Double.MinValue;

            foreach (Point2D point in Points)
            {
                if (left > point.X)
                {
                    left = point.X;
                }
                if (right < point.X)
                {
                    right = point.X;
                }
                if (top > point.Y)
                {
                    top = point.Y;
                }
                if (bottom < point.Y)
                {
                    bottom = point.Y;
                }
            }

            return new Rectangle2D(left, top, right, bottom);
        }

        protected override GraphicsFigure<Point2D, Rectangle2D> CreateFigure(IEnumerable<Point2D> points, bool isClosed)
        {
            return new GraphicsFigure2D(points, isClosed);
        }

        protected override Rectangle2D EmptyBounds
        {
            get { return Rectangle2D.Empty; }
        }
    }
}
