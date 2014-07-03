using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NG3.AppServer.Connector
{
    /// <summary>
    /// Socket的跨应用程序域的代理
    /// </summary>
    public class SocketProxy : MarshalByRefObject
    {
        private Socket _socket = null;
        public SocketProxy(Socket socket)
        {
            Debug.Assert(socket != null);
            _socket = socket;
        }

        public int Available
        {
            get { return _socket.Available; }
        }

        public void Close()
        {
            _socket.Close();
        }

        public bool Connected
        {
            get { return _socket.Connected; }
        }

        public void Shutdown(SocketShutdown socketShutdown)
        {
            _socket.Shutdown(socketShutdown);
        }

        public int Receive(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            return _socket.Receive(buffer, offset, size, socketFlags);
        }

        public int Send(byte[] buffer)
        {
            return _socket.Send(buffer);
        }

        public int Send(byte[] buffer, int offset, int size, SocketFlags socketFlags)
        {
            return _socket.Send(buffer, offset, size, socketFlags);
        }

        public EndPoint RemoteEndPoint
        {
            get { return _socket.RemoteEndPoint; }
        }

        public bool Poll(int microSeconds, SelectMode mode)
        {
            return _socket.Poll(microSeconds, mode);
        }

        public bool AcceptAsync(SocketAsyncEventArgs e)
        {
            return _socket.AcceptAsync(e);
        }
    }
}
