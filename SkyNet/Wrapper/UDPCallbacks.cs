
using System;
using System.Runtime.InteropServices;

namespace SkyNet
{
    public class Callbacks
    {
        private ENetCallbacks nativeCallbacks;

        internal ENetCallbacks NativeData
        {
            get
            {
                return nativeCallbacks;
            }

            set
            {
                nativeCallbacks = value;
            }
        }

        public Callbacks(AllocCallback allocCallback, FreeCallback freeCallback, NoMemoryCallback noMemoryCallback)
        {
            nativeCallbacks.malloc = Marshal.GetFunctionPointerForDelegate(allocCallback);
            nativeCallbacks.free = Marshal.GetFunctionPointerForDelegate(freeCallback);
            nativeCallbacks.no_memory = Marshal.GetFunctionPointerForDelegate(noMemoryCallback);
        }
    }

    public delegate IntPtr AllocCallback(IntPtr size);
    public delegate void FreeCallback(IntPtr memory);
    public delegate void NoMemoryCallback();

    [StructLayout(LayoutKind.Sequential)]
    public struct ENetCallbacks
    {
        public IntPtr malloc;
        public IntPtr free;
        public IntPtr no_memory;
    }
}
