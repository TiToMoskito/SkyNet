using System;
using System.Xml.Serialization;

namespace SkyNet
{
    /// <summary>
    /// Data struct to cofigure an integer compressor.
    /// </summary>
    [Serializable]
    public struct CompressorIntConfig
    {
        [XmlElement]
        public int minValue;
        [XmlElement]
        public int maxValue;
        [XmlElement]
        public bool Enabled;
    }
}
