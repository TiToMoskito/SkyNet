using System;
using System.Net;

namespace SkyNet
{
    public class Tools
    {
        public IPEndPoint GetIPEndPoint(string _ipAddress, int _port)
        {
            return new IPEndPoint(IPAddress.Parse(_ipAddress), _port);
        }
    }
}
