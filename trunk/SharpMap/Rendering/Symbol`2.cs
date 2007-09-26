﻿using System;
using System.IO;
#if !CFBuild
using System.Runtime.Serialization;
#endif
using SharpMap.Utilities;
using IMatrixD = NPack.Interfaces.IMatrix<NPack.DoubleComponent>;
using IAffineMatrixD = NPack.Interfaces.IAffineTransformMatrix<NPack.DoubleComponent>;
using IVectorD = NPack.Interfaces.IVector<NPack.DoubleComponent>;

namespace SharpMap.Rendering
{
    /// <summary>
    /// Represents a graphical symbol used for point data on a map.
    /// </summary>
    public abstract class Symbol<TPoint, TSize> : ICloneable, IDisposable
#if !CFBuild
        ,ISerializable
#endif
        where TPoint : IVectorD
        where TSize : IVectorD
    {
        private ColorMatrix _colorTransform = ColorMatrix.Identity;
        private Stream _symbolData;
        private string _symbolDataHash;
        private bool _disposed;
        private IAffineMatrixD _rotationTransform;
        private IAffineMatrixD _scalingTransform;
        private IAffineMatrixD _translationTransform;
        private TSize _size;

        #region Object construction / disposal

        protected Symbol()
        {
            _symbolData = new MemoryStream(new byte[] {0x0, 0x0, 0x0, 0x0});
            initMatrixes();
        }

        protected Symbol(TSize size) : this()
        {
            _size = size;
        }

        protected Symbol(Stream symbolData, TSize size)
        {
            _size = size;

            if (!symbolData.CanSeek)
            {
                if (symbolData.Position != 0)
                {
                    throw new InvalidOperationException(
                        "Symbol data stream isn't at the beginning, and it can't be repositioned");
                }

                MemoryStream copy = new MemoryStream();

                using (BinaryReader reader = new BinaryReader(symbolData))
                {
                    copy.Write(reader.ReadBytes((int) symbolData.Length), 0, (int) symbolData.Length);
                }

                symbolData = copy;
            }

            setSymbolData(symbolData);
            initMatrixes();
        }

        #region Dispose Pattern

        ~Symbol()
        {
            Dispose(false);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            Dispose(true);
            IsDisposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// Gets a value indicating if the <see cref="Symbol{TPoint,TSize}"/>
        /// is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return _disposed; }
            private set { _disposed = value; }
        }

        protected void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (_symbolData != null)
                {
#if !CFBuild
                    _symbolData.Dispose();
#else
                    _symbolData.Close();
#endif
                }
            }
        }

        #endregion

        #endregion

        #region Public properties

        /// <summary>
        /// Gets or sets a <see cref="IAffineMatrixD"/> object used 
        /// to transform this <see cref="IAffineMatrixD"/>.
        /// </summary>
        public IAffineMatrixD AffineTransform
        {
            get
            {
                CheckDisposed();

                IAffineMatrixD concatenated = CreateMatrix(
                    _rotationTransform
                    .Multiply(_scalingTransform)
                    .Multiply(_translationTransform));

                return CreateMatrix(concatenated);
            }
            // TODO: need to compute a decomposition to get _rotationTransform, _scaleTransform and _translationTransform
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets or sets a <see cref="ColorMatrix"/> used to change the color 
        /// of this symbol.
        /// </summary>
        public ColorMatrix ColorTransform
        {
            get { return _colorTransform; }
            set { _colorTransform = value; }
        }

        /// <summary>
        /// Gets or sets a vector by which to offset the symbol.
        /// </summary>
        public TPoint Offset
        {
            get
            {
                CheckDisposed();
                return GetOffset(_translationTransform);
            }
            set
            {
                CheckDisposed();
                SetOffset(value);
            }
        }

        /// <summary>
        /// Gets or sets the size of this symbol.
        /// </summary>
        public TSize Size
        {
            get
            {
                CheckDisposed();
                return _size;
            }
            set
            {
                CheckDisposed();
                _size = value;
            }
        }

        /// <summary>
        /// Gets a stream containing the <see cref="Symbol{TPoint,TSize}"/> 
        /// data.
        /// </summary>
        /// <remarks>
        /// This is often a bitmap or a vector-based image.
        /// </remarks>
        public Stream SymbolData
        {
            get
            {
                CheckDisposed();
                return _symbolData;
            }
            private set
            {
                CheckDisposed();
                if (value == null) throw new ArgumentNullException("value");
                setSymbolData(value);
            }
        } 
        #endregion

        #region Protected properties

        /// <summary>
        /// Gets or sets a rotation matrix by which to rotate this symbol.
        /// </summary>
        protected IAffineMatrixD Rotation
        {
            get
            {
                CheckDisposed(); 
                return _rotationTransform;
            }
            set
            {
                CheckDisposed();

                if (value == null)
                {
                    _rotationTransform = CreateIdentityMatrix();
                }
                else
                {
                    _rotationTransform = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a scale matrix by which to scale this symbol.
        /// </summary>
        protected IAffineMatrixD Scale
        {
            get
            {

                CheckDisposed(); 
                return _scalingTransform;
            }
            set
            {
                CheckDisposed();

                if (value == null)
                {
                    _scalingTransform = CreateIdentityMatrix();
                }
                else
                {
                    _scalingTransform = value;
                }
            }
        }

        protected string SymbolDataHash
        {
            get { return _symbolDataHash; }
        }

        #endregion

        #region Protected methods
        protected void CheckDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().ToString());
            }
        }

        protected abstract Symbol<TPoint, TSize> CreateNew(TSize size);
        protected abstract IAffineMatrixD CreateIdentityMatrix();
        protected abstract IAffineMatrixD CreateMatrix(IMatrixD matrix);
        protected abstract TPoint GetOffset(IAffineMatrixD translationMatrix);
        protected abstract void SetOffset(TPoint offset); 
        #endregion

        #region ICloneable Members

        /// <summary>
        /// Clones this symbol.
        /// </summary>
        /// <returns>
        /// A duplicate of this <see cref="Symbol{TPoint,TSize}"/>.
        /// </returns>
        public Symbol<TPoint, TSize> Clone()
        {
            Symbol<TPoint, TSize> clone = CreateNew(Size);

            // Record the original position
            long streamPos = _symbolData.Position;
            _symbolData.Seek(0, SeekOrigin.Begin);

            byte[] buffer = new byte[_symbolData.Length];
            _symbolData.Read(buffer, 0, buffer.Length);
            MemoryStream copy = new MemoryStream(buffer);

            // Restore the original position
            _symbolData.Position = streamPos;

            clone.SymbolData = copy;
            clone._symbolDataHash = _symbolDataHash;
            clone.ColorTransform = ColorTransform.Clone();
            clone._rotationTransform = CreateMatrix(_rotationTransform.Clone());
            clone._translationTransform = CreateMatrix(_translationTransform.Clone());
            clone._scalingTransform = CreateMatrix(_scalingTransform.Clone());
            return clone;
        }

        /// <summary>
        /// Clones this symbol.
        /// </summary>
        /// <returns>
        /// A duplicate of this <see cref="Symbol{TPoint,TSize}"/> 
        /// as an object reference.
        /// </returns>
        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion

        #region ISerializable Members

#if !CFBuild
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
#endif
        #endregion

        #region Private helper methods

        private void initMatrixes()
        {
            _rotationTransform = CreateIdentityMatrix();
            _translationTransform = CreateIdentityMatrix();
            _scalingTransform = CreateIdentityMatrix();
        }


        private void setSymbolData(Stream symbolData)
        {
            _symbolData = symbolData;
            _symbolDataHash = Hash.AsString(_symbolData);
        }
        #endregion
    }
}