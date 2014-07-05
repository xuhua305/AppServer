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

        private bool _isKeepAlive = false;

        public bool IsKeepAlive
        {
            get { return _isKeepAlive; }
            set { _isKeepAlive = value; }
        }

        public ReceiveCompleteEventArgs(Token token,bool isKeepAlive)
        {
            _token = token;
            _isKeepAlive = isKeepAlive;
        }
    }
}
