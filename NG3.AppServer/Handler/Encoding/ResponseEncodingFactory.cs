using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace NG3.AppServer.Handler.Encoding
{
    class ResponseEncodingFactory
    {
        public IResponseEncoding CreateResponseEncoding()
        {
            return new GZipEncoding();
        }
    }
}
