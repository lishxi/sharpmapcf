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

using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SharpMap.Utilities
{
    /// <summary>
    /// Generates hash values for data streams and objects.
    /// </summary>
    public static class Hash
    {
        private const string _digits = "0123456789ABCDEF";
        private static readonly SHA1Managed _hash = new SHA1Managed();

        /// <summary>
        /// Computes a hash value of a stream of data as a string.
        /// </summary>
        /// <param name="data">The data to compute the hash for.</param>
        /// <returns>A string which uniquely identifies the data.</returns>
        public static string AsString(Stream data)
        {
            long streamPos = data.Position;
            data.Seek(0, SeekOrigin.Begin);
            byte[] hashValue;

            lock (_hash)
            {
                hashValue = _hash.ComputeHash(data);
            }

            data.Position = streamPos;

            StringBuilder builder = new StringBuilder();

            foreach (byte b in hashValue)
            {
                builder.Append(_digits[b >> 4]);
                builder.Append(_digits[b & 0x07]);
            }

            return builder.ToString();
        }
    }
}