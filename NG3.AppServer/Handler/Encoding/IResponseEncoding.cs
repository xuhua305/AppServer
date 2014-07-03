using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace NG3.AppServer.Handler.Encoding
{
    interface IResponseEncoding
    {
        byte[] EncodingResponse(byte[] inputBytes);
    }
}
