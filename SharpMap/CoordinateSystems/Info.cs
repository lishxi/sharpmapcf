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
using System.Globalization;

namespace SharpMap.CoordinateSystems
{
	/// <summary>
	/// The Info object implements the standard metadata
    /// for spatial reference objects.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The metadata items �Abbreviation�, �Alias�, �Authority�, �AuthorityCode�, �Name� and �Remarks� 
    /// were specified in the Simple Features interfaces, so they have been kept here.
    /// </para>
    /// <para>
    /// This specification does not dictate what the contents of these items 
    /// should be. However, the following guidelines are suggested:
    /// </para>
    /// <para>
    /// When <see cref="ICoordinateSystemAuthorityFactory"/> is used to create an object, 
    /// the �Authority� and 'AuthorityCode' values should be set to the authority name of the 
    /// factory object, and the authority code supplied by the client, respectively. The other 
    /// values may or may not be set. (If the authority is EPSG, the implementer may consider 
    /// using the corresponding metadata values in the EPSG tables.)
    /// </para>
    /// <para>
    /// When <see cref="CoordinateSystemFactory"/> creates an object, the 'Name' should 
    /// be set to the value supplied by the client. All of the other metadata items should 
    /// be left empty.</para>
    /// </remarks>
	public abstract class Info : IInfo
    {
        private static NumberFormatInfo _numberFormat = new CultureInfo("en-US", false).NumberFormat;

        public static NumberFormatInfo NumberFormat
        {
            get { return _numberFormat; }
            set { _numberFormat = value; }
        }

		/// <summary>
        /// Creates a new Info object.
		/// </summary>
		/// <remarks>
		/// <para>
        /// The metadata items �Abbreviation�, �Alias�, �Authority�, �AuthorityCode�, �Name� and �Remarks� 
		/// were specified in the Simple Features interfaces, so they have been kept here.
        /// </para>
		/// <para>
        /// This specification does not dictate what the contents of these items 
		/// should be. However, the following guidelines are suggested:
        /// </para>
		/// <para>
        /// When <see cref="ICoordinateSystemAuthorityFactory"/> is used to create an object, 
		/// the �Authority� and 'AuthorityCode' values should be set to the authority name of the 
		/// factory object, and the authority code supplied by the client, respectively. The other 
		/// values may or may not be set. (If the authority is EPSG, the implementer may consider 
		/// using the corresponding metadata values in the EPSG tables.)
        /// </para>
		/// <para>
        /// When <see cref="CoordinateSystemFactory"/> creates an object, the 'Name' should 
		/// be set to the value supplied by the client. All of the other metadata items should 
        /// be left empty.</para>
		/// </remarks>
		/// <param name="name">Name</param>
		/// <param name="authority">Authority name</param>
		/// <param name="code">Authority-specific identification code</param>
		/// <param name="alias">Alias</param>
		/// <param name="abbreviation">Abbreviation</param>
		/// <param name="remarks">Provider-supplied remarks</param>
		internal Info(
						string name,
						string authority,
						long code,
						string alias,
						string abbreviation,
						string remarks)
		{
			_name = name;
			_authority = authority;
			_code = code;
			_alias = alias;
			_abbreviation = abbreviation;
			_remarks = remarks;
		}

		#region IInfo Members

		private string _name;
		private string _authority;
		private long _code;
		private string _remarks;
		private string _abbreviation;
		private string _alias;

		/// <summary>
		/// Gets or sets the name of the object.
		/// </summary>
		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		/// <summary>
		/// Gets or sets the authority name for this object, e.g., "EPSG",
		/// is this is a standard object with an authority specific
		/// identity code. Returns "CUSTOM" if this is a custom object.
		/// </summary>
		public string Authority
		{
			get { return _authority; }
			set { _authority = value; }
		}

		/// <summary>
		/// Gets or sets the authority specific identification code of the object
		/// </summary>
		public long AuthorityCode
		{
			get { return _code; }
			set { _code = value; }
		}

		/// <summary>
		/// Gets or sets the alias of the object.
		/// </summary>
		public string Alias
		{
			get { return _alias; }
			set { _alias = value; }
		}

		/// <summary>
		/// Gets or sets the abbreviation of the object.
		/// </summary>
		public string Abbreviation
		{
			get { return _abbreviation; }
			set { _abbreviation = value; }
		}

		/// <summary>
		/// Gets or sets the provider-supplied remarks for the object.
		/// </summary>
		public string Remarks
		{
			get { return _remarks; }
			set { _remarks = value; }
		}

		/// <summary>
		/// Returns the Well-known text for this object
		/// as defined in the simple features specification.
		/// </summary>
		public override string ToString()
		{
			return Wkt;
		}

		/// <summary>
		/// Returns the Well-known text for this object
		/// as defined in the simple features specification.
		/// </summary>
		public abstract string Wkt { get;}

		/// <summary>
		/// Gets an XML representation of this object.
		/// </summary>
		public abstract string Xml { get; }

		/// <summary>
		/// Returns an XML string of the info object
		/// </summary>
		internal string InfoXml
		{
			get
			{
				StringBuilder sb = new StringBuilder();
#if !CFBuild
				sb.AppendFormat("<CS_Info");
#else
       			sb.Append("<CS_Info");
#endif

				if (AuthorityCode > 0)
				{
#if !CFBuild
					sb.AppendFormat(" AuthorityCode=\"{0}\"", AuthorityCode);
#else
                    sb.AppendFormat(null," AuthorityCode=\"{0}\"", AuthorityCode);
#endif
				}

				if (!String.IsNullOrEmpty(Abbreviation))
				{
#if !CFBuild
					sb.AppendFormat(" Abbreviation=\"{0}\"", Abbreviation);
#else
                    sb.AppendFormat(null, " Abbreviation=\"{0}\"", Abbreviation);
#endif
				}

				if (!String.IsNullOrEmpty(Authority))
				{
#if !CFBuild
					sb.AppendFormat(" Authority=\"{0}\"", Authority);
#else
                    sb.AppendFormat(null, " Authority=\"{0}\"", Authority);
#endif
	
				}

				if (!String.IsNullOrEmpty(Name))
				{
#if !CFBuild
					sb.AppendFormat(" Name=\"{0}\"", Name);
#else
                    sb.AppendFormat(null, " Name=\"{0}\"", Name);
#endif

				}

				sb.Append("/>");
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
        public abstract bool EqualParams(object obj);

		#endregion
	}
}
