using System;
using System.Net.Sockets;
using UnityEngine;

namespace LocalNetworking
{
    [RequireComponent(typeof(Server))]
    public class NetworkManagerBase : MonoBehaviour
    {
        #region member variables

        protected Server _server;
        protected bool _started = false;

        #endregion

        /// <summary>
        /// Starts the server and binds the event listeners
        /// </summary>
        public void Init()
        {
            _server = GetComponent<Server>();
            _server.OnJoinError += OnJoinError;
            _server.OnJoined += OnJoined;
            _server.OnData += OnData;
            _server.OnClientConnect += OnClientConnect;
            _server.OnClientDisconnect += OnClientDisconnect;
            _server.OnServerShutdown += OnServerShutdown;
            _server.OnDebugMessage += OnDebugMessage;
        }

        /// <summary>
        /// Unbinds the event listeners
        /// </summary>
        public void Shutdown()
        {
            _server.OnJoinError -= OnJoinError;
            _server.OnJoined -= OnJoined;
            _server.OnData -= OnData;
            _server.OnClientConnect -= OnClientConnect;
            _server.OnClientDisconnect -= OnClientDisconnect;
            _server.OnServerShutdown -= OnServerShutdown;
            _server.OnDebugMessage -= OnDebugMessage;
        }

        /// <summary>
        /// Starts the Networking process as a Host
        /// </summary>
        public void Host()
        {
            if (!_started)
            {
                _started = true;
                _server.Host();
            }
        }

        /// <summary>
        /// Starts the Networking process as a Client
        /// </summary>
        public void Join()
        {
            if (!_started)
            {
                _started = true;
                _server.Join();
            }
        }

        /// <summary>
        /// Close the Networking connection
        /// </summary>
        public void Close()
        {
            if (_started)
            {
                _started = false;
                _server.Close();
            }
        }

        #region events


        /// <summary>
        /// Called when a client failed to connect to the server
        /// </summary>
        /// <param name="connection"></param>
        public virtual void OnJoinError(Exception ex) { }

        public virtual void OnJoined() { }
        /// <summary>
        /// Called whenever a packet of data is received from the server
        /// </summary>
        /// <param name="message"></param>
        public virtual void OnData(Server.Message message) { }

        /// <summary>
        /// Called when a client connects to the server
        /// </summary>
        /// <param name="connection"></param>
        public virtual void OnClientConnect(Socket connection) { }

        /// <summary>
        /// called whenever a client is disconnected
        /// </summary>
        public virtual void OnClientDisconnect(Socket connection){}

        /// <summary>
        /// Called when the server closes the connection with the clients
        /// </summary>
        public virtual void OnServerShutdown() => Shutdown();

        public virtual void OnDebugMessage(string msg) { }

        #endregion
    }
}