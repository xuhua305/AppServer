using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace NG3.AppServer.Handler.Encoding
{
    class GZipEncoding : IResponseEncoding
    {
        public byte[] EncodingResponse(byte[] inputBytes)
        {
            MemoryStream stream = new MemoryStream();
            GZipStream gZipStream = new GZipStream(stream, CompressionMode.Compress);
            gZipStream.Write(inputBytes, 0, inputBytes.Length);
            gZipStream.Close();
            return stream.ToArray();
        }
    }
}
