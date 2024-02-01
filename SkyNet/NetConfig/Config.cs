using System.IO;
using System.Xml;
using System.Xml.Serialization;

public class Config
{
    [XmlElement]
    public bool useNetworkSimulation;
    [XmlElement]
    public LogTargets logTargets = LogTargets.Unity | LogTargets.File;
    [XmlElement]
    public ConnectionAcceptMode serverConnectionAcceptMode = ConnectionAcceptMode.Auto;
    [XmlElement]
    public int connectionTimeout = 10000;
    [XmlElement]
    public int connectionRequestTimeout = 500;
    [XmlElement]
    public int connectionRequestAttempts = 20;
    [XmlElement]
    public int tickRate = 60;
    [XmlElement]
    public int sendRate = 30;
    [XmlElement]
    public int channelLimit = 65535;
    [XmlElement]
    public int serverConnectionLimit = 64;
    [XmlElement]
    public ushort serverPort = 9382;
    [XmlElement]
    public float simulatedLoss;
    [XmlElement]
    public int simulatedPingMean;
    [XmlElement]
    public int simulatedPingJitter;
    [XmlElement]
    public string masterServerGameId = "";
    [XmlElement]
    public string masterServerEndPoint = "0.0.0.0:24000";
    [XmlElement]
    public bool masterServerAutoConnect = false;
    [XmlElement]
    public bool masterServerAutoDisconnect = true;
    [XmlElement]
    public int capacity = 8;
    [XmlElement]
    public int stringCapacity = 1024;
    [XmlElement]
    public int growMultiplier = 2;
    [XmlElement]
    public int growAddition = 1;

    private static Config m_instance;
    public static Config instance
    {
        get
        {
            if (m_instance == null)
            {
                if (File.Exists("net_config.xml"))
                {
                    XmlReader xmlReader = XmlReader.Create("net_config.xml");
                    XmlSerializer xs = new XmlSerializer(typeof(Config));
                    m_instance = (Config)xs.Deserialize(xmlReader);                    
                    xmlReader.Close();
                }
                else
                {
                    m_instance = new Config();
                    XmlSerializer xs = new XmlSerializer(typeof(Config));
                    XmlWriter writer = XmlWriter.Create("net_config.xml");
                    xs.Serialize(writer, m_instance);                    
                    writer.Close();
                }
            }
            return m_instance;
        }
        set
        {
            m_instance = value;
            XmlSerializer xs = new XmlSerializer(typeof(Config));
            XmlWriter writer = XmlWriter.Create("net_config.xml");
            xs.Serialize(writer, m_instance);
            writer.Close();
        }
    }
}


public enum ConnectionAcceptMode
{
    Auto,
    Manual,
}