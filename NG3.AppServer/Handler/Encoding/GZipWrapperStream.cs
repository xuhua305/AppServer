using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace NG3.AppServer.Handler.Encoding
{
    internal class GZipWrapperStream : GZipStream, ICloseEx
    {
        public GZipWrapperStream(Stream stream, CompressionMode mode)
            : base(stream, mode, false)
        {
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state)
        {
            IAsyncResult asyncResult;
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0 || offset > (int)buffer.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (size < 0 || size > (int)buffer.Length - offset)
            {
                throw new ArgumentOutOfRangeException("size");
            }
            try
            {
                asyncResult = base.BeginRead(buffer, offset, size, callback, state);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                try
                {
                    if (NclUtilities.IsFatal(exception))
                    {
                        throw;
                    }
                    if (exception is InvalidDataException || exception is InvalidOperationException || exception is IndexOutOfRangeException)
                    {
                        this.Close();
                    }
                }
                catch
                {
                }
                throw exception;
            }
            return asyncResult;
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            int num;
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            try
            {
                num = base.EndRead(asyncResult);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                try
                {
                    if (NclUtilities.IsFatal(exception))
                    {
                        throw;
                    }
                    if (exception is InvalidDataException || exception is InvalidOperationException || exception is IndexOutOfRangeException)
                    {
                        this.Close();
                    }
                }
                catch
                {
                }
                throw exception;
            }
            return num;
        }

        public override int Read(byte[] buffer, int offset, int size)
        {
            int num;
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0 || offset > (int)buffer.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (size < 0 || size > (int)buffer.Length - offset)
            {
                throw new ArgumentOutOfRangeException("size");
            }
            try
            {
                num = base.Read(buffer, offset, size);
            }
            catch (Exception exception1)
            {
                Exception exception = exception1;
                try
                {
                    if (NclUtilities.IsFatal(exception))
                    {
                        throw;
                    }
                    if (exception is InvalidDataException || exception is InvalidOperationException || exception is IndexOutOfRangeException)
                    {
                        this.Close();
                    }
                }
                catch
                {
                }
                throw exception;
            }
            return num;
        }

        void ICloseEx.CloseEx(CloseExState closeState)
        {
            ICloseEx baseStream = base.BaseStream as ICloseEx;
            if (baseStream == null)
            {
                this.Close();
                return;
            }
            baseStream.CloseEx(closeState);
        }
    }
}
