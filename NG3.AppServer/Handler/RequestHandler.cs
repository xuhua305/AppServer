using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web.Hosting;
using Microsoft.Win32.SafeHandles;
using NG3.AppServer.Connector;
using NG3.AppServer.Handler.Encoding;
using NG3.AppServer.Parse;

namespace NG3.AppServer.Handler
{
    internal class RequestHandler:SimpleWorkerRequest
    {
        private const int MaxChunkLength = 64 * 1024;

        private RequestInfo _reRequestInfo = null;

        private int _responseStatus;

        private Host _host = null;

        private List<byte[]> _responseBodyBytes;

        private bool _headersSent;

        private StringBuilder _responseHeadersBuilder;

        private IOCPChannelConnector _iocpChannelConnector;

        private bool _specialCaseStaticFileHeaders = false;

        public void PrepareResponse()
        {
            _headersSent = false;
            _responseStatus = 200;
            _responseHeadersBuilder = new StringBuilder();
            _responseBodyBytes = new List<byte[]>();
        }

        public RequestHandler(RequestInfo requestInfo,Host host,IOCPChannelConnector icoChannelConnector): base(String.Empty, String.Empty, null)
        {
            _reRequestInfo = requestInfo;
            _host = host;
            requestInfo.PathTranslated = MapPath(requestInfo.FilePath);
            _iocpChannelConnector = icoChannelConnector;
        }

        #region 重写方法

        /// <summary>
        /// 终止与客户端的连接
        /// </summary>
        public override void CloseConnection()
        {
            try
            {
                _reRequestInfo.AcceptSocket.Shutdown(SocketShutdown.Both);
                _reRequestInfo.AcceptSocket.Close();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _reRequestInfo.AcceptSocket = null;

            }
        }

        /// <summary>
        /// 通知 HttpWorkerRequest 当前请求的请求处理已完成
        /// </summary>
        public override void EndOfRequest()
        {
            base.EndOfRequest();
        }

        private bool ProcessDirectoryListingRequest()
        {
            if (_reRequestInfo.HttpVerbName != "GET")
            {
                return false;
            }

            string dirPathTranslated = _reRequestInfo.PathTranslated;

            if (_reRequestInfo.PathInfo.Length > 0)
            {
                // directory path can never have pathInfo
                dirPathTranslated = MapPath(_reRequestInfo.UriPath);
            }

            if (!Directory.Exists(dirPathTranslated))
            {
                return false;
            }

            // get all files and subdirs
            FileSystemInfo[] infos = null;
            try
            {
                infos = (new DirectoryInfo(dirPathTranslated)).GetFileSystemInfos();
            }
            catch
            {
            }

            // determine if parent is appropriate
            string parentPath = null;

            if (_reRequestInfo.UriPath.Length > 1)
            {
                int i = _reRequestInfo.UriPath.LastIndexOf('/', _reRequestInfo.UriPath.Length - 2);

                parentPath = (i > 0) ? _reRequestInfo.UriPath.Substring(0, i) : "/";
                if (!_host.IsVirtualPathInApp(parentPath))
                {
                    parentPath = null;
                }
            }

            _iocpChannelConnector.WriteEntireResponseFromString(_reRequestInfo.AcceptSocket, 200, "Content-type: text/html; charset=utf-8\r\n",
                                                      Messages.FormatDirectoryListing(_reRequestInfo.UriPath, parentPath, infos),
                                                      false);
            return true;
        }

        /// <summary>
        /// 将所有挂起的响应数据发送到客户端。
        /// </summary>
        /// <param name="finalFlush"></param>
        public override void FlushResponse(bool finalFlush)
        {
            if (_responseStatus == 404 && !_headersSent && finalFlush && _reRequestInfo.HttpVerbName == "GET")
            {
                // attempt directory listing
                if (ProcessDirectoryListingRequest())
                {
                    return;
                }
            }

            if (!_headersSent)
            {
                _iocpChannelConnector.WriteHeaders(_reRequestInfo.AcceptSocket,_responseStatus, _responseHeadersBuilder.ToString());

                _headersSent = true;
            }

            byte[] bodyBytes;
            long bodyLen = 0;
            foreach (byte[] bytes in _responseBodyBytes)
            {
                bodyLen += bytes.Length;
            }
            bodyBytes = new byte[bodyLen];
            long bodyIndex = 0;
            foreach (byte[] bytes in _responseBodyBytes)
            {
                Array.Copy(bytes,0,bodyBytes,bodyIndex,bytes.Length);
                bodyIndex += bytes.Length;
            }
            byte[] gzipBytes = ResponseParse.EncodingBodyContent(bodyBytes);
            _iocpChannelConnector.WriteBody(_reRequestInfo.AcceptSocket, gzipBytes, 0, gzipBytes.Length);

            _responseBodyBytes = new List<byte[]>();

            if (finalFlush)
            {
                _iocpChannelConnector.Close(_reRequestInfo.AcceptSocket);
            }
        }

        /// <summary>
        /// 返回当前正在执行的服务器应用程序的虚拟路径。
        /// </summary>
        /// <returns></returns>
        public override string GetAppPath()
        {
            return _host.PathInfo.VisualPath;
        }

        /// <summary>
        /// 返回当前正在执行的服务器应用程序的 UNC 翻译路径。
        /// </summary>
        /// <returns></returns>
        public override string GetAppPathTranslated()
        {
            return _host.PathInfo.PhysicalPath;
        }

        public override string GetFilePath()
        {
            return _reRequestInfo.FilePath;
        }

        public override string GetFilePathTranslated()
        {
            return _reRequestInfo.PathTranslated;
        }

        public override string GetHttpVerbName()
        {
            return _reRequestInfo.HttpVerbName;
        }

        public override string GetHttpVersion()
        {
            return _reRequestInfo.Protocol;
        }

        private string GetStackString(StackFrame[] stacks)
        {
            StringBuilder sb = new StringBuilder();
            foreach (StackFrame stack in stacks)
            {
                sb.Append(string.Format("{0} {1} {2} {3}\r\n", stack.GetFileName(), stack.GetFileLineNumber(), stack.GetFileColumnNumber(),
                    stack.GetMethod().ToString()));
            }
            return sb.ToString();
        }

        public override string GetKnownRequestHeader(int index)
        {
            StackFrame[] stacks = new StackTrace().GetFrames();
            string result = GetStackString(stacks);
            return _reRequestInfo.KnownRequestHeaders[index];
        }

        public override string GetLocalAddress()
        {
            return _reRequestInfo.HostUrl;
        }

        public override int GetLocalPort()
        {
            return _reRequestInfo.HostPort;
        }

        public override string GetPathInfo()
        {
            return _reRequestInfo.PathInfo;
        }

        public override byte[] GetPreloadedEntityBody()
        {
            return _reRequestInfo.BodyBytes;
        }

        public override string GetQueryString()
        {
            return _reRequestInfo.QueryString;
        }

        public override byte[] GetQueryStringRawBytes()
        {
            return _reRequestInfo.QueryStringBytes;
        }

        public override string GetRawUrl()
        {
            return _reRequestInfo.RequestUrl;
        }

        public override string GetRemoteAddress()
        {
            if (_reRequestInfo.AcceptSocket != null)
            {
                IPEndPoint ep = (IPEndPoint)_reRequestInfo.AcceptSocket.RemoteEndPoint;
                return (ep != null && ep.Address != null) ? ep.Address.ToString() : "127.0.0.1";
            }
            return "127.0.0.1"; 
        }

        public override int GetRemotePort()
        {
            return 0;
        }

        public override string GetServerName()
        {
            string localAddress = GetLocalAddress();
            if (localAddress.Equals("127.0.0.1"))
            {
                return "localhost";
            }
            return localAddress;
        }

        public override string GetServerVariable(string name)
        {
            string processUser = string.Empty;
            string str2 = name;
            if (str2 == null)
            {
                return processUser;
            }
            if (str2 != "ALL_RAW")
            {
                if (str2 != "SERVER_PROTOCOL")
                {
                    if (str2 == "LOGON_USER")
                    {
                        if (GetUserToken() != IntPtr.Zero)
                        {
                            processUser = _host.ProcessUser;
                        }
                        return processUser;
                    }
                    if ((str2 == "AUTH_TYPE") && (GetUserToken() != IntPtr.Zero))
                    {
                        processUser = "NTLM";
                    }
                    return processUser;
                }
            }
            else
            {
                return _reRequestInfo.HeadStr;
            }
            return _reRequestInfo.Protocol;
        }

        public override string GetUnknownRequestHeader(string name)
        {
            int n = _reRequestInfo.UnknownRequestHeaders.Length;

            for (int i = 0; i < n; i++)
            {
                if (string.Compare(name, _reRequestInfo.UnknownRequestHeaders[i][0], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return _reRequestInfo.UnknownRequestHeaders[i][1];
                }
            }

            return null;
        }

        public override string[][] GetUnknownRequestHeaders()
        {
            return _reRequestInfo.UnknownRequestHeaders;
        }

        public override string GetUriPath()
        {
            return _reRequestInfo.UriPath;
        }

        public override IntPtr GetUserToken()
        {
            return _host.ProcessToken;
        }

        public override bool HeadersSent()
        {
            return _headersSent;
        }

        public override bool IsClientConnected()
        {
            return _reRequestInfo.AcceptSocket.Connected;
        }

        public override bool IsEntireEntityBodyIsPreloaded()
        {
            return (_reRequestInfo.ContentLength == _reRequestInfo.BodyLength);
        }

        public override string MapPath(string path)
        {
            string mappedPath;
            bool isClientScriptPath;

            if (string.IsNullOrEmpty(path) || path.Equals("/"))
            {
                // asking for the site root
                mappedPath = _host.PathInfo.VisualPath == "/" ? _host.PathInfo.PhysicalPath : Environment.SystemDirectory;
            }
            else if (_host.IsVirtualPathAppPath(path))
            {
                // application path
                mappedPath = _host.PathInfo.PhysicalPath;
            }
            else if (_host.IsVirtualPathInApp(path, out isClientScriptPath))
            {
                if (isClientScriptPath)
                {
                    mappedPath = _host.PathInfo.PhysicalClientScriptPath +
                                 path.Substring(_host.PathInfo.LowerCasedClientScriptPathWithTrailingSlash.Length);
                }
                else
                {
                    // inside app but not the app path itself
                    mappedPath = _host.PathInfo.PhysicalPath + path.Substring(_host.PathInfo.LowerCasedVirtualPathWithTrailingSlash.Length);
                }
            }
            else
            {
                // outside of app -- make relative to app path
                if (path.StartsWith("/", StringComparison.Ordinal))
                {
                    mappedPath = _host.PathInfo.PhysicalPath + path.Substring(1);
                }
                else
                {
                    mappedPath = _host.PathInfo.PhysicalPath + path;
                }
            }

            mappedPath = mappedPath.Replace('/', '\\');

            if (mappedPath.EndsWith("\\", StringComparison.Ordinal) &&
                !mappedPath.EndsWith(":\\", StringComparison.Ordinal))
            {
                mappedPath = mappedPath.Substring(0, mappedPath.Length - 1);
            }

            return mappedPath;
        }

        public override int ReadEntityBody(byte[] buffer, int size)
        {
            int bytesRead = 0;

            byte[] bytes = _iocpChannelConnector.ReadRequestBytes(_reRequestInfo.AcceptSocket, size);

            if (bytes != null && bytes.Length > 0)
            {
                bytesRead = bytes.Length;
                Buffer.BlockCopy(bytes, 0, buffer, 0, bytesRead);
            }

            return bytesRead;
        }

        public override void SendCalculatedContentLength(int contentLength)
        {
            if (!_headersSent)
            {
                _responseHeadersBuilder.Append("Content-Length: ");
                _responseHeadersBuilder.Append(contentLength.ToString(CultureInfo.InvariantCulture));
                _responseHeadersBuilder.Append("\r\n");
            }
        }

        public override void SendKnownResponseHeader(int index, string value)
        {
            if (_headersSent)
            {
                return;
            }

            switch (index)
            {
                case HeaderServer:
                case HeaderDate:
                case HeaderConnection:
                    // ignore these
                    return;
                case HeaderAcceptRanges:
                    // FIX: #14359
                    if (value != "bytes")
                    {
                        // use this header to detect when we're processing a static file
                        break;
                    }
                    _specialCaseStaticFileHeaders = true;
                    return;

                case HeaderExpires:
                case HeaderLastModified:
                    // FIX: #14359
                    if (!_specialCaseStaticFileHeaders)
                    {
                        // NOTE: Ignore these for static files. These are generated
                        //       by the StaticFileHandler, but they shouldn't be.
                        break;
                    }
                    return;


                // FIX: #12506
                case HeaderContentType:

                    string contentType = null;

                    if (value == "application/octet-stream")
                    {
                        // application/octet-stream is default for unknown so lets
                        // take a shot at determining the type.
                        // don't do this for other content-types as you are going to
                        // end up sending text/plain for endpoints that are handled by
                        // asp.net such as .aspx, .asmx, .axd, etc etc
                        contentType = CommonExtensions.GetContentType(_reRequestInfo.PathTranslated);
                    }
                    value = contentType ?? value;
                    break;
            }

            _responseHeadersBuilder.Append(GetKnownResponseHeaderName(index));
            _responseHeadersBuilder.Append(": ");
            _responseHeadersBuilder.Append(value);
            _responseHeadersBuilder.Append("\r\n");
        }

        private void SendResponseFromFileStream(Stream f, long offset, long length)
        {
            long fileSize = f.Length;

            if (length == -1)
            {
                length = fileSize - offset;
            }

            if (length == 0 || offset < 0 || length > fileSize - offset)
            {
                return;
            }

            if (offset > 0)
            {
                f.Seek(offset, SeekOrigin.Begin);
            }

            if (length <= MaxChunkLength)
            {
                var fileBytes = new byte[(int)length];
                int bytesRead = f.Read(fileBytes, 0, (int)length);
                SendResponseFromMemory(fileBytes, bytesRead);
            }
            else
            {
                var chunk = new byte[MaxChunkLength];
                var bytesRemaining = (int)length;

                while (bytesRemaining > 0)
                {
                    int bytesToRead = (bytesRemaining < MaxChunkLength) ? bytesRemaining : MaxChunkLength;
                    int bytesRead = f.Read(chunk, 0, bytesToRead);

                    SendResponseFromMemory(chunk, bytesRead);
                    bytesRemaining -= bytesRead;

                    // flush to release keep memory
                    if ((bytesRemaining > 0) && (bytesRead > 0))
                    {
                        FlushResponse(false);
                    }
                }
            }
        }

        public override void SendResponseFromFile(IntPtr handle, long offset, long length)
        {
            if (length == 0)
            {
                return;
            }

            using (var sfh = new SafeFileHandle(handle, false))
            {
                using (var f = new FileStream(sfh, FileAccess.Read))
                {
                    SendResponseFromFileStream(f, offset, length);
                }
            }
        }

        public override void SendResponseFromFile(string filename, long offset, long length)
        {
            if (length == 0)
            {
                return;
            }

            FileStream f = null;
            try
            {
                f = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                SendResponseFromFileStream(f, offset, length);
            }
            finally
            {
                if (f != null)
                {
                    f.Close();
                }
            }
        }

        public override void SendResponseFromMemory(byte[] data, int length)
        {
            if (length > 0)
            {
                var bytes = new byte[length];

                Buffer.BlockCopy(data, 0, bytes, 0, length);

                _responseBodyBytes.Add(bytes);
            }
        }

        public override void SendStatus(int statusCode, string statusDescription)
        {
            _responseStatus = statusCode;
        }

        public override void SendUnknownResponseHeader(string name, string value)
        {
            if (_headersSent)
                return;

            _responseHeadersBuilder.Append(name);
            _responseHeadersBuilder.Append(": ");
            _responseHeadersBuilder.Append(value);
            _responseHeadersBuilder.Append("\r\n");
        }

        #endregion 
    }
}
