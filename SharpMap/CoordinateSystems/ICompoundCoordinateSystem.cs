// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
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

namespace SharpMap.CoordinateSystems
{
	/// <summary>
	/// An aggregate of two coordinate reference systems (CRS). One of these is usually a 
	/// CRS based on a two dimensional coordinate system such as a geographic or
	/// a projected coordinate system with a horizontal datum. The other is a 
	/// vertical CRS which is a one-dimensional coordinate system with a vertical
	/// datum.
	/// </summary>
	public interface ICompoundCoordinateSystem : ICoordinateSystem
	{
		/// <summary>
		/// Gets first sub-coordinate system.
		/// </summary>
		CoordinateSystem HeadCS { get; }
		/// <summary>
		/// Gets second sub-coordinate system.
		/// </summary>
		CoordinateSystem TailCS { get; }
	}
}
