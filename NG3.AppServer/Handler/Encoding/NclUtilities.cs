using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;

namespace NG3.AppServer.Handler.Encoding
{
    internal static class NclUtilities
    {
        internal static bool IsFatal(Exception exception)
        {
            if (exception == null)
            {
                return false;
            }
            if (exception is OutOfMemoryException || exception is StackOverflowException)
            {
                return true;
            }
            return exception is ThreadAbortException;
        }
    }
}
