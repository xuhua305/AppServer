using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using NG3.AppServer.Connector;
using NG3.AppServer.Parse;

namespace NG3.AppServer.Handler
{
    internal class Host : MarshalByRefObject, IRegisteredObject
    {
        private PathInfo _pathInfo = new PathInfo();
        public PathInfo PathInfo
        {
            get { return _pathInfo; }
            set { _pathInfo = value; }
        }

        private IntPtr _processToken = IntPtr.Zero;
        public System.IntPtr ProcessToken
        {
            get { return _processToken; }
        }

        private string _processUser;
        public string ProcessUser
        {
            get { return _processUser; }
        }


        public bool IsVirtualPathAppPath(string path)
        {
            if (path == null)
            {
                return false;
            }
            path = CultureInfo.InvariantCulture.TextInfo.ToLower(path);
            return (path == _pathInfo.LowerCasedVirtualPath || path == _pathInfo.LowerCasedVirtualPathWithTrailingSlash);
        }

        public bool IsVirtualPathInApp(string path, out bool isClientScriptPath)
        {
            isClientScriptPath = false;

            if (path == null)
            {
                return false;
            }

            if (_pathInfo.VisualPath == "/" && path.StartsWith("/", StringComparison.Ordinal))
            {
                if (path.StartsWith(_pathInfo.LowerCasedClientScriptPathWithTrailingSlash, StringComparison.Ordinal))
                {
                    isClientScriptPath = true;
                }
                return true;
            }

            path = CultureInfo.InvariantCulture.TextInfo.ToLower(path);

            if (path.StartsWith(_pathInfo.LowerCasedVirtualPathWithTrailingSlash, StringComparison.Ordinal))
            {
                return true;
            }

            if (path == _pathInfo.LowerCasedVirtualPath)
            {
                return true;
            }

            if (path.StartsWith(_pathInfo.LowerCasedClientScriptPathWithTrailingSlash, StringComparison.Ordinal))
            {
                isClientScriptPath = true;
                return true;
            }

            return false;
        }

        public bool IsVirtualPathInApp(String path)
        {
            bool isClientScriptPath;
            return IsVirtualPathInApp(path, out isClientScriptPath);
        }

        public Host()
        {
            
        }

        public void Config(PathInfo pathInfo, IntPtr processToken, string processUser)
        {
            try
            {
                HostingEnvironment.RegisterObject(this);
                _processToken = processToken;
                _processUser = processUser;
                _pathInfo = pathInfo;

                _pathInfo.LowerCasedVirtualPath = CultureInfo.InvariantCulture.TextInfo.ToLower(_pathInfo.VisualPath);
                _pathInfo.LowerCasedVirtualPathWithTrailingSlash = _pathInfo.VisualPath.EndsWith("/", StringComparison.Ordinal)
                                                              ? _pathInfo.VisualPath
                                                              : _pathInfo.VisualPath + "/";
                _pathInfo.LowerCasedVirtualPathWithTrailingSlash =
                    CultureInfo.InvariantCulture.TextInfo.ToLower(_pathInfo.LowerCasedVirtualPathWithTrailingSlash);
                _pathInfo.PhysicalClientScriptPath = HttpRuntime.AspClientScriptPhysicalPath + "\\";
                _pathInfo.LowerCasedClientScriptPathWithTrailingSlash =
                    CultureInfo.InvariantCulture.TextInfo.ToLower(HttpRuntime.AspClientScriptVirtualPath + "/");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Host(PathInfo pathInfo, IntPtr processToken, string processUser)
        {
            try
            {
                HostingEnvironment.RegisterObject(this);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void ProcessRequest(RequestInfo requestInfo,IOCPChannelConnector icoChannelConnector)
        {
            RequestHandler requestHandler = new RequestHandler(requestInfo, this, icoChannelConnector);
            requestHandler.PrepareResponse();
            HttpRuntimeHandler httpRuntimeHandler = new HttpRuntimeHandler();
            httpRuntimeHandler.Process(requestHandler);
        }


        void IRegisteredObject.Stop(bool immediate)
        {
            try
            {
                HostingEnvironment.UnregisterObject(this);
                Thread.Sleep(100);
                HttpRuntime.Close();
                Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                
                throw;
            }
        }

        public override object InitializeLifetimeService()
        {
            // never expire the license
            return null;
        }


        [SecurityPermission(SecurityAction.Assert, Unrestricted = true)]
        public void Shutdown()
        {
            try
            {
                HostingEnvironment.InitiateShutdown();
            }
            catch (Exception ex)
            {
                throw;
            }


        }

    }
}
