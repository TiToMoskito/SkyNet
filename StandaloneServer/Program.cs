using System;
using System.Threading;
using System.Threading.Tasks;
using SkyNet;

namespace StandaloneServer
{
    class Program
    {
        private static Server server;
        private static SkyNetwork network;
        private static Config config;

        private static void Main(string[] args)
        {
            config = Config.instance;
            config.logTargets = LogTargets.SystemOut | LogTargets.File;

            network = new SkyNetwork();
            server = network.CreateServer("Test Server");

            Task infoTask = Info();

            string command;
            bool quitNow = false;
            while (!quitNow)
            {
                command = Console.ReadLine();
                switch (command)
                {
                    case "/kick":
                        Console.WriteLine("kick");
                        break;
                    case "/ban":
                        Console.WriteLine("ban");
                        break;
                    case "/quit":
                        quitNow = true;
                        break;
                    default:
                        Console.WriteLine("Unknown Command " + command);
                        break;
                }
            }

            network.StopServer(server);
        }

        private static async Task Info()
        {
            await Task.Factory.StartNew(() =>
            {
                while (server != null)
                {
                    Console.Title = string.Format(server.Clients + "/" + config.serverConnectionLimit + " Connections, " + server.Entities + " Entites - " + NetStats.numPacketsIn + " Packets/s (" + ((ulong)NetStats.numBytesIn).FormatBytes(true) + ") in, " + NetStats.numPacketsOut + " Packets/s (" + ((ulong)NetStats.numBytesOut).FormatBytes(true) + ") out");
                    Thread.Sleep(1);
                }
            });
        }
    }
}
