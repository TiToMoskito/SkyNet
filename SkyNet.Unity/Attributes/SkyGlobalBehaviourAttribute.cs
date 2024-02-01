using System;
using SkyNet;

public sealed class SkyGlobalBehaviourAttribute : Attribute
{
    public NetworkModes Mode { get; private set; }

    public string[] Scenes { get; private set; }

    public SkyGlobalBehaviourAttribute() : this(NetworkModes.Shutdown)
    {
    }

    public SkyGlobalBehaviourAttribute(NetworkModes mode) : this(mode, new string[0])
    {
    }

    public SkyGlobalBehaviourAttribute(params string[] scenes): this(NetworkModes.Shutdown, scenes)
    {
    }

    public SkyGlobalBehaviourAttribute(NetworkModes mode, params string[] scenes)
    {
        Mode = mode;
        Scenes = scenes;
    }
}