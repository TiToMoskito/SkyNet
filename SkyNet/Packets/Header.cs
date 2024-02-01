
namespace SkyNet
{
    public class Header
    {
        public NetBuffer Package;
        public PacketFlags flag { get; set; }
        public Targets targets { get; set; }
        public PacketType type { get; set; }
        public uint connection { get; set; }
        public NetworkId networkId { get; set; }
        public TypeId typeId { get; set; }

        public Header() { }

        /// <summary>
        /// Creates a Header by parameter
        /// </summary>
        public Header(PacketFlags flag, Targets targets, PacketType type, uint connection, NetworkId networkId)
        {
            this.flag = flag;
            this.targets = targets;
            this.type = type;
            this.connection = connection;
            this.networkId = networkId;
        }

        /// <summary>
        /// Creates a Header by parameter without connection details
        /// </summary>
        public Header(PacketFlags flag, Targets targets, PacketType type, NetworkId networkId)
        {
            this.flag = flag;
            this.targets = targets;
            this.type = type;
            this.networkId = networkId;
        }

        /// <summary>
        /// Creates a Header by parameter
        /// </summary>
        public Header(PacketFlags flag, Targets targets, PacketType type, uint connection, TypeId typeId)
        {
            this.flag = flag;
            this.targets = targets;
            this.type = type;
            this.connection = connection;
            this.typeId = typeId;
        }

        /// <summary>
        /// Creates a Header by parameter without connection details
        /// </summary>
        public Header(PacketFlags flag, Targets targets, PacketType type, TypeId typeId)
        {
            this.flag = flag;
            this.targets = targets;
            this.type = type;
            this.typeId = typeId;
        }

        /// <summary>
        /// Creates a Header by given byte array
        /// </summary>
        public Header(NetBuffer _stream)
        {
            flag = (PacketFlags)_stream.ReadByte();
            targets = (Targets)_stream.ReadByte();
            type = (PacketType)_stream.ReadUInt32();
            connection = _stream.ReadUInt32();
            networkId = _stream.ReadNetworkId();
            typeId = _stream.ReadTypeId();

            int contentSize = _stream.ReadInt32();
            byte[] content = _stream.ReadBytes(contentSize);

            SkyLog.Debug("Header: {0} {1}", contentSize, System.Text.Encoding.UTF8.GetString(content));

            Package = new NetBuffer(content);
        }

    }
}