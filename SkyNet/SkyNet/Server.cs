using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SkyNet
{
    public class Server : SystemCallbacks, IDisposable
    {
        #region Public Variables        

        #endregion

        #region Private Variables        
        // Protocol Version
        private const uint m_version = 25022018;

        // Network Configuration
        private Config m_config = null;

        // Dictionary of all the active Clients on server.
        private Dictionary<IPEndPoint, Connection> m_connections = new Dictionary<IPEndPoint, Connection>();

        // Dictionary of all the active channels.
        private Dictionary<int, Channel> m_channels = new Dictionary<int, Channel>();

        // Host
        private UDPHost m_host;
        private Random m_random;
        private NetBuffer m_bitPacker;
        private Thread m_pollThread;
        private Task m_task;
        private bool m_isRunning = false;
        private object m_lock = 0;
        private string m_serverName;
        private int m_clientCounter = 0;
        private int m_entityCounter = 0;
        #endregion

        #region Properties        
        /// <summary>
        /// Get the server Name
        /// </summary>
        public string ServerName { get { return m_serverName; } }

        /// <summary>
        /// Get Clients Counter
        /// </summary>
        public int Clients { get { return m_connections.Count; } }

        /// <summary>
        /// How many Entities are created
        /// </summary>
        public int Entities { get { return m_entityCounter; } }
        #endregion

        #region Server   
        internal Server()
        {
            UDPLibrary.Initialize();
            m_config = Config.instance;

            m_host = new UDPHost();

            m_connections = new Dictionary<IPEndPoint, Connection>();
            m_channels = new Dictionary<int, Channel>();
            m_random = new Random();
            m_bitPacker = new NetBuffer();

            m_isRunning = false;
        }

        internal void Start(string _serverName)
        {
            UDPAddress adr = new UDPAddress();
            adr.Port = Config.instance.serverPort;
            m_host.Create(adr, Config.instance.serverConnectionLimit, Config.instance.channelLimit);
            m_host.EnableCompression();

            m_isRunning = true;
            m_task = Listen();

            m_serverName = _serverName;                     
        }

        internal void Stop()
        {
            for (int i = 0; i < m_connections.Count; i++)
            {
                m_connections.ElementAt(i).Value.Disconnect();
            }

            m_connections.Clear();
            m_channels.Clear();

            m_host = new UDPHost();
            m_isRunning = false;

            m_task.Dispose();
        }

        public void Dispose()
        {
            Stop();
            UDPLibrary.Deinitialize();
        }
        #endregion

        #region Connection       
        private Connection AddConnection(UDPPeer _peer)
        {
            Connection conn = new Connection(_peer);
            m_connections.Add(_peer.GetIPEndPoint, conn);
            ++m_clientCounter;          
            return conn;
        }

        private Connection RemoveConnection(IPEndPoint _endpoint)
        {
            Connection conn;
            m_connections.TryGetValue(_endpoint, out conn);
            if (conn == null)
                return null;

            lock (m_lock) { --m_clientCounter; }

            LeaveAllChannels(conn);

            m_connections.Remove(_endpoint);
            SkyLog.Info("Client ({0}:{1}) {2} disconnected", _endpoint, conn.ClientID, conn.ClientName);
            return conn;
        }

        private Connection GetClient(IPEndPoint _endpoint)
        {
            Connection connection = null;
            m_connections.TryGetValue(_endpoint, out connection);
            return connection;
        }

        internal Connection GetClient(uint _ClientID)
        {
            Connection connection = null;
            for (int i = 0; i < m_connections.Count; i++)
            {
                if (m_connections.ElementAt(i).Value.ClientID == _ClientID)
                {
                    connection = m_connections.ElementAt(i).Value;
                    break;
                }
            }
            return connection;
        }

        internal void RequestClientID(Connection _source, RequestClientIDEvent _evnt)
        {
            SkyLog.Info("Client ({0}:{1}) {2} {3}", _source.IPAddress, _source.ClientID, _evnt.name, _evnt.version);

            if (_evnt.version == m_version)
            {
                _source.ClientName = _evnt.name;

                SkyLog.Info("Client ({0}:{1}) {2} connected", _source.IPAddress, _source.ClientID, _source.ClientName);

                ResponseClientIDEvent evnt = new ResponseClientIDEvent();
                evnt.clientID = _source.ClientID;
                _source.SendEvent(evnt);
            }
        }

        private void ResponseClientDisconnected(Connection _conn)
        {
            for (int i = 0; i < _conn.Channels.Count; i++)
            {
                for (int x = 0; x < _conn.Channels[i].Clients.Count; x++)
                {
                    ResponseClientLeftEvent evnt = new ResponseClientLeftEvent();
                    evnt.channelID = _conn.Channels[i].ChannelID;
                    evnt.clientID = _conn.ClientID;
                    _conn.Channels[i].Clients[x].SendEvent(evnt);
                }
            }
        }

        private void ResponseClientLeftChannel(Connection _conn, Channel _channel)
        {
            for (int i = 0; i < _channel.Clients.Count; i++)
            {
                ResponseClientLeftEvent evnt = new ResponseClientLeftEvent();
                evnt.channelID = _channel.ChannelID;
                evnt.clientID = _conn.ClientID;
                _channel.Clients[i].SendEvent(evnt);
            }
        }
        #endregion

        #region Channel   
        private Channel CreateChannel(int channelID, out bool isNew)
        {
            Channel channel;

            if (m_channels.TryGetValue(channelID, out channel))
            {
                isNew = false;
                if (!channel.isOpen) return null;
                return channel;
            }

            channel = new Channel(channelID);
            m_channels[channelID] = channel;
            isNew = true;
            return channel;
        }

        private Channel GetChannel(int channelID)
        {
            Channel channel = null;
            m_channels.TryGetValue(channelID, out channel);
            return channel;
        }

        internal void RequestJoinChannel(Connection _source, RequestJoinChannelEvent _evnt)
        {
            bool isNew;
            Channel channel = CreateChannel(_evnt.channelID, out isNew);

            if (channel == null || !channel.isOpen)
            {
                ResponseJoinChannelEvent evnt = new ResponseJoinChannelEvent();
                evnt.channelID = _evnt.channelID;
                evnt.message = "The requested channel is closed";
                _source.SendEvent(evnt);
            }
            else if (isNew)
            {
                channel.Setup(_evnt.password, _evnt.persistent, _evnt.levelName, _evnt.playerLimit);
                SendJoinChannel(_source, channel, _evnt.levelName);
            }
            else if (string.IsNullOrEmpty(channel.Password) || (channel.Password == _evnt.password))
            {
                SendJoinChannel(_source, channel, _evnt.levelName);
            }
            else
            {
                ResponseJoinChannelEvent evnt = new ResponseJoinChannelEvent();
                evnt.channelID = channel.ChannelID;
                evnt.message = "Wrong password";
                _source.SendEvent(evnt);
            }
        }

        private void SendJoinChannel(Connection _source, Channel channel, string requestedLevelName)
        {
            // Set the Client's channel
            _source.AddChannel(channel);

            // Tell the Client who else is in the channel
            ResponseJoiningChannelEvent rJoinChannelEvent = new ResponseJoiningChannelEvent();
            rJoinChannelEvent.channelID = channel.ChannelID;
            rJoinChannelEvent.connections = channel.GetAllPlayers();
            _source.SendEvent(rJoinChannelEvent);

            // Send the LoadLevel packet, but only if some level name was specified in the original LoadLevel request.
            if (!string.IsNullOrEmpty(requestedLevelName) && !string.IsNullOrEmpty(channel.Level))
            {
                ResponseLoadLevelEvent rLoadLevelEvent = new ResponseLoadLevelEvent();
                rLoadLevelEvent.channelID = channel.ChannelID;
                rLoadLevelEvent.levelName = channel.Level;
                _source.SendEvent(rLoadLevelEvent);
            }

            // Send the list of objects that have been created
            for (int i = 0; i < channel.CreatedObjects.Count; i++)
            {
                var obj = channel.CreatedObjects[i];

                bool isPresent = false;

                for (int b = 0; b < channel.Clients.Count; ++b)
                {
                    if (channel.Clients[b].ClientID == obj.playerID)
                    {
                        isPresent = true;
                        break;
                    }
                }

                // If the previous owner is not present, transfer ownership to the host
                if (!isPresent) obj.playerID = channel.Host.ClientID;

                ResponseCreateObjectEvent RCOEvent = new ResponseCreateObjectEvent();
                RCOEvent.channelID = channel.ChannelID;
                RCOEvent.creator = obj.playerID;
                RCOEvent.netID = obj.netID;
                RCOEvent.prefabID = obj.prefabID;
                RCOEvent.tempNetID = obj.netID;
                _source.SendEvent(RCOEvent);
            }

            // Send the list of objects that have been destroyed
            
            // The join process is now complete
            ResponseJoinChannelEvent rJoiningChannelEvent = new ResponseJoinChannelEvent();
            rJoiningChannelEvent.channelID = channel.ChannelID;
            rJoiningChannelEvent.message = "Success";
            _source.SendEvent(rJoiningChannelEvent);            

            // Inform the channel that a new Client is joining
            for (int i = 0; i < channel.Clients.Count; i++)
            {
                ResponseClientJoinedEvent rClientJoinedEvent = new ResponseClientJoinedEvent();
                rClientJoinedEvent.channelID = channel.ChannelID;
                rClientJoinedEvent.clientID = _source.ClientID;
                rClientJoinedEvent.playerName = _source.ClientName;
                channel.Clients[i].SendEvent(rClientJoinedEvent);
            }

            // Add this Client to the channel now that the joining process is complete
            channel.Clients.Add(_source);
        }

        internal void RequestLeaveChannel(Connection _source, RequestLeaveChannelEvent _evnt)
        {
            Channel channel = GetChannel(_evnt.channelID);

            if (channel == null)
                return;

            if (channel.RemovePlayer(_source.ClientID))
            {
                ResponseClientLeftChannel(_source, channel);
            }
        }

        internal void RequestCloseChannel(Connection _source, RequestCloseChannelEvent _evnt)
        {
            Channel channel = GetChannel(_evnt.channelID);

            if (channel == null)
                return;

            // Check if Client is admin!!
            if (channel.isOpen || !channel.Persistent)
            {
                channel.Close(true);
            }
        }

        private void LeaveAllChannels(Connection Client)
        {
            for (int i = 0; i < Client.Channels.Count; i++)
            {
                SendLeaveChannel(Client, Client.Channels[i]);
            }
        }

        private void SendLeaveChannel(Connection _conn, Channel _channel)
        {
            if (_channel == null) return;

            // Temporary buffer used in SendLeaveChannel below & Remove this Client from the channel
            List<NetworkId> m_temp = _channel.RemovePlayer(_conn);

            if (_conn.Channels.Remove(_channel))
            {
                // Are there other Clients left?
                if (_channel.Clients.Count > 0)
                {
                    // Inform the other Clients that the Client's objects should be destroyed
                    if (m_temp != null && m_temp.Count > 0)
                    {
                        ResponseDestroyObjectEvent RDOEvent = new ResponseDestroyObjectEvent();
                        RDOEvent.clientID = _conn.ClientID;
                        RDOEvent.channelID = _channel.ChannelID;
                        RDOEvent.objects = m_temp.ToArray();
                        SendToChannel(_channel, RDOEvent, _conn.ClientID);
                        lock (m_lock) { --m_entityCounter; }
                    }

                    // If this Client was the host, choose a new host
                    //if (_channel.host == null) SendSetHost(ch, (TcpClient)ch.Clients[0]);

                    // Inform everyone of this Client leaving the channel
                    ResponseClientLeftEvent RPLEvent = new ResponseClientLeftEvent();
                    RPLEvent.channelID = _channel.ChannelID;
                    RPLEvent.clientID = _conn.ClientID;
                    SendToChannel(_channel, RPLEvent, _conn.ClientID);

                }
                else if (!_channel.Persistent)
                {
                    // No other Clients left -- delete this channel
                    m_channels.Remove(_channel.ChannelID);
                }

                // Notify the Client that they have left the channel
                if (_conn.isConnected)
                {
                    ResponseLeaveChannelEvent RLCEvent = new ResponseLeaveChannelEvent();
                    RLCEvent.channelID = _channel.ChannelID;
                    _conn.SendEvent(RLCEvent);
                }
            }
        }

        private void SendToChannel(Channel _channel, Event _event, int _source)
        {
            _channel.SendEvent(_event, _source);
        }
        #endregion

        #region Objects        
        internal void RequestCreateObject(Connection _source, RequestCreateObjectEvent _evnt)
        {
            Channel channel = _source.GetChannel(_evnt.channelID);

            if (channel != null)
            {
                NetworkId id = channel.GetUniqueID();

                if (_evnt.type != 0)
                {
                    CreatedObject obj = new CreatedObject();
                    obj.playerID = _source.ClientID;
                    obj.netID = id;
                    obj.prefabID = _evnt.prefabID;
                    obj.type = _evnt.type;
                    channel.AddCreatedObject(obj);
                }

                ResponseCreateObjectEvent evnt = new ResponseCreateObjectEvent();
                evnt.channelID = channel.ChannelID;
                evnt.creator = _source.ClientID;
                evnt.netID = id;
                evnt.prefabID = _evnt.prefabID;
                evnt.tempNetID = _evnt.tempNetID;
                SendToChannel(channel, evnt, _source.ClientID);
                lock (m_lock) { ++m_entityCounter; }
            }
        }

        internal void RequestDestroyObject(Connection _source, ResponseDestroyObjectEvent _evnt)
        {

        }

        #endregion

        #region LoadLevelStatus
        internal void ResponseLoadLevelStatus(Connection _source, ResponseLoadLevelStatusEvent _evnt)
        {
            if(_evnt.status)
            {
                if(onLoadLevelRemoteDone != null)
                {
                    onLoadLevelRemoteDone(_source, _evnt.channelID, _evnt.levelName);
                }
            }
            else
            {
                if (onLoadLevelRemoteBegin != null)
                {
                    onLoadLevelRemoteBegin(_source, _evnt.channelID, _evnt.levelName);
                }
            }
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
                            AddConnection(evnt.Peer);
                            break;
                        case UDPEventType.Disconnect:
                            SkyLog.Debug("Disconnect");
                            ResponseClientDisconnected(RemoveConnection(evnt.Peer.GetIPEndPoint));
                            break;
                        case UDPEventType.Receive:
                            NetStats.ReceivePacket(evnt.Packet.Length);
                            ProcessPackets(new Packet(evnt.Packet), GetClient(evnt.Peer.GetIPEndPoint));
                            evnt.Packet.Dispose();
                            break;
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }
        #endregion

        #region Process Packets   
        private void ProcessPackets(Packet _packet, Connection _conn)
        {
            SkyLog.Debug("ProcessPackets");
            SkyLog.Debug("type: " + _packet.Header.type);

            switch (_packet.Header.type)
            {
                case PacketType.RequestClientID:
                    RequestClientID(_conn, _packet.Content.ReadEvent<RequestClientIDEvent>());
                    break;
                case PacketType.RequestJoinChannel:
                    RequestJoinChannel(_conn, _packet.Content.ReadEvent<RequestJoinChannelEvent>());
                    break;
                case PacketType.RequestLeaveChannel:
                    RequestLeaveChannel(_conn, _packet.Content.ReadEvent<RequestLeaveChannelEvent>());
                    break;
                case PacketType.RequestCloseChannel:
                    RequestCloseChannel(_conn, _packet.Content.ReadEvent<RequestCloseChannelEvent>());
                    break;
                case PacketType.RequestCreateObject:
                    RequestCreateObject(_conn, _packet.Content.ReadEvent<RequestCreateObjectEvent>());
                    break;
                case PacketType.ResponseLoadLevelStatus:
                    ResponseLoadLevelStatus(_conn, _packet.Content.ReadEvent<ResponseLoadLevelStatusEvent>());
                    break;
                case PacketType.RequestDestroyObject:
                    RequestDestroyObject(_conn, _packet.Content.ReadEvent<ResponseDestroyObjectEvent>());
                    break;
                default:
                    ProcessForwardPacket(_conn, _packet.Header);
                    break;
            }

            m_bitPacker.Flush();
        }
        #endregion

        #region Forward Packages
        internal void ProcessForwardPacket(Connection _source, Header _header)
        {
            switch (_header.targets)
            {
                case Targets.All:
                    ForwaredPacketAll(_source, _header, false);
                    break;
                case Targets.AllSaved:
                    ForwaredPacketAll(_source, _header, true);
                    break;
                case Targets.Others:
                    ForwaredPacketOthers(_source, _header, false);
                    break;
                case Targets.OthersSaved:
                    ForwaredPacketOthers(_source, _header, true);
                    break;
                case Targets.Server:
                    break;
                case Targets.Broadcast:
                    ForwaredPacketBroadcast(_header);
                    break;
                case Targets.Admin:
                    break;
                case Targets.Player:
                    ForwaredPacketPlayer(_source, _header);
                    break;
            }
        }

        private void ForwaredPacketAll(Connection _source, Header _header, bool _saved)
        {
            for (int i = 0; i < _source.Channels.Count; i++)
            {
                _source.Channels[i].SendWriter(_header, _saved);
            }
        }

        private void ForwaredPacketOthers(Connection _source, Header _header, bool _saved)
        {
            for (int i = 0; i < _source.Channels.Count; i++)
            {
                //if (_source.Channels[i].GetPlayer(_source.ClientID) != null) continue;
                _source.Channels[i].SendWriter(_header, _source, _saved);
            }
        }

        private void ForwaredPacketBroadcast(Header _header)
        {
            for (int i = 0; i < m_channels.Count; i++)
            {
                m_channels[i].SendWriter(_header, false);
            }
        }        

        private void ForwaredPacketPlayer(Connection _source, Header _header)
        {
            _source.Channels[0].SendWriter(_header, false);
        }
        #endregion
    }
}
