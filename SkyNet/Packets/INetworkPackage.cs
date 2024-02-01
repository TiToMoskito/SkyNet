namespace SkyNet
{
    /// <summary>
    /// Enables an object to be serialized into or deserialized from a bit packer.
    /// </summary>
    public interface INetworkPackage
    {
        /// <summary>
        /// Serializes the object into the bit packer.
        /// </summary>
        void Unpack(NetBuffer _stream);

        /// <summary>
        /// Deserializes the object from the bit packer.
        /// </summary>
        void Pack(NetBuffer _stream);
    }
}
