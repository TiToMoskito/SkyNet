using System;

namespace SkyNet
{
    public static class UDPExtensions
    {
        public static int StringLength(this byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            int i;

            for (i = 0; i < data.Length && data[i] != 0; i++) ;

            return i;
        }
    }
}