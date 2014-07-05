using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NG3.AppServer.Parse;

namespace NG3.AppServer.Connector
{
    /// <summary>
    /// 网络连接抽象类
    /// </summary>
    abstract class AbstractConnector : MarshalByRefObject
    {
        protected const int MaxHeaderBytes = 32 * 1024;
        private const int MaxSendBytes = 548;

        private int _listenMaxConnections = 0;

        /// <summary>
        /// 监听的最大连接数
        /// </summary>
        public int ListenMaxConnections
        {
            get { return _listenMaxConnections; }
            set { _listenMaxConnections = value; }
        }

        /// <summary>
        /// 等待是否有信息可以读取
        /// </summary>
        /// <param name="acceptSocket"></param>
        /// <returns></returns>
        private int WaitForRequestBytes(SocketProxy acceptSocket)
        {
            int availBytes = 0;
            try
            {
                if (acceptSocket.Available == 0)
                {
                    acceptSocket.Poll(100000, SelectMode.SelectRead);

                    if (acceptSocket.Available == 0 && acceptSocket.Connected)
                    {
                        acceptSocket.Poll(30000000, SelectMode.SelectRead);
                    }
                }

                availBytes = acceptSocket.Available;

            }
            catch (Exception ex)
            {

            }

            return availBytes;
        }

        /// <summary>
        /// 创建监听的Socket
        /// </summary>
        /// <param name="family"></param>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        protected SocketProxy CreateSocketBindAndListen(IPAddress address, int port)
        {
            try
            {
                // Get host related information.
                IPAddress[] addressList = Dns.GetHostEntry(Environment.MachineName).AddressList;

                // Get endpoint for the listener.
                IPEndPoint localEndPoint = new IPEndPoint(addressList[addressList.Length - 1], port);

                Socket listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listenSocket.ReceiveBufferSize = MaxHeaderBytes;
                listenSocket.SendBufferSize = MaxHeaderBytes;

                if (localEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    // Set dual-mode (IPv4 & IPv6) for the socket listener.
                    // 27 is equivalent to IPV6_V6ONLY socket option in the winsock snippet below,
                    listenSocket.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, false);
                    listenSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, localEndPoint.Port));
                }
                else
                {
                    // Associate the socket with the local endpoint.
                    listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    listenSocket.Bind(localEndPoint);
                }


                listenSocket.Listen(_listenMaxConnections);
                return new SocketProxy(listenSocket);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public abstract void Start();

        public virtual void Close(SocketProxy acceptSocket)
        {
            try
            {
                acceptSocket.Shutdown(SocketShutdown.Both);
                acceptSocket.Close();
            }
            catch (Exception ex)
            {

                throw;
            }
            finally
            {
                acceptSocket = null;
            }
        }

        public virtual void WriteHeaders(SocketProxy acceptSocket,string headers)
        {
            try
            {
                acceptSocket.Send(Encoding.UTF8.GetBytes(headers));
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public virtual void WriteBody(SocketProxy acceptSocket, byte[] data, int offset, int length)
        {
            try
            {
                int sendNum = length/MaxSendBytes;
                if(sendNum == 0)
                    acceptSocket.Send(data, offset, length, SocketFlags.None);
                else
                {
                    int modNum = length%MaxSendBytes;
                    int sendIndex = 0;
                    for (int i = 0; i < sendNum; i++)
                    {
                        acceptSocket.Send(data, sendIndex, MaxSendBytes, SocketFlags.None);
                        sendIndex += MaxSendBytes;
                    }
                    acceptSocket.Send(data, sendIndex, modNum, SocketFlags.None);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public virtual void WriteEntireResponseFromString(SocketProxy acceptSocket, string content)
        {
            try
            {
                if (acceptSocket != null)
                    acceptSocket.Send(Encoding.UTF8.GetBytes(content));
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (acceptSocket != null)
                {
                    if (!keepAlive)
                    {
                        Close(acceptSocket);
                    }
                }

            }
        }


        public byte[] ReadRequestBytes(SocketProxy acceptSocket, int maxBytes)
        {
            try
            {
                if (WaitForRequestBytes(acceptSocket) == 0)
                {
                    return null;
                }

                int numBytes = acceptSocket.Available;

                if (numBytes > maxBytes)
                {
                    numBytes = maxBytes;
                }

                int numReceived = 0;

                byte[] buffer = new byte[numBytes];

                if (numBytes > 0)
                {
                    numReceived = acceptSocket.Receive(buffer, 0, numBytes, SocketFlags.None);
                }

                if (numReceived < numBytes)
                {
                    byte[] tempBuffer = new byte[numReceived];

                    if (numReceived > 0)
                    {
                        Buffer.BlockCopy(buffer, 0, tempBuffer, 0, numReceived);
                    }

                    buffer = tempBuffer;
                }
                return buffer;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


    }
}
