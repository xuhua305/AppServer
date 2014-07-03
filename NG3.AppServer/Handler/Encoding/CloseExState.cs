using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NG3.AppServer.Handler.Encoding
{
    [Flags]
    internal enum CloseExState
    {
        Normal,
        Abort,
        Silent
    }
}
