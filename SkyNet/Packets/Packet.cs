
namespace SkyNet
{
    public class Packet
    {
        private UDPPacket m_UDPPacket;
        private Header m_header;
        private NetBuffer m_content;

        public Header Header { get { return m_header; } }
        public NetBuffer Content { get { return m_content; } }

        public Packet(Header _header)
        {
            m_UDPPacket = new UDPPacket();
            m_header = _header;

            m_content = new NetBuffer();
            m_content.Write((byte)m_header.flag);
            m_content.Write((byte)m_header.targets);
            m_content.Write((uint)m_header.type);
            m_content.Write(m_header.connection);
            m_content.WriteNetworkId(m_header.networkId);
            m_content.WriteTypeId(m_header.typeId);            
        }

        public Packet(UDPPacket _UDPPacket)
        {
            m_UDPPacket = _UDPPacket;

            m_content = new NetBuffer(_UDPPacket.GetBytes());
            m_header =  new Header(m_content);
        }

        public void Create(NetBuffer _packer)
        {
            m_content.Write(_packer.Length);
            m_content.Write(_packer.ToArray());

            SkyLog.Debug("Verpackt: {0} {1}", _packer.Length, System.Text.Encoding.UTF8.GetString(_packer.ToArray()));

            int contentSize = _packer.Length;
            byte[] content = _packer.ToArray();

            SkyLog.Debug("Entpackt: {0} {1}", contentSize, System.Text.Encoding.UTF8.GetString(content));

            m_UDPPacket.Create(m_content.ToArray(), m_content.Length, m_header.flag);
        }

        public void Create(byte[] _data)
        {
            m_content = new NetBuffer();
            m_content.Write((byte)m_header.flag);
            m_content.Write((byte)m_header.targets);
            m_content.Write((uint)m_header.type);
            m_content.Write(m_header.connection);
            m_content.WriteNetworkId(m_header.networkId);
            m_content.WriteTypeId(m_header.typeId);
            m_content.Write((byte)_data.Length);
            m_content.Write(_data, m_content.Position, _data.Length);

            int m_bufferSize = m_content.Position;
            m_UDPPacket.Create(m_content.ToArray(), m_bufferSize, m_header.flag);
        }

        public bool SendToTransport(int _channelId, Connection _conn)
        {
            return _conn.Send((byte)_channelId, m_UDPPacket);
        }
    }   
}