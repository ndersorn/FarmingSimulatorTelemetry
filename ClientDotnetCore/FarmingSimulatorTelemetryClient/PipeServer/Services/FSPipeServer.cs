using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using FarmingSimulatorTelemetryClient.PipeServer.Interfaces;
using FarmingSimulatorTelemetryClient.PipeServer.Models.EventArgs;
using FarmingSimulatorTelemetryClient.PipeServer.Models.Internal;

namespace FarmingSimulatorTelemetryClient.PipeServer.Services;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class FSPipeServer(string pipeName, int maxNumberOfServerInstances) : ICommunicationServer
{
    public readonly string Id = Guid.NewGuid().ToString();

    public bool IsStopping;
    public string ServerId { get; } = pipeName;
    public event EventHandler<ClientConnectedArgs>? OnClientConnectHandler;
    public event EventHandler<ClientDisconnectedArgs>? OnClientDisconnectHandler;
    public event EventHandler<MessageReceivedArgs>? OnReceiveMessageHandler;

    private const int _bufferSize = 2048;
    private readonly Lock _lockingObject = new();
    private readonly NamedPipeServerStream pipeServer = new(pipeName, PipeDirection.In, maxNumberOfServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous);

    /// <summary>
    /// This method begins an asynchronous operation to wait for a client to connect.
    /// </summary>
    public void Start()
    {
        pipeServer.BeginWaitForConnection(WaitForConnectionCallback, null);
    }

    /// <summary>
    /// This method disconnects, closes and disposes the server
    /// </summary>
    public void Stop()
    {
        IsStopping = true;
        try
        {
            if (pipeServer.IsConnected)
                pipeServer.Disconnect();
        }
        finally
        {
            pipeServer.Close();
            pipeServer.Dispose();
        }
    }

    #region Server Callbacks

    /// <summary>
    /// This method begins an asynchronous read operation. 
    /// </summary>
    private void BeginRead(Info info)
    {
        pipeServer.BeginRead(info.Buffer, 0, _bufferSize, EndReadCallback, info);
    }

    /// <summary>
    /// This callback is called when the async WaitForConnection operation is completed,
    /// whether a connection was made or not. WaitForConnection can be completed when the server disconnects.
    /// </summary>
    private void WaitForConnectionCallback(IAsyncResult result)
    {
        if (!IsStopping)
        {
            lock (_lockingObject)
            {
                if (!IsStopping)
                {
                    // Call EndWaitForConnection to complete the connection operation
                    pipeServer.EndWaitForConnection(result);
                    OnConnect();
                    BeginRead(new Info(_bufferSize));
                }
            }
        }
    }

    /// <summary>
    /// This callback is called when the BeginRead operation is completed.
    /// We can arrive here whether the connection is valid or not
    /// </summary>
    private void EndReadCallback(IAsyncResult result)
    {
        var readBytes = pipeServer.EndRead(result);
        if (readBytes > 0)
        {
            var info = (Info?)result.AsyncState;
            if (info == null)
                return;

            // Get the read bytes and append them
            info.StringBuilder.Append(Encoding.UTF8.GetString(info.Buffer, 0, readBytes));

            // Message is not complete, continue reading
            if (!pipeServer.IsMessageComplete)
                BeginRead(info);

            // Message is completed
            else
            {
                // Finalize the received string and fire MessageReceivedEvent
                var message = info.StringBuilder.ToString().TrimEnd('\0');
                OnReceiveMessage(message);
                // Begin a new reading operation
                BeginRead(new Info(_bufferSize));
            }
        }

        // When no bytes were read, it can mean that the client have been disconnected
        else
        {
            if (!IsStopping)
            {
                lock (_lockingObject)
                {
                    if (!IsStopping)
                    {
                        OnDisconnect();
                        Stop();
                    }
                }
            }
        }
    }

    #endregion

    #region Events firing

    /// <summary>
    /// This method fires ConnectedEvent 
    /// </summary>
    private void OnConnect() =>
        OnClientConnectHandler?.Invoke(this, new ClientConnectedArgs { ClientId = Id });

    /// <summary>
    /// This method fires DisconnectedEvent 
    /// </summary>
    private void OnDisconnect() =>
        OnClientDisconnectHandler?.Invoke(this, new ClientDisconnectedArgs { ClientId = Id });

    /// <summary>
    /// This method fires MessageReceivedEvent with the given message
    /// </summary>
    private void OnReceiveMessage(string message) =>
        OnReceiveMessageHandler?.Invoke(this, new MessageReceivedArgs { Message = message });

    #endregion
}