namespace SkyNet
{
    /// <summary>
    /// Created objects are saved by the channels.
    /// </summary>
    public class CreatedObject
    {
        public int playerID;
        public NetworkId netID;
        public PrefabId prefabID;
        public byte type;
    }
}
