namespace SkyNet
{
    public interface IProtocolToken
    {
        void Read(UDPPacket packet);

        void Write(UDPPacket packet);
    }
}
