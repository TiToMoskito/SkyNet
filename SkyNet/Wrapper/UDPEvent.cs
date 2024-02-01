using System;
using System.Runtime.InteropServices;

namespace SkyNet
{
    public struct UDPEvent
    {
        private ENetEvent nativeEvent;

        internal ENetEvent NativeData
        {
            get
            {
                return nativeEvent;
            }

            set
            {
                nativeEvent = value;
            }
        }

        public UDPEvent(ENetEvent @event)
        {
            nativeEvent = @event;
        }

        public UDPEventType Type
        {
            get
            {
                return nativeEvent.type;
            }
        }

        public UDPPeer Peer
        {
            get
            {
                return new UDPPeer(nativeEvent.peer);
            }
        }

        public byte ChannelID
        {
            get
            {
                return nativeEvent.channelID;
            }
        }

        public uint Data
        {
            get
            {
                return nativeEvent.data;
            }
        }

        public UDPPacket Packet
        {
            get
            {
                return new UDPPacket(nativeEvent.packet);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ENetEvent
    {
        public UDPEventType type;
        public IntPtr peer;
        public byte channelID;
        public uint data;
        public IntPtr packet;
    }

    public enum UDPEventType
    {
        None = 0,
        Connect = 1,
        Disconnect = 2,
        Receive = 3,
        Timeout = 4
    }
}