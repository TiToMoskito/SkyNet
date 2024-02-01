using System;
using System.Net;
using System.Text;

namespace SkyNet
{
    public struct UDPPeer
    {
        private IntPtr nativePeer;
        private uint nativeID;

        internal IntPtr NativeData
        {
            get
            {
                return nativePeer;
            }

            set
            {
                nativePeer = value;
            }
        }

        public UDPPeer(IntPtr peer)
        {
            nativePeer = peer;
            nativeID = nativePeer != IntPtr.Zero ? UDPNative.enet_peer_get_id(nativePeer) : 0;
        }

        public bool IsSet
        {
            get
            {
                return nativePeer != IntPtr.Zero;
            }
        }

        public uint ID
        {
            get
            {
                return nativeID;
            }
            set
            {
                nativeID = value;
            }
        }

        public IPEndPoint GetIPEndPoint
        {
            get
            {
                return new IPEndPoint(IPAddress.Parse(IP), Port);
            }
        }

        public string IP
        {
            get
            {
                CheckCreated();

                byte[] ip = ArrayPool.GetByteBuffer();

                if (UDPNative.enet_peer_get_ip(nativePeer, ip, (IntPtr)ip.Length) == 0)
                {
                    if (Encoding.ASCII.GetString(ip).Remove(7) != "::ffff:")
                        return Encoding.ASCII.GetString(ip, 0, ip.StringLength());
                    else
                        return Encoding.ASCII.GetString(ip, 0, ip.StringLength()).Substring(7);
                }
                else
                {
                    return String.Empty;
                }
            }
        }

        public ushort Port
        {
            get
            {
                CheckCreated();

                return UDPNative.enet_peer_get_port(nativePeer);
            }
        }

        public uint MTU
        {
            get
            {
                CheckCreated();

                return UDPNative.enet_peer_get_mtu(nativePeer);
            }
        }

        public PeerState State
        {
            get
            {
                return nativePeer == IntPtr.Zero ? PeerState.Uninitialized : UDPNative.enet_peer_get_state(nativePeer);
            }
        }

        public uint RoundTripTime
        {
            get
            {
                CheckCreated();

                return UDPNative.enet_peer_get_rtt(nativePeer);
            }
        }

        public uint LastSendTime
        {
            get
            {
                CheckCreated();

                return UDPNative.enet_peer_get_lastsendtime(nativePeer);
            }
        }

        public uint LastReceiveTime
        {
            get
            {
                CheckCreated();

                return UDPNative.enet_peer_get_lastreceivetime(nativePeer);
            }
        }

        public ulong PacketsSent
        {
            get
            {
                CheckCreated();

                return UDPNative.enet_peer_get_packets_sent(nativePeer);
            }
        }

        public uint PacketsLost
        {
            get
            {
                CheckCreated();

                return UDPNative.enet_peer_get_packets_lost(nativePeer);
            }
        }

        public ulong BytesSent
        {
            get
            {
                CheckCreated();

                return UDPNative.enet_peer_get_bytes_sent(nativePeer);
            }
        }

        public ulong BytesReceived
        {
            get
            {
                CheckCreated();

                return UDPNative.enet_peer_get_bytes_received(nativePeer);
            }
        }

        public IntPtr Data
        {
            get
            {
                CheckCreated();

                return UDPNative.enet_peer_get_data(nativePeer);
            }

            set
            {
                CheckCreated();

                UDPNative.enet_peer_set_data(nativePeer, value);
            }
        }

        internal void CheckCreated()
        {
            if (nativePeer == IntPtr.Zero)
                throw new InvalidOperationException("Peer not created");
        }

        public void ConfigureThrottle(uint interval, uint acceleration, uint deceleration)
        {
            CheckCreated();

            UDPNative.enet_peer_throttle_configure(nativePeer, interval, acceleration, deceleration);
        }

        public bool Send(byte channelID, ref UDPPacket packet)
        {
            CheckCreated();

            packet.CheckCreated();

            return UDPNative.enet_peer_send(nativePeer, channelID, packet.NativeData) == 0;
        }

        public void Ping()
        {
            CheckCreated();

            UDPNative.enet_peer_ping(nativePeer);
        }

        public void PingInterval(uint interval)
        {
            CheckCreated();

            UDPNative.enet_peer_ping_interval(nativePeer, interval);
        }

        public void Timeout(uint timeoutLimit, uint timeoutMinimum, uint timeoutMaximum)
        {
            CheckCreated();

            UDPNative.enet_peer_timeout(nativePeer, timeoutLimit, timeoutMinimum, timeoutMaximum);
        }

        public void Disconnect(uint data)
        {
            CheckCreated();

            UDPNative.enet_peer_disconnect(nativePeer, data);
        }

        public void DisconnectNow(uint data)
        {
            CheckCreated();

            UDPNative.enet_peer_disconnect_now(nativePeer, data);
        }

        public void DisconnectLater(uint data)
        {
            CheckCreated();

            UDPNative.enet_peer_disconnect_later(nativePeer, data);
        }

        public void Reset()
        {
            CheckCreated();

            UDPNative.enet_peer_reset(nativePeer);
        }
    }

    public enum PeerState {
		Uninitialized = -1,
		Disconnected = 0,
		Connecting = 1,
		AcknowledgingConnect = 2,
		ConnectionPending = 3,
		ConnectionSucceeded = 4,
		Connected = 5,
		DisconnectLater = 6,
		Disconnecting = 7,
		AcknowledgingDisconnect = 8,
		Zombie = 9
	}
}