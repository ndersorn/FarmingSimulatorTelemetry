using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using FarmingSimulatorTelemetryClient.PipeServer.Interfaces;
using FarmingSimulatorTelemetryClient.PipeServer.Models.EventArgs;
using FarmingSimulatorTelemetryClient.PipeServer.Utilities.Extensions;

namespace FarmingSimulatorTelemetryClient.PipeServer.Services;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class FSNamedPipeReader(string pipeName) : ICommunicationServer
{
    public const int MaxNumberOfServerInstances = 10;

    public string ServerId { get; } = pipeName;
    public event EventHandler<ClientConnectedArgs>? OnClientConnectHandler;
    public event EventHandler<ClientDisconnectedArgs>? OnClientDisconnectHandler;
    public event EventHandler<MessageReceivedArgs>? OnReceiveMessageHandler;

    private readonly SynchronizationContext _synchronizationContext = AsyncOperationManager.SynchronizationContext;
    private readonly IDictionary<string, ICommunicationServer> _servers = new ConcurrentDictionary<string, ICommunicationServer>();

    public void Start() =>
        StartNewInternalServer();

    public void Stop()
    {
        // stop each connected server
        foreach (var (_, server) in _servers)
        {
            UnregisterFromServerEvents(server);
            server.Stop();
        }

        // clear conBag
        _servers.Clear();
    }

    private void StartNewInternalServer()
    {
        var server = new FSPipeServer(ServerId, MaxNumberOfServerInstances);
        _servers[server.Id] = server;
        server.OnClientConnectHandler += OnClientConnect;
        server.OnClientDisconnectHandler += OnClientDisconnect;
        server.OnReceiveMessageHandler += OnReceiveMessage;
        server.Start();
    }

    private void StopInternalServer(string? id)
    {
        if (id != null && _servers.ContainsKey(id))
        {
            UnregisterFromServerEvents(_servers[id]);
            _servers[id].Stop();
            _servers.Remove(id);
        }
    }

    private void UnregisterFromServerEvents(ICommunicationServer server)
    {
        server.OnClientConnectHandler -= OnClientConnect;
        server.OnClientDisconnectHandler -= OnClientDisconnect;
        server.OnReceiveMessageHandler -= OnReceiveMessage;
    }

    #region Events

    private void OnClientConnect(object? sender, ClientConnectedArgs args)
    {
        _synchronizationContext.Post(e =>
                OnClientConnectHandler.SafeInvoke(this, (ClientConnectedArgs)e!), args
        );
        StartNewInternalServer();
    }

    private void OnClientDisconnect(object? sender, ClientDisconnectedArgs args)
    {
        _synchronizationContext.Post(e =>
                OnClientDisconnectHandler.SafeInvoke(this, (ClientDisconnectedArgs)e!), args
        );
        StopInternalServer(args.ClientId);
    }

    private void OnReceiveMessage(object? sender, MessageReceivedArgs args)
    {
        _synchronizationContext.Post(e =>
                OnReceiveMessageHandler.SafeInvoke(this, (MessageReceivedArgs)e!), args
        );
    }

    #endregion
}