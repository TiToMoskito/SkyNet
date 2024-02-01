using UnityEngine;

public class Client : MonoBehaviour
{
    void Start()
    {
        //SkyManager.Init();
        SkyManager.CreateClient();
        SkyManager.playerName = "Harald"+Random.Range(0,1000);
    }

    public void Connect()
    {      
        SkyManager.Connect("127.0.0.1", 9382);        
    }

    public void Disconnect()
    {
        SkyManager.Disconnect();
    }

    public void Create()
    {
        //11 bytes
        //MessageEvent evnt = new MessageEvent();
        //evnt.message = "test";
        //evnt.Send(SkyTargets.All);

        SkyManager.Instantiate(10, SkyPrefabs.Player);

        //4 bytes
        //byte[] bytes = Encoding.ASCII.GetBytes("test");
        //Packet packet = new Packet();
        //packet.Create(bytes, 0, bytes.Length, PacketFlags.Reliable); // Reliable Sequenced
        //SkyNetwork.CurrentConnection.Peer.Send(0, packet);
    }

    public void Send()
    {
        //LogEvent evnt = new LogEvent();
        //evnt.msg = "FICKER NR1 VON " + SkyManager.playerID;
        //evnt.Send(SkyNet.Targets.Others);
    }

    public void JoinChannel()
    {
        SkyManager.JoinChannel(10, "Game", false, "", 200, true);
    }

    public void CloseChannel()
    {
        SkyManager.CloseChannel(10);
    }

}