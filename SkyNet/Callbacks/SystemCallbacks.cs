namespace SkyNet
{    public class SystemCallbacks
    {
        /// <summary>
        /// Notification when a object was created.
        /// </summary>
        public OnObjectCreated onObjectCreated;        
        public delegate void OnObjectCreated(ResponseCreateObjectEvent _evnt);

        /// <summary>
        /// Notification when a object got deleted
        /// </summary>
        public OnObjectDestroyed onObjectDestroyed;
        public delegate void OnObjectDestroyed(ResponseDestroyObjectEvent _evnt);

        /// <summary>
        /// Notification sent when changing levels.
        /// </summary>
        public OnLoadLevel onLoadLevel;
        public delegate void OnLoadLevel(int channelID, string levelName);

        /// <summary>
        /// Notification sent when a player loading levels.
        /// </summary>
        public OnLoadLevelRemoteBegin onLoadLevelRemoteBegin;
        public delegate void OnLoadLevelRemoteBegin(Connection conn, int channelID, string levelName);

        /// <summary>
        /// Notification sent when a player loading levels.
        /// </summary>
        public OnLoadLevelRemoteDone onLoadLevelRemoteDone;
        public delegate void OnLoadLevelRemoteDone(Connection conn, int channelID, string levelName);

        /// <summary>
        /// Notification sent after the connection terminates for any reason.
        /// </summary>
        public OnDisconnect onDisconnect;
        public delegate void OnDisconnect();

        /// <summary>
        /// Notification when connected
        /// </summary>
        public OnConnected onConnected;
        public delegate void OnConnected();

        /// <summary>
        /// Notification when a connection joined a channel
        /// </summary>
        public OnChannelJoined onChannelJoined;
        public delegate void OnChannelJoined(Connection conn, int channelID, string msg);

        /// <summary>
        /// Notification when a connection left a channel
        /// </summary>
        public OnChannelLeft onChannelLeft;
        public delegate void OnChannelLeft(Connection conn, int channelID, string msg);

        /// <summary>
        /// Notification when dispatching a Event
        /// </summary>
        public OnDispatchEvent onDispatchEvent;
        public delegate void OnDispatchEvent(TypeId _typeId, NetBuffer _packer, Connection _source);

        /// <summary>
        /// Notification when dispatching a State
        /// </summary>
        public OnDispatchState onDispatchState;
        public delegate void OnDispatchState(NetworkId netID, NetBuffer packer);

        /// <summary>
        /// Notification when EnvironmentSetup
        /// </summary>
        public OnEnvironmentSetup onEnvironmentSetup;
        public delegate void OnEnvironmentSetup();

        /// <summary>
        /// Thread Tick
        /// </summary>
        public OnNetworkTick onNetworkTick;
        public delegate void OnNetworkTick();
    }
}
