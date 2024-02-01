using System.Collections.Generic;

namespace SkyNet
{
    public class Channel
    {
        #region Public Variables        
        
        #endregion

        #region Private Variables
        private int m_channelID;
        private string m_password = "";
        private string m_level = "";
        private bool m_persistent = false;
        private bool m_closed = false;
        private bool m_isLocked = false;
        private uint m_playerLimit = 65535;
        private uint m_objectCounter = 0xFFFFFF;        
        private Connection m_host;
        private List<Connection> m_clients = new List<Connection>();
        private List<CreatedObject> m_created = new List<CreatedObject>();
        private List<Packet> m_savedPackets = new List<Packet>();
        private Dictionary<NetworkId, CreatedObject> m_createdObjects = new Dictionary<NetworkId, CreatedObject>();
        #endregion

        #region Public Properties
        public bool isOpen { get { return !m_closed && m_clients.Count < m_playerLimit; } }
        public List<Connection> Clients { get { return m_clients; } }
        public int ChannelID { get { return m_channelID; } }
        public string Password { get { return m_password; } }
        public string Level { get { return m_level; } }
        public List<CreatedObject> CreatedObjects { get { return m_created; } }
        public Connection Host { get { return m_host; } }
        public bool Persistent { get { return m_persistent; } }        
        #endregion

        #region Channel
        public Channel(int _channelID)
        {
            m_channelID = _channelID;            
        }

        public void Setup(string _password, bool _persistent, string _level, uint _playerLimit)
        {
            m_password = _password;
            m_persistent = _persistent;
            m_level = _level;
            m_playerLimit = _playerLimit;
        }

        public void Close(bool _close)
        {
            m_closed = _close;
        }
        #endregion

        #region NetObject
        /// <summary>
        /// Helper function that returns a new unique ID that's not currently used by any object.
        /// </summary>
        public NetworkId GetUniqueID()
        {
            for (; ; )
            {
                uint uniqueID = --m_objectCounter;

                // 1-32767 is reserved for existing scene objects.
                // 32768 - 16777215 is for dynamically created objects.
                if (uniqueID < 32768)
                {
                    m_objectCounter = 0xFFFFFF;
                    uniqueID = 0xFFFFFF;
                }

                NetworkId networkId = new NetworkId(uniqueID);

                // Ensure that this uniqueID is not already in use
                if (!m_createdObjects.ContainsKey(networkId))
                    return networkId;
            }
        }

        /// <summary>
        /// Add a new created object to the list. This object's ID must always be above 32767.
        /// </summary>
        public void AddCreatedObject(CreatedObject obj)
        {
            m_created.Add(obj);
            m_createdObjects[obj.netID] = obj;
        }
        #endregion

        #region Player
        public int[] GetAllPlayers()
        {
            int[] player = new int[m_clients.Count];

            for (int i = 0; i < m_clients.Count; ++i)
            {
                player[i] = m_clients[i].ClientID;
            }

            return player;
        }

        /// <summary>
		/// Return a player with the specified ID.
		/// </summary>
        public Connection GetPlayer(int pid)
        {
            for (int i = 0; i < m_clients.Count; ++i)
            {
                Connection p = m_clients[i];
                if (p.ClientID == pid) return p;
            }
            return null;
        }

        /// <summary>
        /// Remove the player with the specified ID.
        /// </summary>

        public void AddPlayer(Connection _conn)
        {
            m_clients.Add(_conn);
        }

        /// <summary>
        /// Remove the player with the specified ID.
        /// </summary>

        public bool RemovePlayer(int pid)
        {
            for (int i = 0; i < m_clients.Count; ++i)
            {
                Connection p = m_clients[i];
                if (p.ClientID == pid)
                {
                    m_clients.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Remove the specified player from the channel.
        /// </summary>
        public List<NetworkId> RemovePlayer(Connection p)
        {
            List<NetworkId> destroyedObjects = new List<NetworkId>();

            if (m_clients.Remove(p))
            {
                // When the host leaves, clear the host (it gets changed in SendLeaveChannel)
                if (p == m_host) m_host = null;

                // Remove all of the non-persistent objects that were created by this player
                for (int i = 0; i < m_created.Count;)
                {
                    var obj = m_created[i];

                    if (obj.playerID == p.ClientID)
                    {
                        if (obj.type == 2)
                        {
                            NetworkId objID = obj.netID;
                            m_created.RemoveAt(i);
                            destroyedObjects.Add(objID);
                            m_createdObjects.Remove(objID);
                            continue;
                        }

                        // The same operation happens on the client as well
                        if (m_clients.Count != 0) obj.playerID = m_clients[0].ClientID;
                    }
                    ++i;
                }

                // Close the channel if it wasn't persistent
                if ((!m_persistent || m_playerLimit < 1) && m_clients.Count == 0)
                {
                    m_closed = true;
                }
            }

            return destroyedObjects;
        }
        #endregion

        #region Handle Client Packets
        #endregion

        #region Handle Server Packets
        #endregion

        #region Internal Network Packets

        internal void SendEvent(Event _event, int _source)
        {
            for (int i = 0; i < m_clients.Count; i++)
            {
                SendWriter(m_clients[i], new Header(PacketFlags.Reliable, Targets.Internal, PacketType.Event, (ushort)_source, _event.Data.TypeId), false);
            }
        }

        internal void SendWriter(Header _header, bool _saved)
        {
            for (int i = 0; i < m_clients.Count; i++)
            {
                SendWriter(m_clients[i], _header, _saved);
            }
        }

        internal void SendWriter(Header _header, Connection _exclude, bool _saved)
        {
            for (int i = 0; i < m_clients.Count; i++)
            {
                if (m_clients[i] != _exclude)
                    SendWriter(m_clients[i], _header, _saved);
            }
        }

        internal bool SendWriter(Connection _conn, Header _header, bool _saved)
        {
            Packet p = new Packet(_header);
            p.Create(_header.Package);
            if (_saved)
                m_savedPackets.Add(p);

            return p.SendToTransport((byte)ChannelID, _conn);
        }

        #endregion
    }
}
