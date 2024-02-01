using SkyNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SkyManager : MonoBehaviour
{
    // Instance pointer
    private static SkyManager m_instance;
    private static SkyManager instance
    {
        get
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return m_instance;
#endif
            if (m_instance == null)
            {
                GameObject go = new GameObject("Network Manager");
                m_instance = go.AddComponent<SkyManager>();
                go.AddComponent<SkyPoll>();
                ThreadManager.Create();
            }
            return m_instance;
        }
    }

    #region Delegates

    /// <summary>
    /// Default function to check whether the SkyNet is currently joining a channel.
    /// </summary>
    public static bool IsJoiningChannelDefault(int channelID)
    {
        if (!isConnected) return false;

        if (channelID < 0)
        {
            return (m_instance.m_loadingLevel.Count != 0);
        }
        return m_instance.m_loadingLevel.Contains(channelID);
    }
    public delegate bool IsJoiningChannelFunc(int channelID);
    #endregion

    #region Private Variables  
    //SkyNetwork
    private SkyNetwork m_skyNet = null;
    //Config
    private Config m_config = new Config();
    //List of currently loading levels
    private List<int> m_loadingLevel = new List<int>();
    //Entity Dispatcher
    private EntityDispatcher m_entityDispatcher = new EntityDispatcher();
    //Event Dispatcher
    private EventDispatcher m_eventDispatcher = new EventDispatcher();
    //Unity Logging
    private Log m_log;
    //Client
    private Client m_client;
    //Server
    private Server m_server;
    //Temp Client Name
    private string m_tempClientName;
    //Prefab Pool
    private static IPrefabPool PrefabPool = new DefaultPrefabPool();
    #endregion

    #region Public Static Variables
    //Debug Drawer
    public static IDebugDrawer DebugDrawer;
    #endregion

    #region Public Variables

    #endregion

    #region Properties 
    /// <summary>
    /// Return the Event Dispatcher.
    /// </summary>
    public static EventDispatcher EventDispatcher { get { return m_instance != null ? m_instance.m_eventDispatcher : (instance != null ? instance.m_eventDispatcher : null); } }

    /// <summary>
    /// Return the EntityDispatcher.
    /// </summary>
    public static EntityDispatcher EntityDispatcher { get { return m_instance != null ? m_instance.m_entityDispatcher : (instance != null ? instance.m_entityDispatcher : null); } }

    /// <summary>
    /// Client used for communication.
    /// </summary>
    public static Client Client{ get { return m_instance != null ? m_instance.m_client : (instance != null ? instance.m_client : null); } }

    /// <summary>
    /// Server
    /// </summary>
    public static Server Server { get { return m_instance != null ? m_instance.m_server : (instance != null ? instance.m_server : null); } }

    /// <summary>
    /// Whether we're currently connected.
    /// </summary>
    public static bool isConnected { get { return Client != null ? Client.isConnected : false; } }

    /// <summary>
    /// The player's unique identifier.
    /// </summary>
    static public int playerID { get { return isConnected ? Connection.ClientID : UnityEngine.Random.Range(999, 999999); } }

    /// <summary>
    /// Get or set the player's name as everyone sees him on the network.
    /// </summary>
    static public string playerName
    {
        get
        {
            return isConnected ? Client.playerName : "Player";
        }
        set
        {
            if (playerName != value)
            {
                if (Client != null)
                    Client.playerName = value;
                else
                    m_instance.m_tempClientName = value;
            }
        }
    }

    /// <summary>
    /// The player's Connection.
    /// </summary>
    static public Connection Connection { get { return Client != null ? Client.Connection : null; } }

    /// <summary>
    /// The player's Connection.
    /// </summary>
    static public IEnumerable<Connection> Connections { get { return Client != null ? Client.Connections : Enumerable.Empty<Connection>(); } }

    /// <summary>
    /// Current network time.
    /// </summary>
    static public uint deltaTime { get { return UDPLibrary.Time; } }

    /// <summary>
    /// Whether we are currently trying to join the specified channel.
    /// </summary>
    public static IsJoiningChannelFunc IsJoiningChannel = IsJoiningChannelDefault;
    #endregion

    #region Public Static Functions

    public static void CreateServer(string _map)
    {
        CreateServer(string.Empty, _map);
    }

    public static void CreateServer(string _serverName, string _map)
    {
        GlobalEventListenerBase.StartBeginInvoke();

        instance.m_server = instance.m_skyNet.CreateServer(_serverName);

        if (instance.m_server != null)
        {
            GlobalEventListenerBase.StartDoneInvoke();

            instance.SetServerCallbacks();
            instance.OnLoadLevel(0, _map);
        }
        else
        {
            GlobalEventListenerBase.StartFailedInvoke();
        }        
    }

    public static void CreateClient()
    {
        GlobalEventListenerBase.StartBeginInvoke();

        instance.m_client = instance.m_skyNet.CreateClient();

        if (instance.m_client != null)
        {
            GlobalEventListenerBase.StartDoneInvoke();

            instance.SetClientCallbacks();
            playerName = instance.m_tempClientName;
        }
        else
        {
            GlobalEventListenerBase.StartFailedInvoke();
        }
    }

    public static void Connect(string address, int port)
    {
        if (Client == null)
            CreateClient();

        Client.Connect(address, (ushort)port);
    }

    public static void Disconnect()
    {
        GlobalEventListenerBase.DisconnectedInvoke(Client.Connection);

        Client.Disconnect();
        instance.m_loadingLevel.Clear();
        instance.m_entityDispatcher.RemoveAll();        

        EntityDispatcher.RemoveAll();

        string[] scene = SceneUtility.GetScenePathByBuildIndex(0).Replace(".unity", "").Split('/');
        string map = scene[scene.Length - 1];
        instance.OnLoadLevel(0, map);
    }

    public static void JoinChannel(int channelID, string levelName)
    {
        JoinChannel(channelID, levelName, false, string.Empty, 65535, true);
    }

    public static void JoinChannel(int channelID, string levelName, bool leaveCurrentChannel)
    {
        JoinChannel(channelID, levelName, false, string.Empty, 65535, leaveCurrentChannel);
    }

    public static void JoinChannel(int channelID, string levelName, bool persistent, string password, ushort playerLimit, bool leaveCurrentChannel)
    {
        if (!Client.Connection.IsInChannel(channelID))
        {
            //if (leaveCurrentChannel) LeaveAllChannels();

            if (isConnected)
            {
                RequestJoinChannelEvent evnt = new RequestJoinChannelEvent();
                evnt.channelID = channelID;
                evnt.levelName = levelName;
                evnt.persistent = persistent;
                evnt.password = password;
                evnt.playerLimit = playerLimit;
                Client.Connection.SendEvent(evnt);
            }
        }
    }

    public static void CloseChannel(int channelID)
    {
        if (Client.Connection.IsInChannel(channelID))
        {
            RequestCloseChannelEvent evnt = new RequestCloseChannelEvent();
            evnt.channelID = channelID;
            Client.Connection.SendEvent(evnt);
        }
    }

    public static SkyEntity Instantiate(int channelID, PrefabId prefabId)
    {
        return Instantiate(channelID, prefabId, Vector3.zero, Quaternion.identity);
    }

    public static SkyEntity Instantiate(int channelID, PrefabId prefabId, Vector3 position, Quaternion rotation)
    {
        SkyEntity component = PrefabPool.LoadPrefab(prefabId).GetComponent<SkyEntity>();

        if (component.m_prefabId != prefabId.Value)
            throw new Exception("PrefabId for SkyEntity component did not return the same value as prefabId passed in as argument to Instantiate");

        if (component.serializerGuid == UniqueId.None)
            return null;        

        System.Random range = new System.Random();
        NetworkId netID = new NetworkId((uint)range.Next(0, 10000));
        GameObject go = PrefabPool.Instantiate(prefabId, position, rotation);
        go.name = string.Format("{0}_(Local:{1})", go.name, playerID);
        Entity entity = Entity.CreateFor(go, prefabId, Factory.GetFactory(component.serializerGuid).TypeID, netID);
        entity.Initialize(Connection);
        entity.Attach();

        RequestCreateObjectEvent evnt = new RequestCreateObjectEvent();
        evnt.channelID = channelID;
        evnt.type = component.m_persistThroughSceneLoads ? (byte)1 : (byte)2;
        evnt.prefabID = prefabId;
        evnt.tempNetID = netID;
        Connection.SendEvent(evnt);
        
        return component;
    }

    public static void Destroy(Entity _entity)
    {
        if(Connection != null && Connection.isConnected)
        {
            ResponseDestroyObjectEvent evnt = new ResponseDestroyObjectEvent();
            evnt.clientID = playerID;
            evnt.channelID = 0;
            evnt.objects = new NetworkId[] { _entity.NetworkId };
            Connection.SendEvent(evnt);
        }
    }

    public static SkyEntity FindEntity(NetworkId _id)
    {
        return EntityDispatcher.Find(_id);
    }

    /// <summary>
    /// Get the player associated with the specified ID.
    /// </summary>
    public static Connection GetPlayer(int id)
    {
        if (id == 0) return null;
        if (isConnected) return Client.GetConnection(id, false);
        if (id == playerID) return Connection;
        return null;
    }

    /// <summary>
    /// Get the player associated with the specified name.
    /// </summary>
    public static Connection GetPlayer(string name)
    {
        if (isConnected) return Client.GetConnection(name);
        if (name == playerName) return Connection;
        return null;
    }
    #endregion

    #region Internal Static Functions
    internal static void Send(SkyNet.Event _evnt, PacketFlags _flag, Targets _targets)
    {
        Client.Connection.SendEvent(_evnt, _flag, _targets);
    }

    internal static void Send(SkyNet.Event _evnt, PacketFlags _flag, Connection _target)
    {
        Client.Connection.SendEvent(_evnt, _flag, _target);
    }

    internal static void SimulatePhysics()
    {
        if (EntityDispatcher != null)
            EntityDispatcher.Simulate();        
    }

    internal static void PollEvents()
    {
        //if (Client != null)
        //    Client.ListenTest();

        
    }

    void Update()
    {
        //if (Server != null)
        //    Server.Listen();
    }
    #endregion

    #region Private Functions
    private void SetClientCallbacks()
    {
        Client.onObjectCreated = OnCreateObject;
        Client.onObjectDestroyed = OnDestroyObject;
        Client.onLoadLevel = OnLoadLevel;
        Client.onDisconnect = delegate () { ThreadManager.Run(() => { Disconnect(); }); };
        Client.onConnected = delegate () { ThreadManager.Run(() => { GlobalEventListenerBase.ConnectedInvoke(null); }); };
        Client.onChannelJoined = delegate (Connection conn, int channel, string msg) { ThreadManager.Run(() => { GlobalEventListenerBase.ChannelJoinedInvoke(Client.Connection, channel, msg); }); };
        Client.onChannelLeft = delegate (Connection conn, int channel, string msg) { ThreadManager.Run(() => { GlobalEventListenerBase.ChannelLeftInvoke(Client.Connection, channel, msg); }); };
        Client.onDispatchEvent = delegate (TypeId _typeId, SkyNet.NetBuffer _packer, Connection _source) {
            ThreadManager.Run(() => {
                SkyNet.Event evnt = Factory.NewEvent(_typeId);
                evnt.Unpack(_packer);
                evnt.RaisedBy = _source;
                evnt.Data.TypeId = _typeId;
                instance.m_eventDispatcher.Dispatch(evnt);
            });
        };
        Client.onDispatchState = delegate (NetworkId netID, SkyNet.NetBuffer packer) { ThreadManager.Run(() => { instance.m_entityDispatcher.Dispatch(netID, packer); }); };
    }

    private void SetServerCallbacks()
    {
        Server.onLoadLevelRemoteBegin = delegate (Connection conn, int channel, string scene) { ThreadManager.Run(() => { GlobalEventListenerBase.SceneLoadRemoteBeginInvoke(conn, channel, scene); }); };
        Server.onLoadLevelRemoteDone = delegate (Connection conn, int channel, string scene) { ThreadManager.Run(() => { GlobalEventListenerBase.SceneLoadRemoteDoneInvoke(conn, channel, scene); }); };
    }
    #endregion

    #region Unity
    private void Awake()
    {
        if (m_instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            m_instance = this;

            Application.runInBackground = true;
            Time.fixedDeltaTime = 1f / Config.instance.tickRate;

            m_log = new Log();
            m_skyNet = new SkyNetwork();            
            m_entityDispatcher = new EntityDispatcher();
            m_eventDispatcher = new EventDispatcher();
            DebugDrawer = typeof(IDebugDrawer).FindInterfaceImplementations().Select((x => Activator.CreateInstance(x))).Cast<IDebugDrawer>().ToArray()[0];
            typeof(IFactoryRegister).FindInterfaceImplementations().Select((x => Activator.CreateInstance(x))).Cast<IFactoryRegister>().ToArray()[0].EnvironmentSetup();
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        if(Client != null)
            Client.Dispose();

        if (Server != null)
            Server.Dispose();

        SkyLog.RemoveAll();
        m_instance = null;
        Destroy(gameObject);
    }
    #endregion

    #region Callback Functions        

    /// <summary>
    /// A new object being created.
    /// </summary>
    void OnCreateObject(ResponseCreateObjectEvent _evnt)
    {
        ThreadManager.Run(() => {
            if (_evnt == null) return;

            if (_evnt.creator == playerID)
            {
                EntityDispatcher.Update(_evnt.tempNetID, _evnt.netID);
            }
            else
            {
                SkyEntity component = PrefabPool.LoadPrefab(_evnt.prefabID).GetComponent<SkyEntity>();

                if (component == null)
                    return;

                if (component.m_prefabId != _evnt.prefabID.Value)
                    throw new Exception("PrefabId for SkyEntity component did not return the same value as prefabId passed in as argument to Instantiate");

                if (component.serializerGuid == UniqueId.None)
                    return;

                GameObject go = PrefabPool.Instantiate(_evnt.prefabID, Vector3.zero, Quaternion.identity);
                if (go == null) return;
                go.name = string.Format("{0}(Remote:{1})", go.name, _evnt.creator);
                Entity entity = Entity.CreateFor(go, _evnt.prefabID, Factory.GetFactory(component.serializerGuid).TypeID, _evnt.netID);
                if (entity == null) return;
                entity.Initialize(null);
                entity.Attach();
            }
        });
    }

    /// <summary>
    /// A object being destroyed.
    /// </summary>
    void OnDestroyObject(ResponseDestroyObjectEvent _evnt)
    {
        ThreadManager.Run(() => {
            for (int i = 0; i < _evnt.objects.Length; i++)
            {
                EntityDispatcher.Remove(_evnt.objects[i]);
            }
        });
    }

    private void OnLoadLevel(int _channelID, string _map)
    {
        ThreadManager.Run(() => {
            m_loadingLevel.Add(_channelID);
            StartCoroutine("LoadLevelCoroutine", new object[2] { _channelID, _map });
        });
    }

    private AsyncOperation m_loadLevelOperation = null;

    private delegate void LoadSceneFunc(string levelName);
    private delegate AsyncOperation LoadSceneAsyncFunc(string levelName);

    private IEnumerator LoadLevelCoroutine(object[] _params)
    {
        yield return null;

        ResponseLoadLevelStatusEvent evnt1 = new ResponseLoadLevelStatusEvent();
        evnt1.clientID = playerID;
        evnt1.status = false;
        evnt1.channelID = (int)_params[0];
        evnt1.levelName = (string)_params[1];


        if (Client != null)
            Connection.SendEvent(evnt1);

        GlobalEventListenerBase.SceneLoadLocalBeginInvoke((int)_params[0], (string)_params[1]);

        m_loadLevelOperation = LoadSceneAsync((string)_params[1]);
        m_loadLevelOperation.allowSceneActivation = false;

        while (m_loadLevelOperation.progress < 0.9f)
            yield return null;

        m_loadLevelOperation.allowSceneActivation = true;       
        yield return m_loadLevelOperation;

        GlobalEventListenerBase.SceneLoadLocalDoneInvoke((int)_params[0], (string)_params[1]);

        evnt1.status = true;

        if (Client != null)
            Connection.SendEvent(evnt1);

        m_loadLevelOperation = null;
        m_loadingLevel.Remove((int)_params[0]);
    }

    private LoadSceneAsyncFunc LoadSceneAsync = delegate (string levelName)
    {
        if (!string.IsNullOrEmpty(levelName))
        {
            return SceneManager.LoadSceneAsync(levelName);
        }
        return null;
    };
    #endregion
}