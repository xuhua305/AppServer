using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;
using NG3.AppServer.Connector;
using NG3.AppServer.Handler;
using NG3.AppServer.Parse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace NG3.AppServer
{
    [PermissionSet(SecurityAction.LinkDemand, Name = "Everything"),PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public class Server :MarshalByRefObject, IDisposable
    {
        private IOCPChannelConnector _iocpChannelConnector = null;

        private IntPtr _processToken;

        private string _processUser;

        private IList<PathInfo> _pathInfos = null;

        private Host _host;

        private void ObtainProcessToken()
        {
            if (Interop.ImpersonateSelf(2))
            {
                Interop.OpenThreadToken(Interop.GetCurrentThread(), 0xf01ff, true, ref _processToken);
                Interop.RevertToSelf();
                _processUser = WindowsIdentity.GetCurrent().Name;
            }
        }

        public Server()
        {
            ObtainProcessToken();
        }

        public void Start(IList<PathInfo> pathInfos)
        {
            _pathInfos = pathInfos;
            _iocpChannelConnector = new IOCPChannelConnector(10, IPAddress.Parse("127.0.0.1"), 32778);
            _iocpChannelConnector.ReceiveCompleted += OnReceiveCompleted;
            _iocpChannelConnector.Start();
        }

        void OnReceiveCompleted(object sender, ReceiveCompleteEventArgs e)
        {
            ProcessRequest(e);
        }

        public void HostStopped()
        {
            _host = null;
        }

        private void ProcessRequest(ReceiveCompleteEventArgs receiveCompleteEventArgs)
        {
            try
            {
                Token token = receiveCompleteEventArgs.Token;
                if (token == null)
                {

                    return;
                }

                RequestInfo requestInfo = RequestParse.Parse(token);
                receiveCompleteEventArgs.IsKeepAlive = requestInfo.IsKeepAlive;

                HostManager hostManager = new HostManager(_pathInfos);
                int index = -1;
                _host = hostManager.GetHost(requestInfo.RequestUrl,ref index);
                if (_host == null)
                {

                    return;
                }
                _host.Config(_pathInfos[index],_processToken,_processUser);
                _host.ProcessRequest(requestInfo,_iocpChannelConnector);
            }
            catch (Exception ex)
            {
                
                throw;
            }
        }

        public void Dispose()
        {
            
        }
    }
}
