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
using System.Text;
using SharpMap.Rendering.Rendering2D;

namespace SharpMap.Styles
{
	/// <summary>
	/// Represents a style for drawing lines.
	/// </summary>
    [Serializable]
    public class StylePen
	{
		#region Fields
        // This default (10.0f) is the same in both GDI+ and WPF 
        // for the MiterLimit value on the respective Pen objects.
		private float _miterLimit = 10.0f;
        private StyleBrush _backgroundBrush;
        private float _dashOffset;
        private LineDashCap _dashCap;
        private LineDashStyle _dashStyle;
        private float[] _dashPattern;
        private StyleBrush[] _dashBrushes;
        private StyleLineCap _startCap;
        private StyleLineCap _endCap;
        private StyleLineJoin _lineJoin;
        private Matrix2D _transform = new Matrix2D();
        private float _width;
        private float[] _compoundArray;
        private StylePenAlignment _alignment;
		#endregion

		#region Object Constructors
		/// <summary>
		/// Creates a new <see cref="StylePen"/> with the given solid
		/// <paramref name="color"/> and <paramref name="width"/>.
		/// </summary>
		/// <param name="color">Color of the pen.</param>
		/// <param name="width">Width of the pen.</param>
		public StylePen(StyleColor color, float width)
            : this(new SolidStyleBrush(color), width) { }

		/// <summary>
		/// Creates a new pen with the given <see cref="StyleBrush"/>
		/// and <paramref name="width"/>.
		/// </summary>
		/// <param name="backgroundBrush">The StyleBrush which describes the color of the line.</param>
		/// <param name="width">The width of the line.</param>
        public StylePen(StyleBrush backgroundBrush, float width)
        {
            _backgroundBrush = backgroundBrush;
            _width = width;
		}
		#endregion

		#region ToString
		public override string ToString()
		{
			return String.Format(
				"[StylePen] Width: {0}; Alignment: {2}; CompoundArray: {3}; MiterLimit: {4}; DashOffset: {5}; DashPattern: {6}; DashBrushes: {7}; DashStyle: {8}; StartCap: {9}; EndCap: {10}; DashCap: {11}; LineJoin: {12}; Transform: {13}; Background: {1};",
				Width, BackgroundBrush.ToString(), Alignment, printFloatArray(CompoundArray), MiterLimit, DashOffset, DashPattern, printBrushes(DashBrushes), DashStyle, StartCap, EndCap, DashCap, LineJoin, Transform);
		}
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the alignment of the pen.
		/// </summary>
		public StylePenAlignment Alignment
        {
            get { return _alignment; }
            set { _alignment = value; }
        }

		/// <summary>
		/// Gets or sets an array of widths used to create a compound line.
		/// </summary>
        public float[] CompoundArray
        {
            get { return _compoundArray; }
            set { _compoundArray = value; }
        }

		/// <summary>
		/// Gets or sets a value which limits the length of a miter at a line joint.
		/// </summary>
        /// <remarks>
        /// If the value is set to less than 1.0f, the value is clamped to 1.0f.
        /// </remarks>
        public float MiterLimit
        {
            get { return _miterLimit; }
            set 
            {
                if (value < 1.0f)
                {
                    value = 1.0f;
                }

                _miterLimit = value; 
            }
        }

		/// <summary>
		/// Gets or sets a brush used to paint the line.
		/// </summary>
        public StyleBrush BackgroundBrush
        {
            get { return _backgroundBrush; }
            set { _backgroundBrush = value; }
        }

		/// <summary>
		/// Gets or sets the offset of the start of the dash pattern.
		/// </summary>
        public float DashOffset
        {
            get { return _dashOffset; }
            set { _dashOffset = value; }
        }

		/// <summary>
		/// Gets or sets an array of values used as widths in a dash pattern.
		/// </summary>
        public float[] DashPattern
        {
            get { return _dashPattern; }
            set { _dashPattern = value; }
        }

		/// <summary>
		/// Gets or sets an array of brushes used to draw the dashes in a pen.
		/// </summary>
        public StyleBrush[] DashBrushes
        {
            get { return _dashBrushes; }
            set { _dashBrushes = value; }
        }

		/// <summary>
		/// Gets or sets a value used to compute the dash pattern.
		/// </summary>
        public LineDashStyle DashStyle
        {
            get { return _dashStyle; }
            set { _dashStyle = value; }
        }

		/// <summary>
		/// Gets or sets the type of line terminator at the beginning
		/// of a line.
		/// </summary>
        public StyleLineCap StartCap
        {
            get { return _startCap; }
            set { _startCap = value; }
        }

		/// <summary>
		/// Gets or sets the type of line terminator at the end
		/// of a line.
		/// </summary>
        public StyleLineCap EndCap
        {
            get { return _endCap; }
            set { _endCap = value; }
        }

		/// <summary>
		/// Gets or sets the type of ending present at the start and end
		/// of a dash in the line.
		/// </summary>
        public LineDashCap DashCap
        {
            get { return _dashCap; }
            set { _dashCap = value; }
        }

		/// <summary>
		/// Gets or sets the type of joint drawn where a line contains a join.
		/// </summary>
        public StyleLineJoin LineJoin
        {
            get { return _lineJoin; }
            set { _lineJoin = value; }
        }

		/// <summary>
		/// Gets or sets a matrix transformation for drawing with the pen.
		/// </summary>
        /// <remarks>
        /// If set to null, a <see cref="Matrix2D.Identity"/> matrix will be used instead.
        /// </remarks>
        public Matrix2D Transform
        {
            get { return _transform; }
            set 
            {
                if (value == null)
                {
                    value = new Matrix2D();
                }

                _transform = value; 
            }
        }

		/// <summary>
		/// Gets or sets the width of the line drawn by this pen.
		/// </summary>
        public float Width
        {
            get { return _width; }
            set { _width = value; }
		}
		#endregion

		#region Private Helper Methods
		private string printBrushes(StyleBrush[] brushes)
        {
            if (brushes == null || brushes.Length == 0)
                return String.Empty;

            StringBuilder buffer = new StringBuilder();

            foreach (StyleBrush brush in brushes)
            {
                buffer.Append(brush.ToString());
                buffer.Append(", ");
            }

            buffer.Length -= 2;
            return buffer.ToString();
        }

        private string printFloatArray(float[] values)
        {
            if (values == null || values.Length == 0)
                return String.Empty;

            StringBuilder buffer = new StringBuilder();

            foreach (float value in values)
            {
                buffer.AppendFormat("N3", value);
                buffer.Append(", ");
            }

            buffer.Length -= 2;
            return buffer.ToString();
		}
		#endregion
	}
}
