using SkyNet.Utils;

namespace SkyNet
{
    /// <summary>
    /// Compresses floats to a given range with a given precision.
    /// http://stackoverflow.com/questions/8382629/compress-floating-point-numbers-with-specified-range-and-precision
    /// </summary>
    public class CompressorFloat
    {
        private readonly float m_Precision;
        private readonly float m_InvPrecision;

        private readonly float m_MinValue;
        private readonly float m_MaxValue;

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
        public CompressorFloat(float minValue, float maxValue, float precision, bool enabled)
        {
            m_MinValue = minValue;
            m_MaxValue = maxValue;
            m_Precision = precision;

            m_InvPrecision = 1.0f / precision;
            m_RequiredBits = ComputeRequiredBits();
            m_Mask = (uint)((1L << m_RequiredBits) - 1);

            m_enabled = enabled;
        }

        /// <summary>
        /// Constructor with config.
        /// </summary>
        public CompressorFloat(CompressorFloatConfig config)
        {
            m_MinValue = config.minValue;
            m_MaxValue = config.maxValue;
            m_Precision = config.precision;

            m_InvPrecision = 1.0f / config.precision;
            m_RequiredBits = ComputeRequiredBits();
            m_Mask = (uint)((1L << m_RequiredBits) - 1);
        }

        /// <summary>
        /// Returns the number of bits required to store values between the specified range with the given precision.
        /// </summary>
        public int BitsRequired
        {
            get { return m_RequiredBits; }
        }

        /// <summary>
        /// Compresses the value.
        /// </summary>
        public uint Compress(float value)
        {
            if ((value < m_MinValue) || (value > m_MaxValue))
            {
                //if (Debug.isDebugBuild)
                //{
                //    Debug.LogWarning("Clamping value " + value + " to [" + m_MinValue + "," + m_MaxValue + "]");
                //}

                value = Math.Clamp(value, m_MinValue, m_MaxValue);
            }

            float adjusted = (value - m_MinValue) * m_InvPrecision;
            return (uint)(adjusted + 0.5f) & m_Mask;
        }

        /// <summary>
        /// Decompresses the value.
        /// </summary>
        public float Decompress(uint data)
        {
            float adjusted = ((float)data * m_Precision) + m_MinValue;
            return Math.Clamp(adjusted, m_MinValue, m_MaxValue);
        }

        private int ComputeRequiredBits()
        {
            float range = m_MaxValue - m_MinValue;
            float maxVal = range * m_InvPrecision;
            return Math.Log2Fast((uint)(maxVal + 0.5f)) + 1;
        }
    }
}
