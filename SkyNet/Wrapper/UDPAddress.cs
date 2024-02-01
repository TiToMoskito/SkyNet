using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SkyNet
{
    public struct UDPAddress
    {
        private ENetAddress nativeAddress;

        internal ENetAddress NativeData
        {
            get
            {
                return nativeAddress;
            }

            set
            {
                nativeAddress = value;
            }
        }

        public UDPAddress(ENetAddress address)
        {
            nativeAddress = address;
        }

        public ushort Port
        {
            get
            {
                return nativeAddress.port;
            }

            set
            {
                nativeAddress.port = value;
            }
        }

        public bool SetHost(string hostName)
        {
            if (hostName == null)
                throw new ArgumentNullException("hostName");

            return UDPNative.enet_address_set_host(ref nativeAddress, Encoding.ASCII.GetBytes(hostName)) == 0;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ENetAddress
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] host;
        public ushort port;
        public ushort sin6_scope_id;
    }
}