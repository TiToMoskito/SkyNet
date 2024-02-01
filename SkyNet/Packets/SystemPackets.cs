using System;

namespace SkyNet
{
    
    public class RequestJoinChannelEvent : Event
    {
        public int channelID { get; set; }
        public string password { get; set; }
        public string levelName { get; set; }
        public uint playerLimit { get; set; }
        public bool persistent { get; set; }

        public RequestJoinChannelEvent() : base(InternEvent_Data.Instance)
        {
            m_packetType = PacketType.RequestJoinChannel;
        }

        internal override void Unpack(NetBuffer _packer)
        {
            channelID = _packer.ReadInt32();
            password = _packer.ReadString();
            levelName = _packer.ReadString();
            playerLimit = _packer.ReadUInt32();
            persistent = _packer.ReadBoolean();
        }

        internal override void Pack(NetBuffer _packer)
        {
            _packer.Write(channelID);
            _packer.Write(password);
            _packer.Write(levelName);
            _packer.Write(playerLimit);
            _packer.Write(persistent);
        }
    }
    
    public class ResponseJoinChannelEvent : Event
    {        
        public int channelID { get; set; }        
        public string message { get; set; }

        public ResponseJoinChannelEvent() : base(InternEvent_Data.Instance)
        {
            m_packetType = PacketType.ResponseJoinChannel;
        }

        internal override void Unpack(NetBuffer _packer)
        {
            channelID = _packer.ReadInt32();
            message = _packer.ReadString();
        }

        internal override void Pack(NetBuffer _packer)
        {
            _packer.Write(channelID);
            _packer.Write(message);
        }
    }

    
    public class ResponseJoiningChannelEvent : Event
    {        
        public int channelID { get; set; }        
        public int[] connections { get; set; }

        public ResponseJoiningChannelEvent() : base(InternEvent_Data.Instance)
        {
            m_packetType = PacketType.ResponseJoiningChannel;
        }

        internal override void Unpack(NetBuffer _packer)
        {
            channelID = _packer.ReadInt32();

            uint numPlayers = _packer.ReadUInt32();
            connections = new int[numPlayers];

            for (uint i = 0; i < numPlayers; i++)
            {
                connections[i] = _packer.ReadInt32();
            }
        }

        internal override void Pack(NetBuffer _packer)
        {
            _packer.Write(channelID);

            if (connections == null || connections.Length == 0)
            {
                _packer.Write(0);
            }
            else
            {
                _packer.Write((uint)connections.Length);
                for (int i = 0; i < connections.Length; i++)
                {
                    _packer.Write(connections[i]);
                }
            }
        }
    }
    
    public class RequestLeaveChannelEvent : Event
    {        
        public int channelID { get; set; }

        public RequestLeaveChannelEvent() : base(InternEvent_Data.Instance)
        {
            m_packetType = PacketType.RequestLeaveChannel;
        }

        internal override void Unpack(NetBuffer _packer)
        {
            channelID = _packer.ReadInt32();
        }

        internal override void Pack(NetBuffer _packer)
        {
            _packer.Write(channelID);
        }
    }
    
    public class ResponseLeaveChannelEvent : Event
    {        
        public int channelID { get; set; }

        public ResponseLeaveChannelEvent() : base(InternEvent_Data.Instance)
        {
            m_packetType = PacketType.ResponseLeaveChannel;
        }

        internal override void Unpack(NetBuffer _packer)
        {
            channelID = _packer.ReadInt32();
        }

        internal override void Pack(NetBuffer _packer)
        {
            _packer.Write(channelID);
        }
    }
    
    public class RequestCloseChannelEvent : Event
    {        
        public int channelID { get; set; }

        public RequestCloseChannelEvent() : base(InternEvent_Data.Instance)
        {
            m_packetType = PacketType.RequestCloseChannel;
        }

        internal override void Unpack(NetBuffer _packer)
        {
            channelID = _packer.ReadInt32();
        }

        internal override void Pack(NetBuffer _packer)
        {
            _packer.Write(channelID);
        }
    }
    
    public class ResponseLoadLevelEvent : Event
    {        
        public int channelID { get; set; }        
        public string levelName { get; set; }

        public ResponseLoadLevelEvent() : base(InternEvent_Data.Instance)
        {
            m_packetType = PacketType.ResponseLoadLevel;
        }

        internal override void Unpack(NetBuffer _packer)
        {
            channelID = _packer.ReadInt32();
            levelName = _packer.ReadString();
        }

        internal override void Pack(NetBuffer _packer)
        {
            _packer.Write(channelID);
            _packer.Write(levelName);
        }
    }

    public class ResponseLoadLevelStatusEvent : Event
    {
        public int clientID { get; set; }
        public bool status { get; set; }
        public int channelID { get; set; }
        public string levelName { get; set; }

        public ResponseLoadLevelStatusEvent() : base(InternEvent_Data.Instance)
        {
            m_packetType = PacketType.ResponseLoadLevelStatus;
        }

        internal override void Unpack(NetBuffer _packer)
        {
            clientID = _packer.ReadInt32();
            status = _packer.ReadBoolean();
            channelID = _packer.ReadInt32();
            levelName = _packer.ReadString();
        }

        internal override void Pack(NetBuffer _packer)
        {
            _packer.Write(clientID);
            _packer.Write(status);
            _packer.Write(channelID);
            _packer.Write(levelName);
        }
    }

    public class ResponseClientJoinedEvent : Event
    {        
        public int channelID { get; set; }        
        public int clientID { get; set; }        
        public string playerName { get; set; }

        public ResponseClientJoinedEvent() : base(InternEvent_Data.Instance)
        {
            m_packetType = PacketType.ResponseClientJoined;
        }

        internal override void Unpack(NetBuffer _packer)
        {
            channelID = _packer.ReadInt32();
            clientID = _packer.ReadInt32();
            playerName = _packer.ReadString();
        }

        internal override void Pack(NetBuffer _packer)
        {
            _packer.Write(channelID);
            _packer.Write(clientID);
            _packer.Write(playerName);
        }
    }
    
    public class ResponseClientLeftEvent : Event
    {        
        public int channelID { get; set; }        
        public int clientID { get; set; }

        public ResponseClientLeftEvent() : base(InternEvent_Data.Instance)
        {
            m_packetType = PacketType.ResponseClientLeft;
        }

        internal override void Unpack(NetBuffer _packer)
        {
            channelID = _packer.ReadInt32();
            clientID = _packer.ReadInt32();
        }

        internal override void Pack(NetBuffer _packer)
        {
            _packer.Write(channelID);
            _packer.Write(clientID);
        }
    }

    public class RequestCreateObjectEvent : Event
    {
        public int channelID { get; set; }        
        public byte type { get; set; }
        public PrefabId prefabID { get; set; }
        public NetworkId tempNetID { get; set; }

        public RequestCreateObjectEvent() : base(InternEvent_Data.Instance)
        {
            m_packetType = PacketType.RequestCreateObject;
        }

        internal override void Unpack(NetBuffer _packer)
        {
            channelID = _packer.ReadInt32();
            type = _packer.ReadByte();
            prefabID = _packer.ReadPrefabId();
            tempNetID = _packer.ReadNetworkId();
        }

        internal override void Pack(NetBuffer _packer)
        {
            _packer.Write(channelID);
            _packer.Write(type);
            _packer.WritePrefabId(prefabID);
            _packer.WriteNetworkId(tempNetID);
        }
    }

    public class ResponseCreateObjectEvent : Event
    {
        public int channelID { get; set; }
        public int creator { get; set; }
        public NetworkId netID { get; set; }
        public PrefabId prefabID { get; set; }
        public NetworkId tempNetID { get; set; }

        public ResponseCreateObjectEvent() : base(InternEvent_Data.Instance)
        {
            m_packetType = PacketType.ResponseCreateObject;
        }

        internal override void Unpack(NetBuffer _packer)
        {
            channelID = _packer.ReadInt32();
            creator = _packer.ReadInt32();
            netID = _packer.ReadNetworkId();
            prefabID = _packer.ReadPrefabId();
            tempNetID = _packer.ReadNetworkId();
        }

        internal override void Pack(NetBuffer _packer)
        {
            _packer.Write(channelID);
            _packer.Write(creator);
            _packer.WriteNetworkId(netID);
            _packer.WritePrefabId(prefabID);
            _packer.WriteNetworkId(tempNetID);
        }
    }

    public class RequestClientIDEvent : Event
    {
        public uint version { get; set; }
        public string name { get; set; }

        public RequestClientIDEvent() : base(InternEvent_Data.Instance)
        {
            m_packetType = PacketType.RequestClientID;
            Data.TypeId = new TypeId(1);
        }

        internal override void Unpack(NetBuffer _packer)
        {
            version = _packer.ReadUInt32();
            name = _packer.ReadString();
        }

        internal override void Pack(NetBuffer _packer)
        {
            SkyLog.Debug("PACK");

            _packer.Write(version);
            _packer.Write(name);

            SkyLog.Debug("PACKED: {0} {1}", _packer.Length, System.Text.Encoding.UTF8.GetString(_packer.ToArray()));
        }
    }

    public class ResponseClientIDEvent : Event
    {
        public int clientID { get; set; }

        public ResponseClientIDEvent() : base(InternEvent_Data.Instance)
        {
            m_packetType = PacketType.ResponseClientID;
        }

        internal override void Unpack(NetBuffer _packer)
        {
            clientID = _packer.ReadInt32();
        }

        internal override void Pack(NetBuffer _packer)
        {
            _packer.Write(clientID);
        }
    }

    public class ResponseDestroyObjectEvent : Event
    {
        public int clientID { get; set; }
        public int channelID { get; set; }
        public NetworkId[] objects { get; set; }

        public ResponseDestroyObjectEvent() : base(InternEvent_Data.Instance)
        {
            m_packetType = PacketType.ResponseDestroyObject;
        }

        internal override void Unpack(NetBuffer _packer)
        {
            clientID = _packer.ReadInt32();
            channelID = _packer.ReadInt32();

            uint numObjects = _packer.ReadUInt32();
            objects = new NetworkId[numObjects];

            for (uint i = 0; i < numObjects; i++)
            {
                objects[i] = _packer.ReadNetworkId();
            }
        }

        internal override void Pack(NetBuffer _packer)
        {
            _packer.Write(clientID);
            _packer.Write(channelID);

            if (objects == null || objects.Length == 0)
            {
                _packer.Write(0);
            }
            else
            {
                _packer.Write((uint)objects.Length);
                for (int i = 0; i < objects.Length; i++)
                {
                    _packer.WriteNetworkId(objects[i]);
                }
            }
        }
    }

    internal class InternEvent_Data : Event_Data
    {
        internal static InternEvent_Data Instance = new InternEvent_Data();

        static InternEvent_Data()
        {
            Instance.InitData();
        }

        internal void InitData()
        {
            TypeId = new TypeId(0);
        }
    }
}