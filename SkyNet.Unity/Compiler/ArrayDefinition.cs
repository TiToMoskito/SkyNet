using System.Xml.Serialization;

namespace SkyNet.Compiler
{
    public class ArrayDefinition
    {
        [XmlElement]
        public string Type = "Float";
        [XmlElement]
        public int Count = 0;
        [XmlArray]
        public PropertyDefinition[] Properties = new PropertyDefinition[] { new PropertyDefinition() };
    }
}
