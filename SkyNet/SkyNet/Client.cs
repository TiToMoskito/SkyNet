using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SkyNet
{
    public class Client : SystemCallbacks, IDisposable
    {
        #region Public Variables        

        #endregion

        #region Private Variables        
        // Protocol Version
        private const uint m_version = 25022018;

        // Network Configuration
        private Config m_config = null;

        // Dictionary of all the active players
        private Dictionary<int, Connection> m_connections = new Dictionary<int, Connection>();

        // List of all the channels player is in.
        private List<Channel> m_channels = new List<Channel>();

        // Host
        private UDPPeer m_peer;
        private UDPHost m_host;
        private Thread m_pollThread;
        private Task m_task;
        private Connection m_connection;
        private NetBuffer m_bitPacker;
        private bool m_isRunning;
        private object m_lock = 0;
        private string m_playerName;
        #endregion

        #region Properties      
        /// <summary>
        /// Return the all players.
        /// </summary>
        public IEnumerable<Connection> Connections { get { return m_connections.Values.ToList(); } }

        /// <summary>
        /// Return the connection status.
        /// </summary>
        public bool isConnected { get; private set; }

        /// <summary>
        /// Return the local player.
        /// </summary>
        public Connection Connection { get { return m_connection; } }

        /// <summary>
        /// Name of this player.
        /// </summary>
        public string playerName
        {
            get
            {
                return m_connection == null ? m_playerName : m_connection.ClientName;
            }
            set
            {
                if (m_connection == null)
                {
                    m_playerName = value;
                }
                else
                {
                    m_connection.ClientName = value;
                    if (isConnected)
                    {
                        //RequestSetName
                    }
                }
            }
        }

        #endregion

        #region Client       
        internal Client()
        {
            UDPLibrary.Initialize();
            m_config = Config.instance;

            m_peer = new UDPPeer();
            m_host = new UDPHost();
            m_host.Create(null, 1);
            m_host.EnableCompression();

            m_connections = new Dictionary<int, Connection>();
            m_channels = new List<Channel>();
            m_bitPacker = new NetBuffer();

            isConnected = false;

            m_isRunning = true;
            m_task = Listen();
        }

        internal void Connect(string _ip, ushort _port)
        {
            if (isConnected)
                return;

            UDPAddress address = new UDPAddress();
            address.SetHost(_ip);
            address.Port = _port;

            m_peer = new UDPPeer();
            m_peer = m_host.Connect(address, m_config.channelLimit, 0);
        }

        internal void Disconnect()
        {
            if (m_connection != null)
                m_connection.Disconnect();

            m_connections.Clear();
            m_channels.Clear();

            isConnected = false;
            m_isRunning = false;
            m_peer.Reset();
        }

        public void Dispose()
        {
            Disconnect();
            UDPLibrary.Deinitialize();
        }
        #endregion

        #region Connection       
        internal void AddConnection(ResponseClientJoinedEvent _evnt)
        {
            Connection conn = new Connection(_evnt.clientID, _evnt.playerName);
            m_connections.Add(_evnt.clientID, conn);
            if (onChannelJoined != null) onChannelJoined(conn, _evnt.channelID, "");
        }

        internal void RemoveConnection(ResponseClientLeftEvent _evnt)
        {
            Connection conn = null;
            m_connections.TryGetValue(_evnt.clientID, out conn);
            m_connections.Remove(_evnt.clientID);
            if (onChannelLeft != null) onChannelLeft(conn, _evnt.channelID, "");
        }
        internal Connection GetConnection(int _playerID)
        {
            Connection conn = null;
            m_connections.TryGetValue(_playerID, out conn);
            return conn;
        }

        /// <summary>
        /// Retrieve a player by their ID.
        /// </summary>
        internal Connection GetConnection(int id, bool createIfMissing)
        {
            if (isConnected)
            {
                Connection player = null;
                m_connections.TryGetValue(id, out player);

                if (player == null && createIfMissing)
                {
                    player = new Connection(id, "Player");
                    m_connections[id] = player;
                }
                return player;
            }
            return null;
        }

        /// <summary>
        /// Retrieve a player by their name.
        /// </summary>
        internal Connection GetConnection(string name)
        {
            foreach (var p in m_connections)
            {
                if (p.Value.ClientName == name)
                    return p.Value;
            }
            return null;
        }

        internal void ResponseClientID(ResponseClientIDEvent evnt)
        {
            isConnected = true;
            ResponseClientIDEvent rpidEvent = m_bitPacker.ReadEvent<ResponseClientIDEvent>();
            m_connection.ClientID = rpidEvent.clientID;
            if (onConnected != null) onConnected();
        }
        #endregion

        #region Channel
        internal void ResponseJoinChannel(Connection _source, ResponseJoinChannelEvent evnt)
        {
            if (onChannelJoined != null) onChannelJoined(_source, evnt.channelID, evnt.message);
        }

        internal void ResponseLoadLevelEvent(ResponseLoadLevelEvent evnt)
        {
            if (onLoadLevel != null) onLoadLevel(evnt.channelID, evnt.levelName);
        }

        internal void ResponseCreateObject(ResponseCreateObjectEvent evnt)
        {
            if (onObjectCreated != null) onObjectCreated(evnt);
        }

        internal void ResponseDestroyObject(ResponseDestroyObjectEvent evnt)
        {
            if (onObjectDestroyed != null) onObjectDestroyed(evnt);
        }

        internal void Event(TypeId _typeId, NetBuffer _packer, Connection _source)
        {
            if (onDispatchEvent != null) onDispatchEvent(_typeId, _packer, _source);
        }

        internal void State(NetworkId _netId, NetBuffer _packer)
        {
            if (onDispatchState != null) onDispatchState(_netId, _packer);
        }

        internal void JoiningChannel(ResponseJoiningChannelEvent _evnt)
        {
            Channel channel = new Channel(_evnt.channelID);

            for (int i = 0; i < _evnt.connections.Length; i++)
            {
                Connection conn = GetConnection((byte)_evnt.connections[i]);
                if (conn != null)
                {
                    channel.AddPlayer(conn);
                }
            }
            m_connection.AddChannel(channel);
        }

        #endregion

        #region Listen Task
        internal async Task Listen()
        {
            await Task.Factory.StartNew(() =>
            {
                while (m_isRunning)
                {
                    m_host.Service(1000 / m_config.tickRate, out UDPEvent evnt);

                    switch (evnt.Type)
                    {
                        case UDPEventType.Connect:
                            SkyLog.Debug("Connect");
                            string tmpName = playerName;
                            m_connection = new Connection(evnt.Peer);
                            playerName = tmpName;
                            RequestClientIDEvent RPIDEvent = new RequestClientIDEvent();
                            RPIDEvent.version = m_version;
                            RPIDEvent.name = tmpName;
                            m_connection.SendEvent(RPIDEvent);
                            break;
                        case UDPEventType.Disconnect:
                            SkyLog.Debug("Disconnect");
                            Disconnect();
                            if (onDisconnect != null) onDisconnect();
                            break;
                        case UDPEventType.Receive:
                            SkyLog.Debug("Receive");
                            NetStats.ReceivePacket(evnt.Packet.Length);
                            ProcessPackets(new Packet(evnt.Packet));
                            evnt.Packet.Dispose();
                            break;
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }
        #endregion

        #region Process Packets   
        private void ProcessPackets(Packet _packet)
        {
            SkyLog.Debug("type: " + _packet.Header.type);

            switch (_packet.Header.type)
            {
                case PacketType.ResponseClientID:
                    isConnected = true;
                    ResponseClientIDEvent rpidEvent = _packet.Content.ReadEvent<ResponseClientIDEvent>();
                    m_connection.ClientID = rpidEvent.clientID;
                    if (onConnected != null) onConnected();
                    break;
                case PacketType.ResponseJoinChannel:
                    ResponseJoinChannelEvent rJoinChannelEvent = _packet.Content.ReadEvent<ResponseJoinChannelEvent>();
                    if (onChannelJoined != null) onChannelJoined(GetConnection((int)_packet.Header.connection), rJoinChannelEvent.channelID, rJoinChannelEvent.message);
                    break;
                case PacketType.ResponseJoiningChannel:
                    JoiningChannel(_packet.Content.ReadEvent<ResponseJoiningChannelEvent>());
                    break;
                case PacketType.ResponseLoadLevel:
                    ResponseLoadLevelEvent RLLEvent = _packet.Content.ReadEvent<ResponseLoadLevelEvent>();
                    if (onLoadLevel != null) onLoadLevel(RLLEvent.channelID, RLLEvent.levelName);
                    break;
                case PacketType.ResponseClientJoined:
                    AddConnection(_packet.Content.ReadEvent<ResponseClientJoinedEvent>());
                    break;
                case PacketType.ResponseClientLeft:
                    RemoveConnection(_packet.Content.ReadEvent<ResponseClientLeftEvent>());
                    break;
                case PacketType.ResponseCreateObject:
                    if (onObjectCreated != null) onObjectCreated(_packet.Content.ReadEvent<ResponseCreateObjectEvent>());
                    break;
                case PacketType.ResponseDestroyObject:
                    if (onObjectDestroyed != null) onObjectDestroyed(_packet.Content.ReadEvent<ResponseDestroyObjectEvent>());
                    break;
                default:
                    ProcessData(_packet.Header);
                    break;
            }
            m_bitPacker.Flush();
        }

        private void ProcessData(Header _header)
        {
            switch (_header.type)
            {
                case PacketType.Raw:
                    break;
                case PacketType.Event:
                    if (onDispatchEvent != null) onDispatchEvent(_header.typeId, _header.Package, GetConnection((int)_header.connection));
                    break;
                case PacketType.State:
                    if (onDispatchState != null) onDispatchState(_header.networkId, _header.Package);
                    break;
            }
        }
        #endregion

        #region Send Packets
        #endregion
    }
}
