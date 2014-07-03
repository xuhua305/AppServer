using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using NG3.AppServer.Handler.Encoding;

namespace NG3.AppServer.Parse
{
    sealed class ResponseParse
    {
        public static string MakeResponseHeaders(int statusCode, string moreHeaders, int contentLength, bool keepAlive)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("HTTP/1.1 " + statusCode + " " + HttpWorkerRequest.GetStatusDescription(statusCode) + "\r\n");
            sb.Append("Server: NGWebAppServer/" + Messages.VersionString + "\r\n");
            sb.Append("Date: " + DateTime.Now.ToUniversalTime().ToString("R", DateTimeFormatInfo.InvariantInfo) + "\r\n");

            if (contentLength >= 0)
            {
                sb.Append("Content-Length: " + contentLength + "\r\n");
            }

            if (moreHeaders != null)
            {
                sb.Append(moreHeaders);
            }

            if (!keepAlive)
            {
                sb.Append("Connection: Close\r\n");
            }

            sb.Append("Content-Encoding:gzip\r\n");

            sb.Append("\r\n");
            return sb.ToString();
        }

        public static byte[] EncodingBodyContent(byte[] bodyBytes)
        {
            try
            {
                IResponseEncoding responseEncoding = new ResponseEncodingFactory().CreateResponseEncoding();
                return responseEncoding.EncodingResponse(bodyBytes);
            }
            catch (Exception ex)
            {
                
                throw;
            }

        }
    }
}
