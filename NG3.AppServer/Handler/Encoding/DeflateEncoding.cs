using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace NG3.AppServer.Handler.Encoding
{
    class DeflateEncoding : IResponseEncoding
    {
        public byte[] EncodingResponse(byte[] inputBytes)
        {
            MemoryStream stream = new MemoryStream();
            DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Compress);
            deflateStream.Write(inputBytes, 0, inputBytes.Length);
            deflateStream.Close();
            return stream.ToArray();
        }
    }
}
