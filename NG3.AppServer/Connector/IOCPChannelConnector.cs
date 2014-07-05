using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NG3.AppServer.Parse;

namespace NG3.AppServer.Connector
{
    internal class IOCPChannelConnector : AbstractConnector
    {

        private static Mutex _mutex = new Mutex();

        #region 事件

        public event EventHandler<ReceiveCompleteEventArgs> ReceiveCompleted;

        #endregion 

        /// <summary>
        /// 监听的Socket
        /// </summary>
        private SocketProxy _listenSocket = null;

        private IPAddress _address;
        public IPAddress Address
        {
            get { return _address; }
            set { _address = value; }
        }

        private int _port = 0;
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }



        private SocketAsyncEventArgsPool _readWritePool;

        private Semaphore _semaphoreAcceptedClients;

        public IOCPChannelConnector(int listenMaxConnections, IPAddress address,int port)
        {
            ListenMaxConnections = listenMaxConnections;
            _port = port;
            _address = address;

            _semaphoreAcceptedClients = new Semaphore(ListenMaxConnections, ListenMaxConnections);
            _readWritePool = new SocketAsyncEventArgsPool(ListenMaxConnections);

            // Preallocate pool of SocketAsyncEventArgs objects.
            for (int i = 0; i < ListenMaxConnections; i++)
            {
                SocketAsyncEventArgs readWriteEventArg = new SocketAsyncEventArgs();
                readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnIOCompleted);
                readWriteEventArg.SetBuffer(new Byte[MaxHeaderBytes], 0, MaxHeaderBytes);

                // Add SocketAsyncEventArg to the pool.
                _readWritePool.Push(readWriteEventArg);
            }

            
        }

        /// <summary>
        /// Callback called whenever a receive or send operation is completed on a socket.
        /// </summary>
        /// <param name="sender">Object who raised the event.</param>
        /// <param name="e">SocketAsyncEventArg associated with the completed send/receive operation.</param>
        private void OnIOCompleted(object sender, SocketAsyncEventArgs e)
        {
            // Determine which type of operation just completed and call the associated handler.
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    this.ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    this.ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        private void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += OnAcceptCompleted;
            }
            else
            {
                // Socket must be cleared since the context object is being reused.
                acceptEventArg.AcceptSocket = null;
            }

            _semaphoreAcceptedClients.WaitOne();
            if (!_listenSocket.AcceptAsync(acceptEventArg))
            {
                this.ProcessAccept(acceptEventArg);
            }
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            Socket s = e.AcceptSocket;
            if (s.Connected)
            {
                try
                {
                    SocketAsyncEventArgs readEventArgs = _readWritePool.Pop();
                    if (readEventArgs != null)
                    {
                        // Get the socket for the accepted client connection and put it into the 
                        // ReadEventArg object user token.
                        readEventArgs.UserToken = new Token(s, MaxHeaderBytes);
                        //Interlocked.Increment(ref this.numConnectedSockets);
                        //Console.WriteLine("Client connection accepted. There are {0} clients connected to the server",
                        //    this.numConnectedSockets);
                        if (!s.ReceiveAsync(readEventArgs))
                        {
                            this.ProcessReceive(readEventArgs);
                        }
                    }
                    else
                    {
                        Console.WriteLine("There are no more available sockets to allocate.");
                    }
                }
                catch (SocketException ex)
                {
                    Token token = e.UserToken as Token;
                    Console.WriteLine("Error when processing data received from {0}:\r\n{1}", token.Connection.RemoteEndPoint, ex.ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                // Accept the next connection request.
                this.StartAccept(e);
            }
        }

        void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            this.ProcessAccept(e);
        }

        /// <summary>
        /// Close the socket associated with the client.
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the completed send/receive operation.</param>
        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            Token token = e.UserToken as Token;
            this.CloseClientSocket(token, e);
        }

        private void CloseClientSocket(Token token, SocketAsyncEventArgs e)
        {
            token.Dispose();

            // Decrement the counter keeping track of the total number of clients connected to the server.
            _semaphoreAcceptedClients.Release();
            //Interlocked.Decrement(ref this.numConnectedSockets);
            //Console.WriteLine("A client has been disconnected from the server. There are {0} clients connected to the server", this.numConnectedSockets);

            // Free the SocketAsyncEventArg so they can be reused by another client.
            _readWritePool.Push(e);
        }

        /// <summary>
        /// This method is invoked when an asynchronous receive operation completes. 
        /// If the remote host closed the connection, then the socket is closed.  
        /// If data was received then the data is echoed back to the client.
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the completed receive operation.</param>
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            // Check if the remote host closed the connection.
            if (e.BytesTransferred > 0)
            {
                if (e.SocketError == SocketError.Success)
                {
                    Token token = e.UserToken as Token;
                    token.SetData(e);

                    Socket s = token.Connection;
                    if (s.Available == 0)
                    {
                        if(ReceiveCompleted != null)
                        {
                            ReceiveCompleted(null, new ReceiveCompleteEventArgs(token));
                        }
                        this.CloseClientSocket(e);
                    }
                    else if (!s.ReceiveAsync(e))
                    {
                        // Read the next block of data sent by client.
                        this.ProcessReceive(e);
                    }
                }
                else
                {
                    this.ProcessError(e);
                }
            }
            else
            {
                this.CloseClientSocket(e);
            }
        }

        /// <summary>
        /// This method is invoked when an asynchronous send operation completes.  
        /// The method issues another receive on the socket to read any additional 
        /// data sent from the client.
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the completed send operation.</param>
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                // Done echoing data back to the client.
                Token token = e.UserToken as Token;

                if (!token.Connection.ReceiveAsync(e))
                {
                    // Read the next block of data send from the client.
                    this.ProcessReceive(e);
                }
            }
            else
            {
                this.ProcessError(e);
            }
        }

        private void ProcessError(SocketAsyncEventArgs e)
        {
            Token token = e.UserToken as Token;
            IPEndPoint localEp = token.Connection.LocalEndPoint as IPEndPoint;

            this.CloseClientSocket(token, e);

            Console.WriteLine("Socket error {0} on endpoint {1} during {2}.", (Int32)e.SocketError, localEp, e.LastOperation);
        }


        public override void Start()
        {
            _listenSocket = CreateSocketBindAndListen(_address, _port);
            StartAccept(null);
            _mutex.WaitOne();
        }
    }
}
