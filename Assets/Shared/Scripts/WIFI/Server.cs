using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace LocalNetworking
{
    public class Server : MonoBehaviour
    {
        #region nested classes

        [Serializable]
        public class Message
        {
            public string m_OpCode, m_Msg;

            public Message(string opCode, string msg)
            {
                m_OpCode = opCode;
                m_Msg = msg;
            }
        }

        #endregion

        #region member variables

        /// <summary>
        /// The size of the buffer used to send and receive data
        /// </summary>
        const int BUFFER_SIZE = 100 * 1024;

        /// <summary>
        /// should we debug the communications?
        /// </summary>
        public bool m_Debug = false;

        /// <summary>
        /// is this instance a host?
        /// </summary>
        [HideInInspector]
        public bool host;
        /// <summary>
        /// the port used for communications, MUST BE ABOVE 3K
        /// </summary>
        public int _port = 8008;
        /// <summary>
        /// the socket we will use for communications
        /// </summary>
        public Socket _socket;
        /// <summary>
        /// Events handling
        /// </summary>
        public Action<Exception> OnJoinError;
        public Action OnJoined;
        public Action<Message> OnData;
        public Action<Socket> OnClientConnect;
        public Action<Socket> OnClientDisconnect;
        public Action OnServerShutdown;
        public Action<string> OnDebugMessage;

        /// <summary>
        /// Using a thread so we don't block the program's execution while listening
        /// </summary>
        Thread _clientThread;

        /// <summary>
        /// list of the connected clients
        /// </summary>
        readonly List<Socket> _clients = new();

        /// <summary>
        /// buffer used for sending/receiving data
        /// </summary>
        readonly byte[] _buffer = new byte[BUFFER_SIZE];

        #endregion

        /// <summary>
        /// clean up on disable
        /// </summary>
        void OnDisable()
        {
            if (_socket != null && _socket.IsBound)
            {
                if (host) CloseAllSockets(); else CloseClientConnection();
            }
        }

        /// <summary>
        /// We initiate a server session
        /// </summary>
        public void Host()
        {
            if (m_Debug) Debug.Log("HOST STARTED");
            host = true;

            InitSocket();

            var ip = "";
            IPAddress[] localIp = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress address in localIp)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    ip = address.ToString();
                }
            }
            if (m_Debug) Debug.Log("HOST IP " + ip);

            // ipstring= 192.168.178.20

            //_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            //_socket.Bind(new IPEndPoint(IPAddress.Any, _port));
            _socket.Bind(new IPEndPoint(IPAddress.Parse(ip), _port));
            _socket.Listen(100);
            _socket.BeginAccept(ListenForConnections, null);
        }

        /// <summary>
        /// we initiate a client session
        /// </summary>
        public void Join()
        {
            if (m_Debug) Debug.Log("CLIENT JOINED");
            host = false;

            InitSocket();

            try
            {
                var ip = IPAddress.Loopback;
                ip = IPAddress.Parse("192.168.178.20");
                OnDebugMessage?.Invoke($"IP {ip}:{_port}");

                _socket.Connect(ip, _port);
                _clientThread = new Thread(new ThreadStart(ListenForData))
                {
                    IsBackground = true
                };
                _clientThread.Start();
                OnJoined?.Invoke();
            }
            catch (SocketException ex)
            {
                OnJoinError?.Invoke(ex);
                if (m_Debug) Debug.LogError("An error occurred whilst trying to join a game.");
            }
        }

        void InitSocket()
        {
            _socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Close()
        {
            if (host)
                CloseAllSockets();
            else
                CloseClientConnection();
        }


        /// <summary>
        /// thread used to listen for incoming connections
        /// </summary>
        void ListenForConnections(IAsyncResult AR)
        {
            Socket socket;

            try
            {
                socket = _socket.EndAccept(AR);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            _clients.Add(socket);
            OnClientConnect?.Invoke(socket);
            socket.BeginReceive(_buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveData, socket);
            _socket.BeginAccept(ListenForConnections, null);
        }

        /// <summary>
        /// thread used to listen for incoming data
        /// </summary>
        void ListenForData()
        {
            while (true)
            {
                //unpacke the stream of data
                var buffer = new byte[BUFFER_SIZE];
                int received = _socket.Receive(buffer, SocketFlags.None);
                if (received == 0) return;
                var data = new byte[received];
                Array.Copy(buffer, data, received);

                //decode message and invoke events
                string messageJson = Encoding.ASCII.GetString(data);
                Message msg = JsonUtility.FromJson<Message>(UnpackJson(messageJson));
                UnityMainThreadDispatcher.Instance().Enqueue(OnDataCO(msg));
            }
        }

        /// <summary>
        /// fire up events when data is received, this happens on the main thread
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private IEnumerator OnDataCO(Message msg)
        {
            OnData?.Invoke(msg);
            yield return new WaitForEndOfFrame();
        }

        /// <summary>
        /// simpler version of the Send method used to pack Messages across
        /// </summary>
        /// <param name="opCode"></param>
        /// <param name="text"></param>
        public void Send(string opCode, string text)
        {
            Message msg = new(opCode, text);

            string toSend = PackJson(JsonUtility.ToJson(msg).ToString());
            if (m_Debug) Debug.Log("Sending Message: " + toSend);

            byte[] bytes = Encoding.ASCII.GetBytes(toSend);
            if (host)
            {
                _clients.ForEach(socket =>
                {
                    socket.Send(bytes);
                });
            }
            else
            {
                _socket.Send(bytes);
            }
        }

        /// <summary>
        /// both the server and the client will use this to process received data
        /// </summary>
        /// <param name="AR"></param>
        private void ReceiveData(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            int received;

            try
            {
                received = current.EndReceive(AR);
            }
            catch (SocketException)
            {
                //close the connection and clean up if there's an issue
                if (m_Debug) Debug.LogWarning("Client disconnected");
                current.Close();
                _clients.Remove(current);
                return;
            }

            //get the data
            byte[] recBuf = new byte[received];
            Array.Copy(_buffer, recBuf, received);

            //decode the message
            string messageJson = Encoding.ASCII.GetString(recBuf);
            Message msg = JsonUtility.FromJson<Message>(UnpackJson(messageJson));

            if (m_Debug) Debug.Log(msg.m_OpCode + " - " + msg.m_Msg);

            //invoke events
            OnData?.Invoke(msg);

            //check for special messages
            switch (msg.m_OpCode)
            {
                case "CLIENT_EXIT":
                    // Always Shutdown before closing
                    current.Shutdown(SocketShutdown.Both);
                    current.Close();
                    _clients.Remove(current);
                    OnClientDisconnect?.Invoke(current);
                    if (m_Debug) Debug.Log("Client disconnected");
                    break;

                case "SRV_SHUTDOWN":
                    OnServerShutdown?.Invoke();
                    CloseAllSockets();
                    return;
            }

            //broadcast message back on the server
            if (host) _clients.ForEach(socket =>
            {
                socket.Send(recBuf);
            });

            //restart the cycle
            current.BeginReceive(_buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveData, current);
        }

        /// <summary>
        /// clean exit for the client
        /// </summary>
        private void CloseClientConnection()
        {
            if (_socket != null  && IsSocketConnected(_socket))
            {
                print("SEND CLIENT EXIT to SERVER");
                Send("CLIENT_EXIT", ""); // Tell the server we are exiting
                //CloseClientSocket(_socket);
            }
           // _socket = null;
        }

        /// <summary>
        /// complete server shutdown
        /// </summary>
        private void CloseAllSockets()
        {
            if (m_Debug) Debug.Log("HOST CLOSE");

            //notify clients of the imminent server shutdown
            Message msg = new("SRV_SHUTDOWN", "");

            string toSend = PackJson(JsonUtility.ToJson(msg).ToString());
            byte[] bytes = Encoding.ASCII.GetBytes(toSend);

            _clients.ForEach(socket =>
            {
                socket.Send(bytes);
            });

            try
            {
                foreach (Socket socket in _clients)
                {
                    CloseClientSocket(socket);
                }

            }
            catch (Exception ex)
            {
                if (m_Debug) Debug.LogError("Error closing client socket: " + ex.Message);
            }

            try
            {
                _socket.Close();
            }
            catch (Exception ex)
            {
                if (m_Debug) Debug.LogError("Error closing host socket: " + ex.Message);
            }
        }

        private void CloseClientSocket(Socket socket)
        {
            if (m_Debug) Debug.Log("CLIENT CLOSE");

            try
            {
                socket.Shutdown(SocketShutdown.Both);
                //socket.Disconnect(true);
                socket.Close();
                socket.Dispose();
            }
            catch (Exception ex)
            {
                if (m_Debug) Debug.LogError("Error closing client socket: " + ex.Message);
            }
        }

        bool IsSocketConnected(Socket s)
        {
            return !((s.Poll(1000, SelectMode.SelectRead) && (s.Available == 0)) || !s.Connected);

            /* The long, but simpler-to-understand version:

                    bool part1 = s.Poll(1000, SelectMode.SelectRead);
                    bool part2 = (s.Available == 0);
                    if ((part1 && part2 ) || !s.Connected)
                        return false;
                    else
                        return true;

            */
        }

        #region utility

        public string PackJson(string json)
        {
            string newJson = json.Trim('"');
            newJson = json.Replace('"', '\'');
            return newJson;
        }

        public string UnpackJson(string data)
        {
            string newJson = data.Trim('"');
            newJson = newJson.Replace('\'', '"');
            return newJson;
        }

        public void SendJSON<T>(string opCode, T json)
        {
            string toSend = JsonUtility.ToJson(json);
            Send(opCode, toSend);
        }

        public T ReadJSON<T>(string message)
        {
            T toRead = JsonUtility.FromJson<T>(message);
            return toRead;
        }


        #endregion
    }
}