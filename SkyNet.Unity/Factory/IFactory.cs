using System;

namespace SkyNet
{
    internal interface IFactory
    {
        Type TypeObject { get; }

        TypeId TypeID { get; }

        UniqueId TypeKey { get; }

        object Create();
    }
}
