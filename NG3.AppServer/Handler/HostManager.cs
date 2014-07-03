using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace NG3.AppServer.Handler
{
    internal class HostManager
    {
        private readonly object _lockObject = new object();
        private readonly object _lockDouboleObject = new object();
        private IDictionary<string, Host> _hostDic = new Dictionary<string, Host>();

        private readonly IList<PathInfo> _pathInfos = null;
        private readonly ApplicationManager _appManager;

        public HostManager(IList<PathInfo> pathInfos )
        {
            _pathInfos = pathInfos;
            _appManager = ApplicationManager.GetApplicationManager();
        }

        private string GetRequestVisualPath(string visualPath, ref int index)
        {
            for (int i = 0; i < _pathInfos.Count; i++)
            {
                string tempStr = _pathInfos[i].VisualPath + "/";
                if (visualPath.StartsWith(tempStr, StringComparison.OrdinalIgnoreCase))
                {
                    index = i;
                    return _pathInfos[i].VisualPath;
                }
                if (visualPath.Equals(_pathInfos[i].VisualPath, StringComparison.OrdinalIgnoreCase))
                {
                    index = i;
                    return _pathInfos[i].VisualPath;
                }

            }
            index = -1;
            return string.Empty;
        }

        private object CreateWorkerAppDomainWithHost(string virtualPath, string physicalPath, Type hostType)
        {
            try
            {
                // this creates worker app domain in a way that host doesn't need to be in GAC or bin
                // using BuildManagerHost via private reflection
                string uniqueAppString = string.Concat(virtualPath, physicalPath).ToLowerInvariant();
                string appId = (uniqueAppString.GetHashCode()).ToString("x", CultureInfo.InvariantCulture);

                // create BuildManagerHost in the worker app domain
                //ApplicationManager appManager = ApplicationManager.GetApplicationManager();
                Type buildManagerHostType = typeof(HttpRuntime).Assembly.GetType("System.Web.Compilation.BuildManagerHost");
                IRegisteredObject buildManagerHost = _appManager.CreateObject(appId, buildManagerHostType, virtualPath,
                                                                              physicalPath, false);

                // call BuildManagerHost.RegisterAssembly to make Host type loadable in the worker app domain
                buildManagerHostType.InvokeMember("RegisterAssembly",
                                                  BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic,
                                                  null,
                                                  buildManagerHost,
                                                  new object[] { hostType.Assembly.FullName, hostType.Assembly.Location });

                // create Host in the worker app domain
                // FIXME: getting FileLoadException Could not load file or assembly 'WebDev.WebServer20, Version=4.0.1.6, Culture=neutral, PublicKeyToken=f7f6e0b4240c7c27' or one of its dependencies. Failed to grant permission to execute. (Exception from HRESULT: 0x80131418)
                // when running dnoa 3.4 samples - webdev is registering trust somewhere that we are not
                return _appManager.CreateObject(appId, hostType, virtualPath, physicalPath, false);
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public Host GetHost(string vPath,ref int index)
        {
            try
            {
                string visualPath = GetRequestVisualPath(vPath, ref index);
                Host host = null;
                if (string.IsNullOrEmpty(visualPath))
                    return null;

                lock (_lockDouboleObject)
                {
                    if (!_hostDic.ContainsKey(visualPath))
                    {
                        lock (_lockObject)
                        {
                            host = (Host)CreateWorkerAppDomainWithHost(visualPath, _pathInfos[index].PhysicalPath, typeof(Host));

                            //host.Configure(this, _port, visualPath, _physicalPath[index], _requireAuthentication, _disableDirectoryListing);
                            _hostDic.Add(visualPath, host);
                        }
                    }
                    else
                    {
                        host = _hostDic[visualPath];
                    }
                }

                return host;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
