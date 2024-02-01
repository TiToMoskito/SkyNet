using System.Xml.Serialization;

namespace SkyNet.Compiler
{
    [XmlInclude(typeof(EventDefinition))]
    [XmlInclude(typeof(StateDefinition))]
    [XmlInclude(typeof(ObjDefinition))]
    public abstract class AssetDefinition
    {
        [XmlIgnore]
        public string FileName;
        [XmlIgnore]
        public bool Delete;
        [XmlElement]
        public string Name;
        [XmlElement]
        public string UniqueId;
        [XmlElement]
        public ConfigPacketFlags PacketFlag = ConfigPacketFlags.UnreliableFragment;
        [XmlElement]
        public ConfigTargets PacketTarget = ConfigTargets.OthersSaved;
    }

    public enum ConfigPacketFlags
    {
        Reliable = 1 << 0,
        UnreliableFragment = 1 << 3,
        Unsequenced = 1 << 1
    }

    public enum ConfigTargets
    {
        All,
        AllSaved,
        Others,
        OthersSaved,
        Server
    }
}
