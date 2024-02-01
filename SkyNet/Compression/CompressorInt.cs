using SkyNet.Utils;

namespace SkyNet
{
    /// <summary>
    /// Compresses integers to a given range
    /// </summary>
    public class CompressorInt
    {
        private readonly int m_MinValue;
        private readonly int m_MaxValue;

        private readonly int m_RequiredBits;
        private readonly uint m_Mask;

        private readonly bool m_enabled;

        /// <summary>
        /// Returns if this compressor is active
        /// </summary>
        public bool Enabled
        {
            get
            {
                return m_enabled;
            }
        }


        /// <summary>
        /// Default constructor.
        /// </summary>
        public CompressorInt(int minValue, int maxValue, bool enabled)
        {
            m_MinValue = minValue;
            m_MaxValue = maxValue;

            m_RequiredBits = ComputeRequiredBits();
            m_Mask = (uint)((1L << m_RequiredBits) - 1);

            m_enabled = enabled;
        }

        /// <summary>
        /// Constructor with config.
        /// </summary>
        public CompressorInt(CompressorIntConfig config)
        {
            m_MinValue = config.minValue;
            m_MaxValue = config.maxValue;

            m_RequiredBits = ComputeRequiredBits();
            m_Mask = (uint)((1L << m_RequiredBits) - 1);
        }

        /// <summary>
        /// Returns the number of bits required to store values between the specified range.
        /// </summary>
        public int BitsRequired
        {
           get { return m_RequiredBits; }
        }

        /// <summary>
        /// Compresses the value.
        /// </summary>
        public uint Compress(int value)
        {
            if ((value < m_MinValue) || (value > m_MaxValue))
            {
                //if (Debug.isDebugBuild)
                //{
                //    Debug.LogWarning("Clamping value " + value + " to [" + m_MinValue + "," + m_MaxValue + "]");
                //}

                value = Math.Clamp(value, m_MinValue, m_MaxValue);
            }

            return (uint)(value - m_MinValue) & m_Mask;
        }

        /// <summary>
        /// Decompresses the value.
        /// </summary>
        public int Decompress(uint data)
        {
            return (int)(data + m_MinValue);
        }

        private int ComputeRequiredBits()
        {
            if (m_MinValue > m_MaxValue)
            {
                return 0;
            }

            long minLong = (long)m_MinValue;
            long maxLong = (long)m_MaxValue;
            uint range = (uint)(maxLong - minLong);
            return Math.Log2Fast(range) + 1;
        }
    }
}
