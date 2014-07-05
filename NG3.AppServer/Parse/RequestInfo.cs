using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using NG3.AppServer.Connector;

namespace NG3.AppServer.Parse
{
    /// <summary>
    /// 请求信息
    /// </summary>
    [Serializable]
    internal class RequestInfo
    {
        private string _lineStr = string.Empty;
        public string LineStr
        {
            get { return _lineStr; }
            set { _lineStr = value; }
        }


        private string _headStr = string.Empty;
        public string HeadStr
        {
            get { return _headStr; }
            set { _headStr = value; }
        }

        private string _bodyStr = string.Empty;
        public string BodyStr
        {
            get { return _bodyStr; }
            set { _bodyStr = value; }
        }

        private int _bodyLength = 0;
        public int BodyLength
        {
            get { return _bodyLength; }
            set { _bodyLength = value; }
        }


        private byte[] _bodyBytes;
        public byte[] BodyBytes
        {
            get { return _bodyBytes; }
            set { _bodyBytes = value; }
        }

        private string[] _knownRequestHeaders;
        public string[] KnownRequestHeaders
        {
            get { return _knownRequestHeaders; }
            set { _knownRequestHeaders = value; }
        }

        private string[][] _unknownRequestHeaders;
        public string[][] UnknownRequestHeaders
        {
            get { return _unknownRequestHeaders; }
            set { _unknownRequestHeaders = value; }
        }


        private SocketProxy _acceptSocket = null;

        public SocketProxy AcceptSocket
        {
            get { return _acceptSocket; }
            set { _acceptSocket = value; }
        }

        #region Request Line

        private string _httpVerbName = string.Empty;
        /// <summary>
        /// Http动作的类型，可以是Get,Post,Put,Delete中的一种
        /// </summary>
        public string HttpVerbName
        {
            get { return _httpVerbName; }
            set { _httpVerbName = value; }
        }

        private string _protocol = string.Empty;
        /// <summary>
        /// Http协议的版本，可以是类似于"HTTP/1.1","HTTP/1.0",目前也就这两种
        /// </summary>
        public string Protocol
        {
            get { return _protocol; }
            set { _protocol = value; }
        }

        private string _requestUrl = string.Empty;
        /// <summary>
        /// 请求的Url地址
        /// </summary>
        public string RequestUrl
        {
            get { return _requestUrl; }
            set { _requestUrl = value; }
        }

        private string _uriPath = string.Empty;
        public string UriPath
        {
            get { return _uriPath; }
            set { _uriPath = value; }
        }


        private string _queryString;
        public string QueryString
        {
            get { return _queryString; }
            set { _queryString = value; }
        }

        private byte[] _queryStringBytes;
        public byte[] QueryStringBytes
        {
            get { return _queryStringBytes; }
            set { _queryStringBytes = value; }
        }

        private string _filePath = string.Empty;
        public string FilePath
        {
            get { return _filePath; }
            set { _filePath = value; }
        }

        private string _pathInfo = string.Empty;
        public string PathInfo
        {
            get { return _pathInfo; }
            set { _pathInfo = value; }
        }

        private string _pathTranslated = string.Empty;
        public string PathTranslated
        {
            get { return _pathTranslated; }
            set { _pathTranslated = value; }
        }

        #endregion 

        #region Request Header

        private string _host = string.Empty;
        public string Host
        {
            get { return _host; }
            set { _host = value; }
        }

        private string _hostUrl = string.Empty;
        public string HostUrl
        {
            get { return _hostUrl; }
            set { _hostUrl = value; }
        }

        private int _hostPort = 0;
        public int HostPort
        {
            get { return _hostPort; }
            set { _hostPort = value; }
        }

        private string _connection = string.Empty;
        public string Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        private bool _isKeepAlive = false;
        public bool IsKeepAlive
        {
            get { return _isKeepAlive; }
            set { _isKeepAlive = value; }
        }

        private string _cookie = string.Empty;
        public string Cookie
        {
            get { return _cookie; }
            set { _cookie = value; }
        }

        private string _userAgent = string.Empty;
        public string UserAgent
        {
            get { return _userAgent; }
            set { _userAgent = value; }
        }

        private string _cacheControl = string.Empty;
        public string CacheControl
        {
            get { return _cacheControl; }
            set { _cacheControl = value; }
        }

        private string _accept = string.Empty;
        public string Accept
        {
            get { return _accept; }
            set { _accept = value; }
        }

        private string _referer = string.Empty;
        public string Referer
        {
            get { return _referer; }
            set { _referer = value; }
        }

        private string _acceptEncoding = string.Empty;
        public string AcceptEncoding
        {
            get { return _acceptEncoding; }
            set { _acceptEncoding = value; }
        }

        private string _acceptLanguage = string.Empty;
        public string AcceptLanguage
        {
            get { return _acceptLanguage; }
            set { _acceptLanguage = value; }
        }

        private string _ifModifiedSince = string.Empty;
        public string IfModifiedSince
        {
            get { return _ifModifiedSince; }
            set { _ifModifiedSince = value; }
        }

        private int _contentLength;
        public int ContentLength
        {
            get { return _contentLength; }
            set { _contentLength = value; }
        }

        private string _contentType = string.Empty;
        public string ContentType
        {
            get { return _contentType; }
            set { _contentType = value; }
        }

        private string _visualRootPath = string.Empty;
        public string VisualRootPath
        {
            get { return _visualRootPath; }
            set { _visualRootPath = value; }
        }



        #endregion

        #region Request Body

        #endregion

    }
}
