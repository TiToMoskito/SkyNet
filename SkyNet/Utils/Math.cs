using System;

namespace SkyNet.Utils
{
    public static class Math
    {
        // Lookup table for fast Log2 calculations:
        // http://stackoverflow.com/questions/15967240/fastest-implementation-of-log2int-and-log2float
        private static readonly int[] m_DeBruijnLookup = new int[32]
        {
            0, 9, 1, 10, 13, 21, 2, 29, 11, 14, 16, 18, 22, 25, 3, 30,
            8, 12, 20, 28, 15, 17, 24, 7, 19, 27, 23, 6, 26, 5, 4, 31
        };

        /// <summary>
        /// Clamping a value to be sure it lies between two values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="aValue"></param>
        /// <param name="aMax"></param>
        /// <param name="aMin"></param>
        /// <returns></returns>
        public static T Clamp<T>(T aValue, T aMin, T aMax) where T : IComparable<T>
        {
            var _Result = aValue;
            if (aValue.CompareTo(aMax) > 0)
                _Result = aMax;
            else if (aValue.CompareTo(aMin) < 0)
                _Result = aMin;
            return _Result;
        }

        /// <summary>
        ///   <para>Clamps value between 0 and 1 and returns value.</para>
        /// </summary>
        /// <param name="value"></param>
        public static uint Clamp01(float value)
        {
            if (value < 0.0)
                return 0;
            if (value > 1.0)
                return 1;
            return (uint)value;
        }

        /// <summary>
        ///   <para>Linearly interpolates between a and b by t.</para>
        /// </summary>
        /// <param name="a">The start value.</param>
        /// <param name="b">The end value.</param>
        /// <param name="t">The interpolation value between the two floats.</param>
        /// <returns>
        ///   <para>The interpolated float result between the two float values.</para>
        /// </returns>
        public static uint Lerp(uint a, uint b, float t)
        {
            return a + (b - a) * Clamp01(t);
        }

        /// <summary>
        /// Optimized implementation of Log2
        /// </summary>
        public static int Log2Fast(uint v)
        {
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;

            return m_DeBruijnLookup[(v * 0x07C4ACDDU) >> 27];
        }

        /// <summary>
        /// Normal implementation of Log2
        /// </summary>
        public static int Log2(uint x)
        {
            return (int)(System.Math.Log(x) / System.Math.Log(2));
        }

        /// <summary>
        ///   <para>Returns the smallest of two or more values.</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="values"></param>
        public static float Min(float a, float b)
        {
            return (double)a >= (double)b ? b : a;
        }

        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="values"></param>
        public static int Min(int a, int b)
        {
            return a >= b ? b : a;
        }
    }
}
