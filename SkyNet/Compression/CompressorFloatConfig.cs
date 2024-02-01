using System;
using System.Xml.Serialization;

namespace SkyNet
{
    /// <summary>
    /// Data struct to cofigure a float compressor.
    /// </summary>
    [Serializable]
    public struct CompressorFloatConfig
    {
        [XmlElement]
        public float minValue;
        [XmlElement]
        public float maxValue;
        [XmlElement]
        public float precision;
        [XmlElement]
        public bool Enabled;
    }
}