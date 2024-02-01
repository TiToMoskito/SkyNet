namespace SkyNet
{
    /// <summary>
    /// Channel information class created as a result of retrieving a list of channels.
    /// </summary>
    public class ChannelInfo
    {
        public int id;              // Channel's ID
        public ushort players;      // Number of players present
        public ushort limit;        // Player limit
        public bool hasPassword;    // Whether the channel is password-protected or not
        public bool isPersistent;   // Whether the channel is persistent or not
        public string level;        // Name of the loaded level
    }
}
