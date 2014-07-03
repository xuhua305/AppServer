using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using NG3.AppServer.Connector;

namespace NG3.AppServer.Parse
{
    /// <summary>
    /// 请求解析
    /// </summary>
    internal class RequestParse
    {
        private const string HeadAndBodySplitChar = "\r\n\r\n";
        private const string HeadSplitChar = "\r\n";

        /// <summary>
        /// 解析Request的请求行
        /// </summary>
        /// <param name="lineStr"></param>
        /// <param name="requestInfo"></param>
        private static void ParseLine(RequestInfo requestInfo)
        {
            try
            {
                string[] splitArray = requestInfo.LineStr.Split(' ');
                Debug.Assert(splitArray.Length == 3);
                requestInfo.HttpVerbName = splitArray[0];
                requestInfo.RequestUrl = splitArray[1];
                requestInfo.Protocol = splitArray[2];

                int iqs = requestInfo.RequestUrl.IndexOf("?");
                if (iqs > 0)
                {
                    requestInfo.UriPath = requestInfo.RequestUrl.Substring(0, iqs);
                    requestInfo.QueryString = requestInfo.RequestUrl.Substring(iqs + 1);
                    requestInfo.QueryStringBytes = Encoding.Default.GetBytes(requestInfo.QueryString);
                }
                else
                {
                    requestInfo.UriPath = requestInfo.RequestUrl;
                    requestInfo.QueryStringBytes = new byte[0];
                }


                if (requestInfo.UriPath.IndexOf('%') >= 0)
                {
                    requestInfo.UriPath = HttpUtility.UrlDecode(requestInfo.UriPath, Encoding.UTF8);
                    iqs = requestInfo.RequestUrl.IndexOf('?');
                    if (iqs >= 0)
                    {
                        requestInfo.RequestUrl = requestInfo.UriPath + requestInfo.RequestUrl.Substring(iqs);
                    }
                    else
                    {
                        requestInfo.RequestUrl = requestInfo.UriPath;
                    }
                }

                // path info

                int lastDot = requestInfo.UriPath.LastIndexOf('.');
                int lastSlh = requestInfo.UriPath.LastIndexOf('/');

                if (lastDot >= 0 && lastSlh >= 0 && lastDot < lastSlh)
                {
                    int ipi = requestInfo.UriPath.IndexOf('/', lastDot);
                    requestInfo.FilePath = requestInfo.UriPath.Substring(0, ipi);
                    requestInfo.PathInfo = requestInfo.UriPath.Substring(ipi);
                }
                else
                {
                    requestInfo.FilePath = requestInfo.UriPath;
                    requestInfo.PathInfo = String.Empty;
                }
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        private static void ParseHeader(RequestInfo requestInfo)
        {
            try
            {
                string[] splitArray = requestInfo.HeadStr.Split(new string[]{"\r\n"},StringSplitOptions.None);
                requestInfo.KnownRequestHeaders = new string[SimpleWorkerRequest.RequestHeaderMaximum];
                IList<string> headers = new List<string>();
                
                foreach(string str in splitArray)
                {
                    int childIndex = str.IndexOf(':');
                    string[] childSplitArray = new string[] { str.Substring(0, childIndex), str.Substring(childIndex+1,str.Length-childIndex-1).Trim() };
                    Debug.Assert(childSplitArray.Length ==2);
                    int knowIndex = SimpleWorkerRequest.GetKnownRequestHeaderIndex(childSplitArray[0]);
                    if (knowIndex >= 0)
                    {
                        requestInfo.KnownRequestHeaders[knowIndex] = childSplitArray[1];
                    }
                    else
                    {
                        headers.Add(childSplitArray[0]);
                        headers.Add(childSplitArray[1]);
                    }

                    string lowerHeaderName = childSplitArray[0].ToLower();
                    switch(lowerHeaderName)
                    {
                        case "host":
                            requestInfo.Host = childSplitArray[1];
                            if (requestInfo.Host.Contains(":"))
                            {
                                string[] urlArray = requestInfo.Host.Split(':');
                                requestInfo.HostUrl = urlArray[0];
                                requestInfo.HostPort = Convert.ToInt32(urlArray[1]);
                            }
                            else
                            {
                                requestInfo.HostUrl = requestInfo.Host;
                            }
                            break;
                        case "connection":
                            requestInfo.Connection = childSplitArray[1];
                            break;
                        case "cookie":
                            requestInfo.Cookie = childSplitArray[1];
                            break;
                        case "user-agent":
                            requestInfo.UserAgent = childSplitArray[1];
                            break;
                        case "cache-control":
                            requestInfo.CacheControl = childSplitArray[1];
                            break;
                        case "accept":
                            requestInfo.Accept = childSplitArray[1];
                            break;
                        case "referer":
                            requestInfo.Referer = childSplitArray[1];
                            break;
                        case "accept-encoding":
                            requestInfo.AcceptEncoding = childSplitArray[1];
                            break;
                        case "accept-language":
                            requestInfo.AcceptLanguage = childSplitArray[1];
                            break;
                        case "if-modified-since":
                            requestInfo.IfModifiedSince = childSplitArray[1];
                            break;
                        case "content-length":
                            requestInfo.ContentLength = Convert.ToInt32(childSplitArray[1]);
                            break;
                        case "content-type":
                            requestInfo.ContentType = childSplitArray[1];
                            break;
                        default:
                            break;
                    }
                }

                string contentLengthValue = requestInfo.KnownRequestHeaders[SimpleWorkerRequest.HeaderContentLength];
                if (contentLengthValue != null)
                {
                    requestInfo.ContentLength = Int32.Parse(contentLengthValue, CultureInfo.InvariantCulture);
                }
                int n = headers.Count / 2;
                requestInfo.UnknownRequestHeaders = new string[n][];
                int j = 0;

                for (int i = 0; i < n; i++)
                {
                    requestInfo.UnknownRequestHeaders[i] = new string[2];
                    requestInfo.UnknownRequestHeaders[i][0] = headers[j++];
                    requestInfo.UnknownRequestHeaders[i][1] = headers[j++];
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static RequestInfo Parse(Token token)
        {
            Debug.Assert(token != null);
            Debug.Assert(token.Connection != null);
            RequestInfo requestInfo = new RequestInfo();
            requestInfo.AcceptSocket = new SocketProxy(token.Connection);
            string requestStr = token.ToString();

            //解析Line,Header和Body
            string[] splitHeaderAndBody = requestStr.Split(new string[] { HeadAndBodySplitChar }, StringSplitOptions.None);
            Debug.Assert(splitHeaderAndBody.Length == 2);
            requestInfo.BodyStr = splitHeaderAndBody[1];
            requestInfo.BodyLength = requestInfo.BodyStr.Length;
            requestInfo.BodyBytes = Encoding.Default.GetBytes(requestInfo.BodyStr);
            int lineEndIndex = splitHeaderAndBody[0].IndexOf(HeadSplitChar);
            requestInfo.LineStr = splitHeaderAndBody[0].Substring(0, lineEndIndex);
            requestInfo.HeadStr = splitHeaderAndBody[0].Substring(lineEndIndex+2, splitHeaderAndBody[0].Length - lineEndIndex-2);

            ParseLine(requestInfo);
            ParseHeader(requestInfo);


            return requestInfo;
        }
        
    }
}
