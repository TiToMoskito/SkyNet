using System.Collections.Generic;
using System.Xml.Serialization;

namespace SkyNet.Compiler
{
    public class StateDefinition : AssetDefinition
    {
        [XmlArray]
        public List<PropertyDefinition> Properties = new List<PropertyDefinition>();
    }
}
