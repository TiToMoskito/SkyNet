using System;
using System.Collections.Generic;
using System.Net;

namespace SkyNet
{
    public class Connection
    {
        #region Private Variables
        private UDPPeer m_peer;
        private NetBuffer m_bitPacker;

        private int m_id = 1;
        private string m_name = "Client";

        private List<Channel> m_channels = new List<Channel>();
        #endregion

        #region Public Properties
        public int ClientID { get { return (int)m_peer.ID; } set { m_peer.ID = (uint)value; } }
        public string ClientName { get { return m_name; } internal set { m_name = value; } }
        public IPEndPoint IPAddress { get { return m_peer.GetIPEndPoint; } }
        public object ClientData { get { return m_peer.Data; } set { m_peer.Data = (IntPtr)value; } }
        public bool isConnected { get { return m_peer.State == PeerState.Connected; } }
        public uint RTT { get { return m_peer.RoundTripTime; } }
        public List<Channel> Channels { get { return m_channels; } }
        #endregion

        #region Connection
        public Connection(UDPPeer _peer)
        {
            m_peer = _peer;
            m_bitPacker = new NetBuffer();
        }

        public Connection(int _id, string _name)
        {
            m_id = _id;
            m_name = _name;
        }

        public void Disconnect()
        {
            m_peer.DisconnectNow(0);
            Reset();
        }

        private void Reset()
        {
            m_peer.Reset();
        }

        public void Ping()
        {
            m_peer.Ping();
        }

        #endregion

        #region Send Packets
        internal bool Send(int _channelID, UDPPacket _packet)
        {
            NetStats.SentPacket(_packet.Length);
            return m_peer.Send((byte)_channelID, ref _packet);
        }

        internal bool SendEvent(Event _event, PacketFlags _flag, Targets _targets)
        {
            _event.Pack(m_bitPacker);

            Packet p = new Packet(new Header(_flag, _targets, PacketType.Event, _event.Data.TypeId));
            p.Create(m_bitPacker);
            return p.SendToTransport(0, this);
        }

        internal bool SendEvent(Event _event, PacketFlags _flag, Connection _target)
        {
            _event.Pack(m_bitPacker);

            Packet p = new Packet(new Header(_flag, Targets.Player, PacketType.Event, (uint)_target.ClientID, _event.Data.TypeId));
            p.Create(m_bitPacker);
            return p.SendToTransport(0, this);
        }

        /// <summary>
        /// Send a internal Event
        /// </summary>
        internal bool SendEvent(Event _event)
        {
            m_bitPacker = new NetBuffer();
            _event.Pack(m_bitPacker);

            Packet p = new Packet(new Header(PacketFlags.Reliable, Targets.Internal, _event.m_packetType, _event.Data.TypeId));
            p.Create(m_bitPacker);
            return p.SendToTransport(0, this);
        }       

        internal bool SendState(byte[] _buffer, NetworkId _netId, PacketFlags _flag, Targets _targets)
        {
            Packet p = new Packet(new Header(_flag, _targets, PacketType.State, _netId));
            p.Create(_buffer);
            return p.SendToTransport(0, this);
        }

        //public void SendRaw(byte[] data)
        //{
        //    for (int i = 0; i < m_channels.Count; i++)
        //    {
        //        m_channels[i].SendBytes(this, data, data.Length, PacketFlags.UnreliableFragment);
        //    }
        //}
        #endregion

        #region Channel      
        public void AddChannel(Channel _channel)
        {
            m_channels.Add(_channel);
        }

        public void RemoveChannel(int channelID)
        {
            for (int i = 0; i < m_channels.Count; i++)
            {
                if (m_channels[i].ChannelID == channelID)
                {
                    m_channels.Remove(m_channels[i]);
                    break;
                }
            }
        }

        public bool LeaveChannel(int channelID)
        {
            if (IsInChannel(channelID))
            {
                RequestLeaveChannelEvent evnt = new RequestLeaveChannelEvent();
                evnt.channelID = channelID;
                SendEvent(evnt);
                RemoveChannel(channelID);
                return true;
            }
            return false;
        }

        public void LeaveAllChannels()
        {
            for (int i = 0; i < m_channels.Count; i++)
            {
                RequestLeaveChannelEvent evnt = new RequestLeaveChannelEvent();
                evnt.channelID = m_channels[i].ChannelID;
                SendEvent(evnt);
                m_channels.Remove(m_channels[i]);
            }
        }

        /// <summary>
        /// Return the specified channel if the player is currently within it, null otherwise.
        /// </summary>
        public Channel GetChannel(int channelID)
        {
            for (int i = 0; i < m_channels.Count; ++i)
                if (m_channels[i].ChannelID == channelID) return m_channels[i];
            return null;
        }

        public bool IsInChannel(int channelID)
        {
            for (int i = 0; i < m_channels.Count; ++i)
            {
                Channel ch = m_channels[i];
                if (ch.ChannelID == channelID) return true;
            }
            return false;
        }

        #endregion
    }
}
