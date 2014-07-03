using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NG3.AppServer.Connector
{
    internal class ReceiveCompleteEventArgs : EventArgs
    {
        private Token _token = null;
        public Token Token
        {
            get { return _token; }
            set { _token = value; }
        }

        public ReceiveCompleteEventArgs(Token token)
        {
            _token = token;
        }
    }
}
