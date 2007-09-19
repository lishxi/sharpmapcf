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
using SharpMap.Data;
using SharpMap.Features;
using SharpMap.Rendering.Rendering2D;
using SharpMap.Styles;

namespace SharpMap.Rendering.Thematics
{
    /// <summary>
    /// Delegate type for a method to compute a linear interpolation between two 
    /// <see cref="IStyle"/> instances.
    /// </summary>
    /// <param name="min">
    /// Style which represents the minimum end of the interpolation.
    /// </param>
    /// <param name="max">
    /// Style which represents the maximum end of the interpolation.
    /// </param>
    /// <param name="weighting">
    /// Value which determines how much either end participates in the interpolation.
    /// </param>
    /// <returns>
    /// An <see cref="IStyle"/> instance which blends <paramref name="min"/> and 
    /// <paramref name="max"/>.
    /// </returns>
    public delegate IStyle CalculateStyleDelegate(IStyle min, IStyle max, double weighting);

    /// <summary>
    /// The GradientTheme class defines a gradient color 
    /// thematic rendering of features based by a numeric attribute.
    /// </summary>
    public class GradientTheme2D : ITheme
    {
        private string _columnName;
        private double _min;
        private double _max;
        private readonly IStyle _minStyle;
        private readonly IStyle _maxStyle;
        private StyleColorBlend _fillColorBlend;
        private StyleColorBlend _lineColorBlend;
        private StyleColorBlend _textColorBlend;

        private readonly Dictionary<RuntimeTypeHandle, CalculateStyleDelegate> _styleTypeFunctionTable =
            new Dictionary<RuntimeTypeHandle, CalculateStyleDelegate>();

        /// <summary>
        /// Initializes a new instance of the GradientTheme class
        /// </summary>
        /// <remarks>
        /// <para>The gradient theme interpolates linearly between two styles based on a numerical attribute in the datasource.
        /// This is useful for scaling symbols, line widths, line and fill colors from numerical attributes.</para>
        /// <para>Colors are interpolated between two colors, but if you want to interpolate through more colors (fx. a rainbow),
        /// set the <see cref="TextColorBlend"/>, <see cref="LineColorBlend"/> and <see cref="FillColorBlend"/> properties
        /// to a custom <see cref="StyleColorBlend"/>.
        /// </para>
        /// <para>The following properties are scaled (properties not mentioned here are not interpolated):
        /// <list type="table">
        ///		<listheader><term>Property</term><description>Remarks</description></listheader>
        ///		<item><term><see cref="System.Drawing.Color"/></term><description>Red, Green, Blue and Alpha values are linearly interpolated.</description></item>
        ///		<item><term><see cref="System.Drawing.Pen"/></term><description>The color, width, color of pens are interpolated. MiterLimit,StartCap,EndCap,LineJoin,DashStyle,DashPattern,DashOffset,DashCap,CompoundArray, and Alignment are switched in the middle of the min/max values.</description></item>
        ///		<item><term><see cref="System.Drawing.SolidBrush"/></term><description>SolidBrush color are interpolated. Other brushes are not supported.</description></item>
        ///		<item><term><see cref="SharpMap.Styles.VectorStyle"/></term><description>MaxVisible, MinVisible, Line, Outline, Fill and SymbolScale are scaled linearly. Symbol, EnableOutline and Enabled switch in the middle of the min/max values.</description></item>
        ///		<item><term><see cref="SharpMap.Styles.LabelStyle"/></term><description>FontSize, BackColor, ForeColor, MaxVisible, MinVisible, Offset are scaled linearly. All other properties use min-style.</description></item>
        /// </list>
        /// </para>
        /// <example>
        /// Creating a rainbow colorblend showing colors from red, through yellow, green and blue depicting 
        /// the population density of a country.
        /// <code lang="C#">
        /// //Create two vector styles to interpolate between
        /// SharpMap.Styles.VectorStyle min = new SharpMap.Styles.VectorStyle();
        /// SharpMap.Styles.VectorStyle max = new SharpMap.Styles.VectorStyle();
        /// min.Outline.Width = 1f; //Outline width of the minimum value
        /// max.Outline.Width = 3f; //Outline width of the maximum value
        /// //Create a theme interpolating population density between 0 and 400
        /// SharpMap.Rendering.Thematics.GradientTheme popdens = new SharpMap.Rendering.Thematics.GradientTheme("PopDens", 0, 400, min, max);
        /// //Set the fill-style colors to be a rainbow blend from red to blue.
        /// popdens.FillColorBlend = SharpMap.Rendering.Thematics.ColorBlend.Rainbow5;
        /// myFeatureLayer.Theme = popdens;
        /// </code>
        /// </example>
        /// </remarks>
        /// <param name="columnName">Name of column to extract the attribute</param>
        /// <param name="minValue">Minimum value</param>
        /// <param name="maxValue">Maximum value</param>
        /// <param name="minStyle">Color for minimum value</param>
        /// <param name="maxStyle">Color for maximum value</param>
        public GradientTheme2D(string columnName, double minValue, double maxValue, IStyle minStyle, IStyle maxStyle)
        {
            if (minStyle == null) throw new ArgumentNullException("minStyle");
            if (maxStyle == null) throw new ArgumentNullException("maxStyle");

            _columnName = columnName;
            _min = minValue;
            _max = maxValue;
            _maxStyle = maxStyle;
            _minStyle = minStyle;
            _styleTypeFunctionTable[typeof (VectorStyle).TypeHandle] = calculateVectorStyle;
            _styleTypeFunctionTable[typeof (LabelStyle).TypeHandle] = calculateLabelStyle;
        }

        /// <summary>
        /// Gets or sets the column name from where to get the attribute value
        /// </summary>
        public string ColumnName
        {
            get { return _columnName; }
            set { _columnName = value; }
        }

        /// <summary>
        /// Gets or sets the minimum value of the gradient
        /// </summary>
        public double Min
        {
            get { return _min; }
            set { _min = value; }
        }

        /// <summary>
        /// Gets or sets the maximum value of the gradient
        /// </summary>
        public double Max
        {
            get { return _max; }
            set { _max = value; }
        }

        /// <summary>
        /// Gets the <see cref="SharpMap.Styles.IStyle">style</see> 
        /// for the minimum value.
        /// </summary>
        public IStyle MinStyle
        {
            get { return _minStyle; }
        }

        /// <summary>
        /// Gets the <see cref="SharpMap.Styles.IStyle">style</see> 
        /// for the maximum value.
        /// </summary>
        public IStyle MaxStyle
        {
            get { return _maxStyle; }
        }

        /// <summary>
        /// Gets or sets the <see cref="StyleColorBlend"/> used on labels.
        /// </summary>
        public StyleColorBlend TextColorBlend
        {
            get { return _textColorBlend; }
            set { _textColorBlend = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="StyleColorBlend"/> used on lines.
        /// </summary>
        public StyleColorBlend LineColorBlend
        {
            get { return _lineColorBlend; }
            set { _lineColorBlend = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="StyleColorBlend"/> used as Fill
        /// </summary>
        public StyleColorBlend FillColorBlend
        {
            get { return _fillColorBlend; }
            set { _fillColorBlend = value; }
        }

        #region ITheme Members

        /// <summary>
        /// Returns the style based on a numeric DataColumn, where style
        /// properties are linearly interpolated between max and min values.
        /// </summary>
        /// <param name="row">Feature</param>
        /// <returns><see cref="SharpMap.Styles.IStyle">Style</see> calculated by a linear interpolation between the min/max styles</returns>
        public IStyle GetStyle(FeatureDataRow row)
        {
            double weighting = 0;

            try
            {
                weighting = Convert.ToDouble(row[_columnName]);
            }
            catch
            {
                throw new InvalidOperationException(
                    "Invalid attribute type in gradient theme. "+
                    "Couldn't parse weighting attribute (must be numeric).");
            }

            if (MinStyle == null)
            {
                throw new InvalidOperationException("Cannot create a gradient style if the MinStyle is missing.");
            }

            if (MaxStyle == null)
            {
                throw new InvalidOperationException("Cannot create a gradient style if the MaxStyle is missing.");
            }

            Type minStyleType = MinStyle.GetType();
            Type maxStyleType = MaxStyle.GetType();

            if (minStyleType != maxStyleType)
            {
                throw new ArgumentException("MinStyle and MaxStyle must be of the same type");
            }

            CalculateStyleDelegate styleCalculator;
            _styleTypeFunctionTable.TryGetValue(minStyleType.TypeHandle, out styleCalculator);

            if (styleCalculator == null)
            {
                throw new ArgumentException(
                    "Only SharpMap.Styles.VectorStyle and SharpMap.Styles.LabelStyle "+
                    "are supported for the gradient theme");
            }

            return styleCalculator(MinStyle, MaxStyle, weighting);
        }

        #endregion
        
		#region Explicit Interface Implementation
        #region ITheme Members
        IStyle ITheme.GetStyle(IFeatureDataRecord row)
        {
            if (!(row is FeatureDataRow))
            {
                throw new ArgumentException("Parameter 'row' must be of type FeatureDataRow");
            }

            return GetStyle(row as FeatureDataRow);
        }
        #endregion
        #endregion

        #region Private helper methods

        private IStyle calculateVectorStyle(IStyle min, IStyle max, double value)
        {
            if (!(min is VectorStyle && max is VectorStyle))
            {
                throw new ArgumentException(
                    "Both min style and max style must be vector styles to compute a gradient vector style");
            }

            VectorStyle style = new VectorStyle();
            VectorStyle vectorMin = min as VectorStyle;
            VectorStyle vectorMax = max as VectorStyle;

            double dFrac = fraction(value);
            float fFrac = Convert.ToSingle(dFrac);
            style.Enabled = (dFrac > 0.5 ? min.Enabled : max.Enabled);
            style.EnableOutline = (dFrac > 0.5 ? vectorMin.EnableOutline : vectorMax.EnableOutline);

            if (_fillColorBlend != null)
            {
                style.Fill = new SolidStyleBrush(_fillColorBlend.GetColor(fFrac));
            }
            else if (vectorMin.Fill != null && vectorMax.Fill != null)
            {
                style.Fill = interpolateBrush(vectorMin.Fill, vectorMax.Fill, value);
            }

            if (vectorMin.Line != null && vectorMax.Line != null)
            {
                style.Line = interpolatePen(vectorMin.Line, vectorMax.Line, value);
            }

            if (_lineColorBlend != null)
            {
                style.Line.BackgroundBrush = new SolidStyleBrush(_lineColorBlend.GetColor(fFrac));
            }

            if (vectorMin.Outline != null && vectorMax.Outline != null)
            {
                style.Outline = interpolatePen(vectorMin.Outline, vectorMax.Outline, value);
            }

            style.MinVisible = interpolateDouble(min.MinVisible, max.MinVisible, value);
            style.MaxVisible = interpolateDouble(min.MaxVisible, max.MaxVisible, value);
            style.Symbol = (dFrac > 0.5 ? vectorMin.Symbol : vectorMax.Symbol);
            style.HighlightSymbol = (dFrac > 0.5 ? vectorMin.HighlightSymbol : vectorMax.HighlightSymbol);
            //We don't interpolate the offset but let it follow the symbol instead
            style.SelectSymbol = (dFrac > 0.5 ? vectorMin.SelectSymbol : vectorMax.SelectSymbol);
            return style;
        }

        private IStyle calculateLabelStyle(IStyle min, IStyle max, double value)
        {
            if (!(min is LabelStyle && max is LabelStyle))
            {
                throw new ArgumentException(
                    "Both min style and max style must be label styles to compute a gradient label style");
            }

            LabelStyle style = new LabelStyle();
            LabelStyle labelMin = min as LabelStyle;
            LabelStyle labelMax = max as LabelStyle;

            style.CollisionDetection = labelMin.CollisionDetection;
            style.Enabled = interpolateBool(min.Enabled, max.Enabled, value);

            double fontSize = interpolateDouble(labelMin.Font.Size.Width, labelMax.Font.Size.Width, value);
            style.Font = new StyleFont(labelMin.Font.FontFamily, new Size2D(fontSize, fontSize), labelMin.Font.Style);

            if (labelMin.BackColor != null && labelMax.BackColor != null)
            {
                style.BackColor = interpolateBrush(labelMin.BackColor, labelMax.BackColor, value);
            }

            if (_textColorBlend != null)
            {
                style.ForeColor = _lineColorBlend.GetColor(Convert.ToSingle(fraction(value)));
            }
            else
            {
                style.ForeColor = StyleColor.Interpolate(labelMin.ForeColor, labelMax.ForeColor, value);
            }

            if (labelMin.Halo != null && labelMax.Halo != null)
            {
                style.Halo = interpolatePen(labelMin.Halo, labelMax.Halo, value);
            }

            style.MinVisible = interpolateDouble(labelMin.MinVisible, labelMax.MinVisible, value);
            style.MaxVisible = interpolateDouble(labelMin.MaxVisible, labelMax.MaxVisible, value);
            style.Offset = new Point2D(interpolateDouble(labelMin.Offset.X, labelMax.Offset.X, value),
                                       interpolateDouble(labelMin.Offset.Y, labelMax.Offset.Y, value));

            return style;
        }

        private double fraction(double attr)
        {
            if (attr < _min) return 0;
            if (attr > _max) return 1;
            return (attr - _min) / (_max - _min);
        }

        private bool interpolateBool(bool min, bool max, double attr)
        {
            double frac = fraction(attr);
            if (frac > 0.5) return max;
            else return min;
        }

        private float interpolateFloat(float min, float max, double attr)
        {
            return Convert.ToSingle((max - min) * fraction(attr) + min);
        }

        private double interpolateDouble(double min, double max, double attr)
        {
            return (max - min) * fraction(attr) + min;
        }

        private StyleBrush interpolateBrush(StyleBrush min, StyleBrush max, double attr)
        {
            return new SolidStyleBrush(StyleColor.Interpolate(min.Color, max.Color, fraction(attr)));
        }

        private StylePen interpolatePen(StylePen min, StylePen max, double attr)
        {
            double frac = fraction(attr);

            StyleColor color = StyleColor.Interpolate(min.BackgroundBrush.Color, max.BackgroundBrush.Color, frac);
            StylePen pen = new StylePen(color, interpolateFloat(min.Width, max.Width, attr));

            pen.MiterLimit = interpolateFloat(min.MiterLimit, max.MiterLimit, attr);
            pen.StartCap = (frac > 0.5 ? max.StartCap : min.StartCap);
            pen.EndCap = (frac > 0.5 ? max.EndCap : min.EndCap);
            pen.LineJoin = (frac > 0.5 ? max.LineJoin : min.LineJoin);
            pen.DashStyle = (frac > 0.5 ? max.DashStyle : min.DashStyle);

            if (min.DashStyle == LineDashStyle.Custom && max.DashStyle == LineDashStyle.Custom)
            {
                pen.DashPattern = (frac > 0.5 ? max.DashPattern : min.DashPattern);
            }

            pen.DashOffset = (frac > 0.5 ? max.DashOffset : min.DashOffset);
            pen.DashCap = (frac > 0.5 ? max.DashCap : min.DashCap);

            if (min.CompoundArray.Length > 0 && max.CompoundArray.Length > 0)
            {
                pen.CompoundArray = (frac > 0.5 ? max.CompoundArray : min.CompoundArray);
            }

            pen.Alignment = (frac > 0.5 ? max.Alignment : min.Alignment);
            //pen.CustomStartCap = (frac > 0.5 ? max.CustomStartCap : min.CustomStartCap);  //Throws ArgumentException
            //pen.CustomEndCap = (frac > 0.5 ? max.CustomEndCap : min.CustomEndCap);  //Throws ArgumentException
            return pen;
        }
        #endregion
    }
}