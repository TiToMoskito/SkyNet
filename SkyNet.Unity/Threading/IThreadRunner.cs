using System;

namespace SkyNet
{
    public interface IThreadRunner
    {
        void Execute(Action action);
    }
}
