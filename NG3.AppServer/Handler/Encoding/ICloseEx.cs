using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NG3.AppServer.Handler.Encoding
{
    internal interface ICloseEx
    {
        void CloseEx(CloseExState closeState);
    }
}
