using System;

namespace SkyNet.Utils
{
    public interface IWriter : IDisposable
    {
        void Info(string message);

        void Debug(string message);

        void Warn(string message);

        void Error(string message);
    }
}