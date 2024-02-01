using System;

namespace SkyNet
{
    public static class UDPLibrary
    {
        public const uint maxChannelCount = 65535;
        public const uint maxPeers = 0xFFF;
        public const uint maxPacketSize = 32 * 1024 * 1024;
        public const uint throttleScale = 32;
        public const uint throttleAcceleration = 2;
        public const uint throttleDeceleration = 2;
        public const uint throttleInterval = 5000;
        public const uint timeoutLimit = 32;
        public const uint timeoutMinimum = 5000;
        public const uint timeoutMaximum = 30000;
        public const uint version = (2 << 16) | (1 << 8) | (5);

        public static bool Initialize()
        {
            return UDPNative.enet_initialize() == 0;
        }

        public static bool Initialize(Callbacks inits)
        {
            var nativeCallbacks = inits.NativeData;

            return UDPNative.enet_initialize_with_callbacks(version, ref nativeCallbacks) == 0;
        }

        public static void Deinitialize()
        {
            UDPNative.enet_deinitialize();
        }

        public static uint Time
        {
            get
            {
                return UDPNative.enet_time_get();
            }
        }
    }

    internal static class ArrayPool
    {
        [ThreadStatic]
        private static byte[] byteBuffer;
        [ThreadStatic]
        private static IntPtr[] pointerBuffer;

        public static byte[] GetByteBuffer()
        {
            if (byteBuffer == null)
                byteBuffer = new byte[64];

            return byteBuffer;
        }

        public static IntPtr[] GetPointerBuffer()
        {
            if (pointerBuffer == null)
                pointerBuffer = new IntPtr[UDPLibrary.maxPeers];

            return pointerBuffer;
        }
    }
}