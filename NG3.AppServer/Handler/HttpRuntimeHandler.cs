using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace NG3.AppServer.Handler
{
    internal class HttpRuntimeHandler
    {
        public void Process(RequestHandler requestHandler)
        {
            HttpRuntime.ProcessRequest(requestHandler);
        }
    }
}
