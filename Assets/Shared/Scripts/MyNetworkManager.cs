using System.Net;
using System.Net.Sockets;
using UnityEngine;
using LocalNetworking;
using UnityEngine.Events;
using System;

public class MyNetworkManager : NetworkManagerBase
{
    public UnityEvent OnConnected, OnDisconnected;
    public UnityEvent<Exception> OnError;
    public UnityEvent<string> OnDebug;

    void Awake() => Init();

    void OnDestroy() => Shutdown();

    void Update()
    {
        if (_started && Input.GetKeyDown(KeyCode.Space))
        {
            _server.Send("msg", "Message!");
        }
    }

    public override void OnClientConnect(Socket connection)
    {
        IPEndPoint endPoint = (IPEndPoint)connection.RemoteEndPoint;
        if (_server.m_Debug) Debug.Log("Client: " + endPoint.Address.ToString() + " Connected to the Server!");

        OnConnected?.Invoke();
    }

    public override void OnJoined()
    {
        OnConnected?.Invoke();
    }

    public override void OnJoinError(Exception ex)
    {
        OnError?.Invoke(ex);
    }

    public override void OnClientDisconnect(Socket connection)
    {
        base.OnClientDisconnect(connection);

        IPEndPoint ipEndpoint = (IPEndPoint)connection.RemoteEndPoint;
        if (_server.m_Debug) Debug.Log("Client disconnected from: " + ipEndpoint.Address.ToString());
    }

    public override void OnServerShutdown()
    {
        base.OnServerShutdown();

        if (_server.m_Debug) Debug.Log("Server shut down");
    }

    public override void OnData(Server.Message message)
    {
        base.OnData(message);

        switch (message.m_OpCode)
        {
            case "msg":
                Debug.Log(message.m_Msg);
                break;
        }
    }

    public override void OnDebugMessage(string msg)
    {
        OnDebug?.Invoke(msg);

    }

}
