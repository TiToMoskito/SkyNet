namespace SkyNet
{
    public abstract class Event
    {
        internal Connection m_raisedBy;
        internal PacketType m_packetType = PacketType.Event;
        internal PacketFlags m_flag;
        internal Event_Data Data;

        internal Event(Event_Data _data) { Data = _data; }

        public Connection RaisedBy { get { return m_raisedBy; } internal set { m_raisedBy = value; } }

        internal PacketType PacketType { get { return m_packetType; } }

        internal virtual void Unpack(NetBuffer _packer) { }
        internal virtual void Pack(NetBuffer _packer) { }
    }
}