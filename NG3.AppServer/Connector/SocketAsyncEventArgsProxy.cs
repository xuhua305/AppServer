using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NG3.AppServer.Connector
{
    //internal class SocketAsyncEventArgsProxy:MarshalByRefObject
    //{

    //    private SocketAsyncEventArgs _socketAsyncEventArgs = null;

    //    /// <summary>
    //    /// Creates an empty <see cref="T:System.Net.Sockets.SocketAsyncEventArgs"/> instance.
    //    /// </summary>
    //    /// <exception cref="T:System.NotSupportedException">The platform is not supported. </exception>
    //    public SocketAsyncEventArgsProxy()
    //    {
    //        _socketAsyncEventArgs = new SocketAsyncEventArgs();
    //    }

    //    /// <summary>
    //    /// Sets the data buffer to use with an asynchronous socket method.
    //    /// </summary>
    //    /// <param name="buffer">The data buffer to use with an asynchronous socket method.</param><param name="offset">The offset, in bytes, in the data buffer where the operation starts.</param><param name="count">The maximum amount of data, in bytes, to send or receive in the buffer.</param><exception cref="T:System.ArgumentException">There are ambiguous buffers specified. This exception occurs if the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.Buffer"/> property is also not null and the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.BufferList"/> property is also not null.</exception><exception cref="T:System.ArgumentOutOfRangeException">An argument was out of range. This exception occurs if the <paramref name="offset"/> parameter is less than zero or greater than the length of the array in the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.Buffer"/> property. This exception also occurs if the <paramref name="count"/> parameter is less than zero or greater than the length of the array in the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.Buffer"/> property minus the <paramref name="offset"/> parameter.</exception>
    //    public void SetBuffer(byte[] buffer, int offset, int count);
    //    /// <summary>
    //    /// Sets the data buffer to use with an asynchronous socket method.
    //    /// </summary>
    //    /// <param name="offset">The offset, in bytes, in the data buffer where the operation starts.</param><param name="count">The maximum amount of data, in bytes, to send or receive in the buffer.</param><exception cref="T:System.ArgumentException">There are ambiguous buffers specified. This exception occurs if the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.Buffer"/> property is also not null and the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.BufferList"/> property is also not null.</exception><exception cref="T:System.ArgumentOutOfRangeException">An argument was out of range. This exception occurs if the <paramref name="offset"/> parameter is less than zero or greater than the length of the array in the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.Buffer"/> property. This exception also occurs if the <paramref name="count"/> parameter is less than zero or greater than the length of the array in the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.Buffer"/> property minus the <paramref name="offset"/> parameter.</exception>
    //    public void SetBuffer(int offset, int count);
    //    /// <summary>
    //    /// Releases the unmanaged resources used by the <see cref="T:System.Net.Sockets.SocketAsyncEventArgs"/> instance and optionally disposes of the managed resources.
    //    /// </summary>
    //    public void Dispose();

    //    /// <summary>
    //    /// Gets or sets the socket to use or the socket created for accepting a connection with an asynchronous socket method.
    //    /// </summary>
    //    /// 
    //    /// <returns>
    //    /// The <see cref="T:System.Net.Sockets.Socket"/> to use or the socket created for accepting a connection with an asynchronous socket method.
    //    /// </returns>
    //    public Socket AcceptSocket { get; set; }
    //    /// <summary>
    //    /// Gets the data buffer to use with an asynchronous socket method.
    //    /// </summary>
    //    /// 
    //    /// <returns>
    //    /// A <see cref="T:System.Byte"/> array that represents the data buffer to use with an asynchronous socket method.
    //    /// </returns>
    //    public byte[] Buffer { get; }
    //    /// <summary>
    //    /// Gets the offset, in bytes, into the data buffer referenced by the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.Buffer"/> property.
    //    /// </summary>
    //    /// 
    //    /// <returns>
    //    /// An <see cref="T:System.Int32"/> that contains the offset, in bytes, into the data buffer referenced by the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.Buffer"/> property.
    //    /// </returns>
    //    public int Offset { get; }
    //    /// <summary>
    //    /// Gets the maximum amount of data, in bytes, to send or receive in an asynchronous operation.
    //    /// </summary>
    //    /// 
    //    /// <returns>
    //    /// An <see cref="T:System.Int32"/> that contains the maximum amount of data, in bytes, to send or receive.
    //    /// </returns>
    //    public int Count { get; }
    //    /// <summary>
    //    /// Gets or sets an array of data buffers to use with an asynchronous socket method.
    //    /// </summary>
    //    /// 
    //    /// <returns>
    //    /// An <see cref="T:System.Collections.IList"/> that represents an array of data buffers to use with an asynchronous socket method.
    //    /// </returns>
    //    /// <exception cref="T:System.ArgumentException">There are ambiguous buffers specified on a set operation. This exception occurs if a value other than null is passed and the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.Buffer"/> property is also not null.</exception>
    //    public IList<ArraySegment<byte>> BufferList { get; set; }
    //    /// <summary>
    //    /// Gets the number of bytes transferred in the socket operation.
    //    /// </summary>
    //    /// 
    //    /// <returns>
    //    /// An <see cref="T:System.Int32"/> that contains the number of bytes transferred in the socket operation.
    //    /// </returns>
    //    public int BytesTransferred { get; }
    //    /// <summary>
    //    /// Gets or sets a value that specifies if socket can be reused after a disconnect operation.
    //    /// </summary>
    //    /// 
    //    /// <returns>
    //    /// A <see cref="T:System.Boolean"/> that specifies if socket can be reused after a disconnect operation.
    //    /// </returns>
    //    public bool DisconnectReuseSocket { get; set; }
    //    /// <summary>
    //    /// Gets the type of socket operation most recently performed with this context object.
    //    /// </summary>
    //    /// 
    //    /// <returns>
    //    /// A <see cref="T:System.Net.Sockets.SocketAsyncOperation"/> instance that indicates the type of socket operation most recently performed with this context object.
    //    /// </returns>
    //    public SocketAsyncOperation LastOperation { get; }
    //    /// <summary>
    //    /// Gets the IP address and interface of a received packet.
    //    /// </summary>
    //    /// 
    //    /// <returns>
    //    /// An <see cref="T:System.Net.Sockets.IPPacketInformation"/> instance that contains the IP address and interface of a received packet.
    //    /// </returns>
    //    public IPPacketInformation ReceiveMessageFromPacketInfo { get; }
    //    /// <summary>
    //    /// Gets or sets the remote IP endpoint for an asynchronous operation.
    //    /// </summary>
    //    /// 
    //    /// <returns>
    //    /// An <see cref="T:System.Net.EndPoint"/> that represents the remote IP endpoint for an asynchronous operation.
    //    /// </returns>
    //    public EndPoint RemoteEndPoint { get; set; }
    //    /// <summary>
    //    /// Gets or sets an array of buffers to be sent for an asynchronous operation used by the <see cref="M:System.Net.Sockets.Socket.SendPacketsAsync(System.Net.Sockets.SocketAsyncEventArgs)"/> method.
    //    /// </summary>
    //    /// 
    //    /// <returns>
    //    /// An array of <see cref="T:System.Net.Sockets.SendPacketsElement"/> objects that represent an array of buffers to be sent.
    //    /// </returns>
    //    public SendPacketsElement[] SendPacketsElements { get; set; }
    //    /// <summary>
    //    /// Gets or sets a bitwise combination of <see cref="T:System.Net.Sockets.TransmitFileOptions"/> values for an asynchronous operation used by the <see cref="M:System.Net.Sockets.Socket.SendPacketsAsync(System.Net.Sockets.SocketAsyncEventArgs)"/> method.
    //    /// </summary>
    //    /// 
    //    /// <returns>
    //    /// A <see cref="T:System.Net.Sockets.TransmitFileOptions"/> that contains a bitwise combination of values that are used with an asynchronous operation.
    //    /// </returns>
    //    public TransmitFileOptions SendPacketsFlags { get; set; }
    //    /// <summary>
    //    /// Gets or sets the size, in bytes, of the data block used in the send operation.
    //    /// </summary>
    //    /// 
    //    /// <returns>
    //    /// An <see cref="T:System.Int32"/> that contains the size, in bytes, of the data block used in the send operation.
    //    /// </returns>
    //    public int SendPacketsSendSize { get; set; }
    //    /// <summary>
    //    /// Gets or sets the result of the asynchronous socket operation.
    //    /// </summary>
    //    /// 
    //    /// <returns>
    //    /// A <see cref="T:System.Net.Sockets.SocketError"/> that represents the result of the asynchronous socket operation.
    //    /// </returns>
    //    public SocketError SocketError { get; set; }
    //    /// <summary>
    //    /// Gets the results of an asynchronous socket operation or sets the behavior of an asynchronous operation.
    //    /// </summary>
    //    /// 
    //    /// <returns>
    //    /// A <see cref="T:System.Net.Sockets.SocketFlags"/> that represents the results of an asynchronous socket operation.
    //    /// </returns>
    //    public SocketFlags SocketFlags { get; set; }
    //    /// <summary>
    //    /// Gets or sets a user or application object associated with this asynchronous socket operation.
    //    /// </summary>
    //    /// 
    //    /// <returns>
    //    /// An object that represents the user or application object associated with this asynchronous socket operation.
    //    /// </returns>
    //    public object UserToken { get; set; }
    //    /// <summary>
    //    /// The event used to complete an asynchronous operation.
    //    /// </summary>
    //    public event EventHandler<SocketAsyncEventArgs> Completed;
    //}
}
