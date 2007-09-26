﻿// Copyright 2006, 2007 - Rory Plaire (codekaizen@gmail.com)
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

using System.Collections.Generic;
using System.IO;
using SharpMap.Geometries;

namespace SharpMap.Data
{
    /// <summary>
    /// NOTE: this class is just an experimental attempt to express a view on 
    /// raster data. It _will_ change.
    /// </summary>
    public class RasterDataView : Stream, IEnumerable<byte>
    {
        public RasterDataView(RasterBand band)
        {

        }

        public RasterDataView(RasterDataSet dataSet)
        {

        }

        public RasterDataView(RasterDataSet dataSet, Geometry intersectionFilter, double scale)
        {

        }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for 
        /// this stream and causes any buffered data to be written to 
        /// the underlying device.
        /// </summary>
        ///
        /// <exception cref="T:System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <filterpriority>2</filterpriority>
        public override void Flush()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        ///
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        ///
        /// <param name="offset">
        /// A byte offset relative to the origin parameter.
        /// </param>
        /// <param name="origin">
        /// A value of type <see cref="T:System.IO.SeekOrigin"></see> indicating the 
        /// reference point used to obtain the new position.
        /// </param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="T:System.NotSupportedException">
        /// The stream does not support seeking, such as if the stream is 
        /// constructed from a pipe or console output.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        /// Methods were called after the stream was closed. 
        /// </exception>
        /// <filterpriority>1</filterpriority>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        ///
        /// <param name="value">The desired length of the current stream in bytes. </param>
        /// <exception cref="T:System.NotSupportedException">
        /// The stream does not support both writing and seeking, such as if the stream 
        /// is constructed from a pipe or console output.
        /// </exception>
        /// <exception cref="T:System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        /// Methods were called after the stream was closed.
        /// </exception>
        /// <filterpriority>2</filterpriority>
        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes 
        /// from the current stream and advances the position within the stream 
        /// by the number of bytes read.
        /// </summary>
        ///
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than 
        /// the number of bytes requested if that many bytes are not currently available, 
        /// or zero (0) if the end of the stream has been reached.
        /// </returns>
        ///
        /// <param name="offset">
        /// The zero-based byte offset in buffer at which to begin storing the 
        /// data read from the current stream.
        /// </param>
        /// <param name="count">
        /// The maximum number of bytes to be read from the current stream.
        /// </param>
        /// <param name="buffer">
        /// An array of bytes. When this method returns, the buffer contains the 
        /// specified byte array with the values between offset and (offset + count - 1) 
        /// replaced by the bytes read from the current source.
        /// </param>
        /// <exception cref="T:System.ArgumentException">
        /// The sum of offset and count is larger than the buffer length.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        /// Methods were called after the stream was closed.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// The stream does not support reading.
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="buffer "/> is null.
        /// </exception>
        /// <exception cref="T:System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="offset"/> or <paramref name="count"/> is negative.
        /// </exception>
        /// <filterpriority>1</filterpriority>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current 
        /// stream and advances the current position within this stream by the number of 
        /// bytes written.
        /// </summary>
        ///
        /// <param name="offset">
        /// The zero-based byte offset in buffer at which to begin copying 
        /// bytes to the current stream.
        /// </param>
        /// <param name="count">
        /// The number of bytes to be written to the current stream.
        /// </param>
        /// <param name="buffer">
        /// An array of bytes. This method copies count bytes from buffer to the current stream.
        /// </param>
        /// <exception cref="T:System.IO.IOException">
        /// An I/O error occurs. 
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// The stream does not support writing.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        /// Methods were called after the stream was closed.
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="buffer"/> is null. 
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// The sum of <paramref name="offset"/> and <paramref name="count"/> 
        /// is greater than the buffer length. 
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="offset"/> or <paramref name="count"/> is negative. 
        /// </exception>
        /// <filterpriority>1</filterpriority>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the 
        /// current stream supports reading.
        /// </summary>
        ///
        /// <returns>
        /// true if the stream supports reading; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool CanRead
        {
            get { throw new System.NotImplementedException(); }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the 
        /// current stream supports seeking.
        /// </summary>
        ///
        /// <returns>
        /// true if the stream supports seeking; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool CanSeek
        {
            get { throw new System.NotImplementedException(); }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the 
        /// current stream supports writing.
        /// </summary>
        ///
        /// <returns>
        /// true if the stream supports writing; otherwise, false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool CanWrite
        {
            get { throw new System.NotImplementedException(); }
        }

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        ///
        /// <returns>
        /// A long value representing the length of the stream in bytes.
        /// </returns>
        ///
        /// <exception cref="T:System.NotSupportedException">
        /// A class derived from Stream does not support seeking. 
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        /// Methods were called after the stream was closed. 
        /// </exception>
        /// <filterpriority>1</filterpriority>
        public override long Length
        {
            get { throw new System.NotImplementedException(); }
        }

        /// <summary>
        /// When overridden in a derived class, gets or sets the position 
        /// within the current stream.
        /// </summary>
        ///
        /// <returns>
        /// The current position within the stream.
        /// </returns>
        ///
        /// <exception cref="T:System.IO.IOException">
        /// An I/O error occurs.
        /// </exception>
        /// <exception cref="T:System.NotSupportedException">
        /// The stream does not support seeking.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        /// Methods were called after the stream was closed.
        /// </exception><
        /// filterpriority>1</filterpriority>
        public override long Position
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        #region IEnumerable<byte> Members

        public IEnumerator<byte> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
