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

namespace SharpMap.Geometries
{
    /// <summary>
    /// A MultiLineString is a MultiCurve whose elements are LineStrings.
    /// </summary>
    [Serializable]
    public class MultiLineString : MultiCurve<LineString>
    {
        /// <summary>
        /// Initializes an instance of a MultiLineString
        /// </summary>
        public MultiLineString() { }

        public MultiLineString(int initialCapacity)
            : base(initialCapacity) { }

        /// <summary>
        /// Collection of <see cref="LineString">LineStrings</see> 
        /// in the <see cref="MultiLineString"/>
        /// </summary>
        public IList<LineString> LineStrings
        {
            get { return Collection; }
        }

        /// <summary>
        /// Creates a copy of this geometry.
        /// </summary>
        /// <returns>Copy of the MultiLineString.</returns>
        public override Geometry Clone()
        {
            MultiLineString multiLineString = new MultiLineString();

            foreach (LineString lineString in Collection)
            {
                multiLineString.LineStrings.Add(lineString.Clone() as LineString);
            }

            return multiLineString;
        }
    }
}