using SkyNet;
using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class SkyEntity : MonoBehaviour
{
    internal Entity m_entity;
    [SerializeField]
    internal string m_sceneGuid;
    [SerializeField]
    internal string m_serializerGuid;
    [SerializeField]
    internal int m_prefabId;
    [SerializeField]
    internal float m_updateRate = 1f;
    [SerializeField]
    internal bool m_persistThroughSceneLoads;

    internal Entity Entity
    {
        get
        {
            if (m_entity == null)
                throw new System.Exception("You can't access any SkyEntity specific methods or properties on an entity which is detached");
            return m_entity;
        }
    }

    public UniqueId sceneGuid
    {
        get
        {
            return UniqueId.Parse(m_sceneGuid);
        }
        set
        {
            m_sceneGuid = value.IdString;
        }
    }

    public UniqueId serializerGuid
    {
        get
        {
            return UniqueId.Parse(m_serializerGuid);
        }
        set
        {
            m_serializerGuid = value.IdString;
        }
    }

    public PrefabId prefabId
    {
        get
        {
            return PrefabId.Parse(m_prefabId);
        }
        set
        {
            m_prefabId = value.Value;
        }
    }

    public NetworkId networkId
    {
        get
        {
            return Entity.NetworkId;
        }
    }

    public Connection source
    {
        get
        {
            return Entity.Source;
        }
    }

    public Connection controller
    {
        get
        {
            return Entity.Controller;
        }
    }

    public bool canFreeze
    {
        get
        {
            return Entity.CanFreeze;
        }
        set
        {
            Entity.CanFreeze = value;
        }
    }

    public bool isAttached
    {
        get
        {
            return m_entity != null && m_entity.IsAttached;
        }
    }

    public bool isOwner
    {
        get
        {
            return Entity.IsOwner;
        }
    }

    public bool hasControl
    {
        get
        {
            return Entity.HasControl;
        }
    }

    public bool isControlled
    {
        get
        {
            return hasControl || Entity.Controller != null;
        }
    }

    public bool isControllerOrOwner
    {
        get
        {
            return hasControl || isOwner;
        }
    }

    public bool isFrozen
    {
        get
        {
            return Entity.IsFrozen;
        }
    }

    public bool isSceneObject
    {
        get
        {
            return Entity.IsSceneObject;
        }
    }

    public bool persistsOnSceneLoad
    {
        get
        {
            return Entity.PersistsOnSceneLoad;
        }
    }

    public void TakeControl()
    {
        Entity.TakeControl(null);
    }

    public void TakeControl(IProtocolToken token)
    {
        Entity.TakeControl(token);
    }

    public void ReleaseControl()
    {
        Entity.ReleaseControl(null);
    }

    public void ReleaseControl(IProtocolToken token)
    {
        Entity.ReleaseControl(token);
    }

    public void AssignControl(Connection connection)
    {
        Entity.AssignControl(connection, null);
    }

    public void AssignControl(Connection connection, IProtocolToken token)
    {
        Entity.AssignControl(connection, token);
    }

    public void RevokeControl()
    {
        Entity.RevokeControl(null);
    }

    public void RevokeControl(IProtocolToken token)
    {
        Entity.RevokeControl(token);
    }

    public bool isController(Connection connection)
    {
        return ReferenceEquals(Entity.Controller, connection);
    }

    public TState GetState<TState>()
    {
        if (Entity.Serializer is TState)
            return (TState)Entity.Serializer;
        SkyLog.Error("You are trying to access the state of {0} as '{1}'", this, typeof(TState));
        return default(TState);
    }

    public void Freeze(bool pause)
    {
        Entity.Freeze(pause);
    }

    internal void StartSynchronize()
    {
        StartCoroutine(Synchronize());
    }

    internal void StopSynchronize()
    {
        StopCoroutine(Synchronize());
    }

    internal IEnumerator Synchronize()
    {
        for (; ; )
        {
            Entity.Serializer.OnSynchronize();
            yield return new WaitForSeconds((1f / (Config.instance.sendRate * m_updateRate)));
        }
    }

    public override string ToString()
    {
        return string.Format("[Entity {0} {1}]", networkId, Entity.Serializer);
    }

    private void Awake()
    {
        if (!Application.isEditor || Application.isPlaying || !(sceneGuid == UniqueId.None))
            return;
        sceneGuid = UniqueId.New();
    }

    //private void OnDisable()
    //{
    //    if (!Application.isPlaying)
    //        return;
    //    OnDestroy();
    //}

    private void OnDestroy()
    {
        if (!Application.isPlaying)
            return;
        if (m_entity == null)
            return;
        m_entity.Detach();
        m_entity = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "SkyEntity", true);
    }
}