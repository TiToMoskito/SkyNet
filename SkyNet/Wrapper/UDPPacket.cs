using System;
using System.Runtime.InteropServices;

namespace SkyNet
{
    public struct UDPPacket : IDisposable
    {
        private IntPtr nativePacket;

        internal IntPtr NativeData
        {
            get
            {
                return nativePacket;
            }

            set
            {
                nativePacket = value;
            }
        }

        public UDPPacket(IntPtr packet)
        {
            nativePacket = packet;
        }

        public void Dispose()
        {
            if (nativePacket != IntPtr.Zero)
            {
                UDPNative.enet_packet_dispose(nativePacket);
                nativePacket = IntPtr.Zero;
            }
        }

        public bool IsSet
        {
            get
            {
                return nativePacket != IntPtr.Zero;
            }
        }

        public IntPtr Data
        {
            get
            {
                CheckCreated();

                return UDPNative.enet_packet_get_data(nativePacket);
            }
        }

        public int Length
        {
            get
            {
                CheckCreated();

                return UDPNative.enet_packet_get_length(nativePacket);
            }
        }

        public bool HasReferences
        {
            get
            {
                CheckCreated();

                return UDPNative.enet_packet_check_references(nativePacket) != 0;
            }
        }

        internal void CheckCreated()
        {
            if (nativePacket == IntPtr.Zero)
                throw new InvalidOperationException("Packet not created");
        }

        public void SetFreeCallback(PacketFreeCallback callback)
        {
            CheckCreated();

            UDPNative.enet_packet_set_free_callback(nativePacket, Marshal.GetFunctionPointerForDelegate(callback));
        }

        public void Create(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            Create(data, data.Length);
        }

        public void Create(byte[] data, int length)
        {
            Create(data, length, PacketFlags.None);
        }

        public void Create(byte[] data, PacketFlags flags)
        {
            Create(data, data.Length, flags);
        }

        public void Create(byte[] data, int length, PacketFlags flags)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            if (length < 0 || length > data.Length)
                throw new ArgumentOutOfRangeException();

            nativePacket = UDPNative.enet_packet_create(data, (IntPtr)length, flags);
        }

        public void Create(IntPtr data, int length, PacketFlags flags)
        {
            if (data == IntPtr.Zero)
                throw new ArgumentNullException("data");

            if (length < 0)
                throw new ArgumentOutOfRangeException();

            nativePacket = UDPNative.enet_packet_create(data, (IntPtr)length, flags);
        }

        public void CopyTo(byte[] destination)
        {
            if (destination == null)
                throw new ArgumentNullException("destination");

            Marshal.Copy(Data, destination, 0, Length);
        }

        public byte[] GetBytes()
        {
            CheckCreated();
            var array = new byte[Length];
            CopyTo(array);
            return array;
        }
    }

    public delegate void PacketFreeCallback(UDPPacket packet);

    [Flags]
    public enum PacketFlags
    {
        None = 0,
        Reliable = 1 << 0,
        Unsequenced = 1 << 1,
        NoAllocate = 1 << 2,
        UnreliableFragment = 1 << 3
    }
}