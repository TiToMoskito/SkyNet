using System;
using System.Xml.Serialization;

namespace SkyNet.Compiler
{
    public class FloatCompression
    {
        [XmlElement]
        public int MinValue;
        [XmlElement]
        public int MaxValue;
        [XmlElement]
        public float Accuracy;
        [XmlElement]
        public bool Enabled;

        public int BitsRequired
        {
            get
            {
                return BitsForNumber((int)Math.Round((MaxValue + -MinValue) * (double)(1f / Accuracy)));
            }
        }

        public static FloatCompression Default()
        {
            return new FloatCompression()
            {
                MinValue = -2048,
                MaxValue = 2048,
                Accuracy = 0.01f
            };
        }

        public static FloatCompression DefaultAngle()
        {
            return new FloatCompression()
            {
                MinValue = 0,
                MaxValue = 360,
                Accuracy = 0.1f
            };
        }

        private static int BitsForNumber(int number)
        {
            if (number < 0)
                return 32;
            if (number == 0)
                return 1;
            for (int index = 31; index >= 0; --index)
            {
                int num = 1 << index;
                if ((number & num) == num)
                    return index + 1;
            }
            throw new Exception();
        }
    }
}
