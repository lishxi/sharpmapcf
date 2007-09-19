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

namespace SharpMap.Rendering
{
	/// <summary>
	/// Exception thrown when rendering fails.
	/// </summary>
	public class RenderException : Exception
	{
		/// <summary>
		/// Creates a new RenderException instance.
		/// </summary>
		public RenderException()
		{
		}

		/// <summary>
		/// Creates a new RenderException instance with the given message.
		/// </summary>
		/// <param name="message">The message to include in the exception.</param>
		public RenderException(string message) : base(message)
		{
		}

		/// <summary>
		/// Exception thrown when layer rendering has failed.
		/// </summary>
		/// <param name="message">The message to include in the exception.</param>
		/// <param name="inner">A related, usually causing, exception.</param>
		public RenderException(string message, Exception inner) : base(message,inner)
		{
		}
	}
}
