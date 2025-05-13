using System;
using FarmingSimulatorTelemetryClient.PipeServer.Models.EventArgs;

namespace FarmingSimulatorTelemetryClient.PipeServer.Interfaces;

public interface ICommunicationServer
{
    string ServerId { get; }
    event EventHandler<ClientConnectedArgs> OnClientConnectHandler;
    event EventHandler<ClientDisconnectedArgs> OnClientDisconnectHandler;
    event EventHandler<MessageReceivedArgs> OnReceiveMessageHandler;
    void Start();
    void Stop();
}