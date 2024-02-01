using System.Linq;
using UnityEngine;

using System.Runtime.CompilerServices;
using System.Collections;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("SkyNet.Editor")]
namespace SkyNet
{
    public class Entity
    {
        internal UniqueId SceneId;
        internal NetworkId NetworkId;
        internal PrefabId PrefabId;
        internal UniqueId SerializerGuid;
        internal EntityFlags Flags;
        internal bool AttachIsRunning;
        internal Vector3 SpawnPosition;
        internal Quaternion SpawnRotation;
        internal Entity Parent;
        internal SkyEntity UnityObject;
        internal Connection Source;
        internal Connection Controller;
        internal IProtocolToken ControlLostToken;
        internal IProtocolToken ControlGainedToken;
        internal IEntitySerializer Serializer;
        internal IEntityBehaviour[] Behaviours;
        internal bool IsOwner;
        internal bool CanFreeze = true;
        internal bool IsFrozen;
        internal int UpdateRate = 1;

        internal bool IsSceneObject
        {
            get
            {
                return Flags == EntityFlags.SCENE_OBJECT;
            }
        }

        internal bool HasParent
        {
            get
            {
                return Parent != null;
            }
        }

        internal bool HasControl
        {
            get
            {
                return Flags == EntityFlags.HAS_CONTROL;
            }
        }

        internal bool IsAttached
        {
            get
            {
                return Flags == EntityFlags.ATTACHED;
            }
        }

        public bool PersistsOnSceneLoad
        {
            get
            {
                return Flags == EntityFlags.PERSIST_ON_LOAD;
            }
        }

        internal void AddEventListener(MonoBehaviour behaviour)
        {
            SkyManager.EventDispatcher.Add(behaviour);
        }

        internal void RemoveEventListener(MonoBehaviour behaviour)
        {
            SkyManager.EventDispatcher.Remove(behaviour);
        }

        internal void Initialize(Connection _owner)
        {
            Source = _owner;
            IsOwner = Source == null ? false : true;
            Behaviours = (UnityObject.GetComponentsInChildren(typeof(IEntityBehaviour))).Select((x => x as IEntityBehaviour)).Where((x => x != null)).ToArray();
            UnityObject.m_entity = this;
            foreach (IEntityBehaviour behaviour in Behaviours)
            {
                if (behaviour.invoke)
                    behaviour.Initialized();
            }
        }

        public void AssignControl(Connection connection)
        {
            AssignControl(connection, null);
        }

        public void AssignControl(Connection connection, IProtocolToken token)
        {
            if (IsOwner)
            {
                if (HasControl)
                    ReleaseControl(token);
                Controller = connection;
                Freeze(false);
            }
            else
                SkyLog.Error("You can not assign control of {0}, you are not the owner", this);
        }

        public void ReleaseControl()
        {
            ReleaseControl(null);
        }

        public void ReleaseControl(IProtocolToken token)
        {
            if (IsOwner)
            {
                if (HasControl)
                {
                    ReleaseControlInternal(token);
                    Freeze(false);
                }
                else
                    SkyLog.Warn("You are not controlling {0}", (object)this);
            }
            else
                SkyLog.Error("You can not release control of {0}, you are not the owner", (object)this);
        }

        internal void ReleaseControlInternal(IProtocolToken token)
        {
            Flags = EntityFlags.HAS_CONTROL;
            ControlLostToken = token;
            ControlGainedToken = null;
            foreach (IEntityBehaviour behaviour in Behaviours)
            {
                behaviour.ControlLost();
            }
            GlobalEventListenerBase.ControlOfEntityLostInvoke(UnityObject);
            Freeze(false);
        }

        public void Freeze(bool freeze)
        {
            if (IsFrozen == freeze)
                return;
            if (IsFrozen)
            {
                IsFrozen = false;
                //._entitiesFZ.Remove(this);
                //._entitiesOK.AddLast(this);
                GlobalEventListenerBase.EntityThawedInvoke(UnityObject);
            }
            else if (CanFreeze)
            {
                IsFrozen = true;
                //._entitiesOK.Remove(this);
                //._entitiesFZ.AddLast(this);
                GlobalEventListenerBase.EntityFrozenInvoke(UnityObject);
            }
        }

        public void TakeControl()
        {
            TakeControl(null);
        }

        public void TakeControl(IProtocolToken token)
        {
            if (IsOwner)
            {
                if (HasControl)
                {
                    SkyLog.Warn("You already have control of {0}", (object)this);
                }
                else
                {
                    RevokeControl(token);
                    TakeControlInternal(token);
                    Freeze(false);
                }
            }
            else
                SkyLog.Error("Only the owner of {0} can take control of it", (object)this);
        }

        internal void TakeControlInternal(IProtocolToken token)
        {
            Flags = EntityFlags.HAS_CONTROL;
            ControlGainedToken = token;
            ControlLostToken = null;
            GlobalEventListenerBase.ControlOfEntityGainedInvoke(UnityObject);
            foreach (IEntityBehaviour behaviour in Behaviours)
            {
                behaviour.ControlGained();
            }
            Freeze(false);
        }

        public void RevokeControl()
        {
            RevokeControl(null);
        }

        public void RevokeControl(IProtocolToken token)
        {
            if (IsOwner)
            {
                if (Controller == null)
                    return;
                Controller = null;
                Freeze(false);
            }
            else
                SkyLog.Error("You can not revoke control of {0}, you are not the owner", (object)this);
        }

        internal void SetParent(Entity entity)
        {
            if (IsOwner)
                SetParentInternal(entity);
            else
                SkyLog.Error("You are not allowed to assign the parent of this entity, only the owner or a controller with local prediction can");
        }

        internal void SetParentInternal(Entity entity)
        {
            if (!(entity != Parent)) return;
            if (entity != null && !entity.IsAttached)
            {
                SkyLog.Error("You can't assign a detached entity as the parent of another entity");
            }
            else
            {
                try
                {
                    Serializer.OnParentChanging(entity, Parent);
                }
                finally
                {
                    Parent = entity;
                }
            }
        }

        internal void Attach()
        {
            if (UnityObject == null) return;
            if (IsAttached) return;            
            try
            {
                AttachIsRunning = true;
                Object.DontDestroyOnLoad(UnityObject.gameObject);

                if (Source != null)
                    SkyManager.EntityDispatcher.AddLocal(this);
                else
                    SkyManager.EntityDispatcher.AddRemote(this);

                Flags |= EntityFlags.ATTACHED;
                foreach (IEntityBehaviour behaviour in Behaviours)
                {
                    try
                    {
                        if (behaviour.invoke && ReferenceEquals(behaviour.entity, UnityObject))
                            behaviour.Attached();
                    }
                    catch (System.Exception ex)
                    {
                        SkyLog.Error("User code threw exception inside Attached callback");
                        SkyLog.Exception(ex);
                    }
                }
                try
                {
                    GlobalEventListenerBase.EntityAttachedInvoke(UnityObject);
                }
                catch (System.Exception ex)
                {
                    SkyLog.Error("User code threw exception inside Attached callback");
                    SkyLog.Exception(ex);
                }
                SkyLog.Debug("Attached {0}", this);
                if (IsOwner) UnityObject.StartSynchronize();
            }
            finally
            {
                AttachIsRunning = false;
            }
        }

        internal void Detach()
        {
            if (UnityObject == null) return;
            if (!IsAttached) return;

            if (Controller != null)
                RevokeControl(null);

            SkyManager.Destroy(this);

            foreach (IEntityBehaviour behaviour in Behaviours)
            {
                try
                {
                    if (behaviour.invoke && ReferenceEquals(behaviour.entity, UnityObject))
                    {
                        behaviour.Detached();
                        behaviour.entity = null;
                    }
                }
                catch (System.Exception ex)
                {
                    SkyLog.Error("User code threw exception inside Detach callback");
                    SkyLog.Exception(ex);
                }
            }
            try
            {
                GlobalEventListenerBase.EntityDetachedInvoke(UnityObject);
            }
            catch (System.Exception ex)
            {
                SkyLog.Error("User code threw exception inside Detach callback");
                SkyLog.Exception(ex);
            }
            Flags &= ~EntityFlags.ATTACHED;
            SkyManager.EntityDispatcher.Remove(NetworkId);
            if (IsOwner) UnityObject.StopSynchronize();
            UnityObject.m_entity = null;
            SkyLog.Debug("Detached {0}", this);
        }

        internal void Simulate()
        {
            for (int i = 0; i < Behaviours.Length; i++)
            {
                if (Behaviours[i] == null)
                    return;

                if (Behaviours[i].invoke)
                {
                    if (IsOwner)
                    {
                        Behaviours[i].SimulateOwner();
                    }
                    else if (HasControl)
                    {
                        Behaviours[i].SimulateController();
                    }
                }
            }
        }      

        internal void SimulateRemote()
        {
            Serializer.OnNotOwner();
            for (int i = 0; i < Behaviours.Length; i++)
            {
                if (Behaviours[i] == null)
                    return;

                if (Behaviours[i].invoke)
                    Behaviours[i].SimulateRemote();
            }
        }

        internal static Entity CreateFor(GameObject instance, PrefabId prefabId, TypeId serializerId, NetworkId _netID)
        {
            return CreateFor(instance, prefabId, serializerId, EntityFlags.ZERO, _netID);
        }

        internal static Entity CreateFor(GameObject instance, PrefabId prefabId, TypeId serializerId, EntityFlags flags, NetworkId _netID)
        {
            Entity entity = new Entity();
            entity.UnityObject = instance.GetComponent<SkyEntity>();
            entity.UpdateRate = (int)entity.UnityObject.m_updateRate;

            entity.PrefabId = prefabId;
            entity.SceneId = entity.UnityObject.sceneGuid;
            entity.NetworkId = _netID;

            entity.Flags = flags;
            if (prefabId.Value == 0)
                entity.Flags = EntityFlags.SCENE_OBJECT;
            if (entity.UnityObject.m_persistThroughSceneLoads)
                entity.Flags = EntityFlags.PERSIST_ON_LOAD;
            entity.Serializer = Factory.NewSerializer(serializerId);
            entity.Serializer.OnCreated(entity);
            return entity;
        }
    }

    public enum EntityFlags
    {
        ZERO,
        HAS_CONTROL,
        ATTACHED,
        PERSIST_ON_LOAD,
        SCENE_OBJECT
    }
}
