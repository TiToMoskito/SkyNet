using SkyNet.Utils;
using System;
using UnityEngine;

namespace SkyNet
{
    public class Log
    {
        public Log()
        {
            if ((Config.instance.logTargets & LogTargets.Unity) == LogTargets.Unity)
                SkyLog.Add(new Unity());
        }

        internal class Unity : IWriter, IDisposable
        {
            void IWriter.Info(string message)
            {
                Debug.Log(SkyLog.GetTime() + (object)message);
            }

            void IWriter.Debug(string message)
            {
                Debug.Log(SkyLog.GetTime() + (object)message);
            }

            void IWriter.Warn(string message)
            {
                Debug.LogWarning(SkyLog.GetTime() + (object)message);
            }

            void IWriter.Error(string message)
            {
                Debug.LogError(SkyLog.GetTime() + (object)message);
            }

            public void Dispose()
            {
            }
        }
    }    
}
