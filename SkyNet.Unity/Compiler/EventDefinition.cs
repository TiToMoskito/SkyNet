using System.Collections.Generic;
using System.Xml.Serialization;

namespace SkyNet.Compiler
{
    public class EventDefinition : AssetDefinition
    {
        [XmlElement]
        public bool GlobalTarget;
        [XmlArray]
        public List<PropertyDefinition> Properties = new List<PropertyDefinition>();
    }
}
