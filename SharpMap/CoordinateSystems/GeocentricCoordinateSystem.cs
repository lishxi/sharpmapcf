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
	/// A 3D coordinate system, with its origin at the center of the Earth.
	/// </summary>
	public class GeocentricCoordinateSystem : CoordinateSystem, IGeocentricCoordinateSystem
	{
		internal GeocentricCoordinateSystem(IHorizontalDatum datum, ILinearUnit linearUnit, IPrimeMeridian primeMeridian, List<AxisInfo> axisinfo,
			string name, string authority, long code, string alias, 
			string remarks, string abbreviation)
			: base(name, authority, code, alias, abbreviation, remarks)
		{
			_HorizontalDatum = datum;
			_LinearUnit = linearUnit;
			_Primemeridan = primeMeridian;
			if (axisinfo.Count != 3)
				throw new ArgumentException("Axis info should contain three axes for geocentric coordinate systems");
			base.AxisInfo = axisinfo;
		}

		#region Predefined geographic coordinate systems

		/// <summary>
		/// Creates a geocentric coordinate system based on the WGS84 ellipsoid, suitable for GPS measurements
		/// </summary>
		public static IGeocentricCoordinateSystem WGS84
		{
			get
			{
				return new CoordinateSystemFactory().CreateGeocentricCoordinateSystem("WGS84 Geocentric",
					SharpMap.CoordinateSystems.HorizontalDatum.WGS84, SharpMap.CoordinateSystems.LinearUnit.Metre, 
					SharpMap.CoordinateSystems.PrimeMeridian.Greenwich);
			}
		}

		#endregion

		#region IGeocentricCoordinateSystem Members

		private IHorizontalDatum _HorizontalDatum;

		/// <summary>
		/// Returns the HorizontalDatum. The horizontal datum is used to determine where
		/// the centre of the Earth is considered to be. All coordinate points will be 
		/// measured from the centre of the Earth, and not the surface.
		/// </summary>
		public IHorizontalDatum HorizontalDatum
		{
			get { return _HorizontalDatum; }
			set { _HorizontalDatum = value; }
		}

		private ILinearUnit _LinearUnit;

		/// <summary>
		/// Gets the units used along all the axes.
		/// </summary>
		public ILinearUnit LinearUnit
		{
			get { return _LinearUnit; }
			set { _LinearUnit = value; }
		}

		/// <summary>
		/// Gets units for dimension within coordinate system. Each dimension in 
		/// the coordinate system has corresponding units.
		/// </summary>
		/// <param name="dimension">Dimension</param>
		/// <returns>Unit</returns>
		public override IUnit GetUnits(int dimension)
		{
			return _LinearUnit;
		}

		private IPrimeMeridian _Primemeridan;

		/// <summary>
		/// Returns the PrimeMeridian.
		/// </summary>
		public IPrimeMeridian PrimeMeridian
		{
			get { return _Primemeridan; }
			set { _Primemeridan = value; }
		}

		/// <summary>
		/// Returns the Well-known text for this object
		/// as defined in the simple features specification.
		/// </summary>
		public override string Wkt
		{
			get
			{
				StringBuilder sb = new StringBuilder();
#if !CFBuild
                sb.AppendFormat("GEOCCS[\"{0}\", {1}, {2}, {3}", Name, HorizontalDatum.Wkt, PrimeMeridian.Wkt, LinearUnit.Wkt);
#else
                sb.AppendFormat(null, "GEOCCS[\"{0}\", {1}, {2}, {3}", Name, HorizontalDatum.Wkt, PrimeMeridian.Wkt, LinearUnit.Wkt);
#endif
				
                //Skip axis info if they contain default values				
                if (AxisInfo.Count != 3 ||
                    AxisInfo[0].Name != "X" || AxisInfo[0].Orientation != AxisOrientationEnum.Other ||
                    AxisInfo[1].Name != "Y" || AxisInfo[1].Orientation != AxisOrientationEnum.East ||
                    AxisInfo[2].Name != "Z" || AxisInfo[2].Orientation != AxisOrientationEnum.North)
                {
                    for (int i = 0; i < AxisInfo.Count; i++)
                    {
#if !CFBuild
                        sb.AppendFormat(", {0}", GetAxis(i).WKT);
#else
                        sb.AppendFormat(null, ", {0}", GetAxis(i).WKT);
#endif
                    }
                }

                if (!String.IsNullOrEmpty(Authority) && AuthorityCode > 0)
                {
#if !CFBuild
                    sb.AppendFormat(", AUTHORITY[\"{0}\", \"{1}\"]", Authority, AuthorityCode);                        sb.AppendFormat(", {0}", GetAxis(i).WKT);
#else
                    sb.AppendFormat(null, ", AUTHORITY[\"{0}\", \"{1}\"]", Authority, AuthorityCode);
#endif
                }

				sb.Append("]");
				return sb.ToString();
			}
		}

		/// <summary>
		/// Gets an XML representation of this object
		/// </summary>
		public override string Xml
		{
			get
			{
				StringBuilder sb = new StringBuilder();

                sb.AppendFormat(NumberFormat,
					"<CS_CoordinateSystem Dimension=\"{0}\"><CS_GeocentricCoordinateSystem>{1}",
					Dimension, InfoXml);

                foreach (AxisInfo ai in AxisInfo)
                {
                    sb.Append(ai.XML);
                }
#if !CFBuild
				sb.AppendFormat("{0}{1}{2}</CS_GeocentricCoordinateSystem></CS_CoordinateSystem>",
					HorizontalDatum.Xml, LinearUnit.Xml, PrimeMeridian.Xml);

#else
                sb.AppendFormat(null, "{0}{1}{2}</CS_GeocentricCoordinateSystem></CS_CoordinateSystem>",
                    HorizontalDatum.Xml, LinearUnit.Xml, PrimeMeridian.Xml);

#endif

				return sb.ToString();
			}
		}

		/// <summary>
		/// Checks whether the values of this instance is equal to the values of another instance.
		/// Only parameters used for coordinate system are used for comparison.
		/// Name, abbreviation, authority, alias and remarks are ignored in the comparison.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns>True if equal</returns>
		public override bool EqualParams(object obj)
		{
			GeocentricCoordinateSystem other = obj as GeocentricCoordinateSystem;

			if (other == null)
			{
				return false;
			}
			
			return other.HorizontalDatum.EqualParams(HorizontalDatum) &&
				other.LinearUnit.EqualParams(LinearUnit) &&
				other.PrimeMeridian.EqualParams(PrimeMeridian);
		}
		#endregion
	}
}
