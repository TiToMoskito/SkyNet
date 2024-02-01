using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkyNet.Utils
{
    /// <summary>
    /// Utility class that contains useful math and bit operations required by the bit packer.
    /// </summary>
    public static class BitUtils
    {      
        /// <summary>
        /// Finds the highest bit position in the given byte
        /// </summary>
        public static int FindHighestBitPosition(byte data)
        {
            int shiftCount = 0;
            while (data > 0)
            {
                data >>= 1;
                shiftCount++;
            }
            return shiftCount;
        }


#if !NETFX_CORE
        /// <summary>
        /// Extension method that clears the string builder without deallocating memory
        /// </summary>
        public static void Clear(this System.Text.StringBuilder sb)
        {
            sb.Length = 0;
        }
#endif
    }
}
