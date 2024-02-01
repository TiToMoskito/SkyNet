using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("SkyNet.Unity")]
[assembly: InternalsVisibleTo("SkyNet.Generated")]
namespace SkyNet
{
    public class SkyNetwork
    {
        public SkyNetwork()
        {
            SkyLog.Setup(Config.instance.logTargets);
            SkyLog.Info("SkyNet started with a tick rate: " + Config.instance.tickRate + " (ticks per second)");
        }

        #region Client
        public Client CreateClient()
        {
            return new Client();
        }
        #endregion

        #region Server
        public Server CreateServer()
        {
            return CreateServer(string.Empty);
        }
        
        public Server CreateServer(string _serverName)
        {            
            Server server = new Server();
            server.Start(_serverName);
            return server;
        }

        public void StopServer(Server _server)
        {
            _server.Stop();
        }
        #endregion
    }
}
