using System;

namespace SkyNet
{
    public class UDPHost : IDisposable
    {
        private IntPtr nativeHost;

        internal IntPtr NativeData
        {
            get
            {
                return nativeHost;
            }

            set
            {
                nativeHost = value;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (nativeHost != IntPtr.Zero)
            {
                UDPNative.enet_host_destroy(nativeHost);
                nativeHost = IntPtr.Zero;
            }
        }

        ~UDPHost()
        {
            Dispose(false);
        }

        public bool IsSet
        {
            get
            {
                return nativeHost != IntPtr.Zero;
            }
        }

        public uint PeersCount
        {
            get
            {
                CheckCreated();

                return UDPNative.enet_host_get_peers_count(nativeHost);
            }
        }

        public uint PacketsSent
        {
            get
            {
                CheckCreated();

                return UDPNative.enet_host_get_packets_sent(nativeHost);
            }
        }

        public uint PacketsReceived
        {
            get
            {
                CheckCreated();

                return UDPNative.enet_host_get_packets_received(nativeHost);
            }
        }

        public uint BytesSent
        {
            get
            {
                CheckCreated();

                return UDPNative.enet_host_get_bytes_sent(nativeHost);
            }
        }

        public uint BytesReceived
        {
            get
            {
                CheckCreated();

                return UDPNative.enet_host_get_bytes_received(nativeHost);
            }
        }

        internal void CheckCreated()
        {
            if (nativeHost == IntPtr.Zero)
                throw new InvalidOperationException("Host not created");
        }

        private void CheckChannelLimit(int channelLimit)
        {
            if (channelLimit < 0 || channelLimit > UDPLibrary.maxChannelCount)
                throw new ArgumentOutOfRangeException("channelLimit");
        }

        public void Create()
        {
            Create(null, 1, 0);
        }

        public void Create(UDPAddress? address, int peerLimit)
        {
            Create(address, peerLimit, 0);
        }

        public void Create(UDPAddress? address, int peerLimit, int channelLimit)
        {
            Create(address, peerLimit, channelLimit, 0, 0);
        }

        public void Create(int peerLimit, int channelLimit)
        {
            Create(null, peerLimit, channelLimit, 0, 0);
        }

        public void Create(int peerLimit, int channelLimit, uint incomingBandwidth, uint outgoingBandwidth)
        {
            Create(null, peerLimit, channelLimit, incomingBandwidth, outgoingBandwidth);
        }

        public void Create(UDPAddress? address, int peerLimit, int channelLimit, uint incomingBandwidth, uint outgoingBandwidth)
        {
            if (nativeHost != IntPtr.Zero)
                throw new InvalidOperationException("Host already created");

            if (peerLimit < 0 || peerLimit > UDPLibrary.maxPeers)
                throw new ArgumentOutOfRangeException("peerLimit");

            CheckChannelLimit(channelLimit);

            if (address != null)
            {
                var nativeAddress = address.Value.NativeData;

                nativeHost = UDPNative.enet_host_create(ref nativeAddress, (IntPtr)peerLimit, (IntPtr)channelLimit, incomingBandwidth, outgoingBandwidth);
            }
            else
            {
                nativeHost = UDPNative.enet_host_create(IntPtr.Zero, (IntPtr)peerLimit, (IntPtr)channelLimit, incomingBandwidth, outgoingBandwidth);
            }

            if (nativeHost == IntPtr.Zero)
                throw new InvalidOperationException("Host creation call failed");
        }

        public void EnableCompression()
        {
            CheckCreated();

            UDPNative.enet_host_enable_compression(nativeHost);
        }

        public void PreventConnections(bool state)
        {
            CheckCreated();

            UDPNative.enet_host_prevent_connections(nativeHost, (byte)(state ? 1 : 0));
        }

        public void Broadcast(byte channelID, ref UDPPacket packet)
        {
            CheckCreated();

            packet.CheckCreated();
            UDPNative.enet_host_broadcast(nativeHost, channelID, packet.NativeData);
            packet.NativeData = IntPtr.Zero;
        }

        public void Broadcast(byte channelID, ref UDPPacket packet, ref UDPPeer[] peers)
        {
            CheckCreated();

            packet.CheckCreated();

            if (peers.Length > 0)
            {
                IntPtr[] nativePeers = ArrayPool.GetPointerBuffer();
                int nativeCount = 0;

                for (int i = 0; i < peers.Length; i++)
                {
                    if (peers[i].NativeData != IntPtr.Zero)
                    {
                        nativePeers[nativeCount] = peers[i].NativeData;
                        nativeCount++;
                    }
                }

                UDPNative.enet_host_broadcast_selective(nativeHost, channelID, packet.NativeData, nativePeers, (IntPtr)nativeCount);
            }

            packet.NativeData = IntPtr.Zero;
        }

        public int CheckEvents(out UDPEvent @event)
        {
            CheckCreated();

            ENetEvent nativeEvent;

            var result = UDPNative.enet_host_check_events(nativeHost, out nativeEvent);

            if (result <= 0)
            {
                @event = new UDPEvent();

                return result;
            }

            @event = new UDPEvent(nativeEvent);

            return result;
        }

        public UDPPeer Connect(UDPAddress address)
        {
            return Connect(address, 0, 0);
        }

        public UDPPeer Connect(UDPAddress address, int channelLimit)
        {
            return Connect(address, channelLimit, 0);
        }

        public UDPPeer Connect(UDPAddress address, int channelLimit, uint data)
        {
            CheckCreated();
            CheckChannelLimit(channelLimit);

            var nativeAddress = address.NativeData;
            var peer = new UDPPeer(UDPNative.enet_host_connect(nativeHost, ref nativeAddress, (IntPtr)channelLimit, data));

            if (peer.NativeData == IntPtr.Zero)
                throw new InvalidOperationException("Host connect call failed");

            return peer;
        }

        public int Service(int timeout, out UDPEvent @event)
        {
            if (timeout < 0)
                throw new ArgumentOutOfRangeException("timeout");

            CheckCreated();

            ENetEvent nativeEvent;

            var result = UDPNative.enet_host_service(nativeHost, out nativeEvent, (uint)timeout);

            if (result <= 0)
            {
                @event = new UDPEvent();

                return result;
            }

            @event = new UDPEvent(nativeEvent);

            return result;
        }

        public void SetBandwidthLimit(uint incomingBandwidth, uint outgoingBandwidth)
        {
            CheckCreated();

            UDPNative.enet_host_bandwidth_limit(nativeHost, incomingBandwidth, outgoingBandwidth);
        }

        public void SetChannelLimit(int channelLimit)
        {
            CheckCreated();
            CheckChannelLimit(channelLimit);

            UDPNative.enet_host_channel_limit(nativeHost, (IntPtr)channelLimit);
        }

        public void Flush()
        {
            CheckCreated();

            UDPNative.enet_host_flush(nativeHost);
        }
    }
}