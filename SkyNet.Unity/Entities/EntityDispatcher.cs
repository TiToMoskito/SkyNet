using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkyNet
{
    public class EntityDispatcher
    {
        //Dictionary of all created net objects
        private Dictionary<NetworkId, Entity> m_netObjects = new Dictionary<NetworkId, Entity>();
        private List<Entity> m_netObjectsList = new List<Entity>();
        //Dictionary of all local created net objects
        private Dictionary<NetworkId, Entity> m_netOwnObjects = new Dictionary<NetworkId, Entity>();
        private List<Entity> m_netOwnObjectsList = new List<Entity>();

        public void Dispatch(NetworkId _netID, NetBuffer _packer)
        {
            Entity entity = null;
            m_netObjects.TryGetValue(_netID, out entity);
            if (entity == null)
                return;

            entity.Serializer.Unpack(_packer);
        }

        public void Simulate()
        {
            for (int i = 0; i < m_netOwnObjectsList.Count; i++)
            {
                m_netOwnObjectsList[i].Simulate();
            }

            for (int i = 0; i < m_netObjectsList.Count; i++)
            {
                m_netObjectsList[i].SimulateRemote();
            }
        }

        public SkyEntity Find(NetworkId _netID)
        {
            Entity entity = null;
            m_netObjects.TryGetValue(_netID, out entity);

            if(entity == null)
                m_netOwnObjects.TryGetValue(_netID, out entity);

            return entity.UnityObject;
        }

        public void AddRemote(Entity _entity)
        {
            m_netObjects.Add(_entity.NetworkId, _entity);
            m_netObjectsList.Add(_entity);
        }

        public void AddLocal(Entity _entity)
        {
            m_netOwnObjects.Add(_entity.NetworkId, _entity);
            m_netOwnObjectsList.Add(_entity);
        }

        public void Update(NetworkId _tempNetID, NetworkId _netID)
        {
            Entity entity = null;
            m_netOwnObjects.TryGetValue(_tempNetID, out entity);
            if (entity == null)
                return;
            
            entity.NetworkId = _netID;
        }

        public void Remove(NetworkId _netID)
        {
            Entity entity = null;
            m_netObjects.TryGetValue(_netID, out entity);
            if (entity == null)
                return;
            Object.Destroy(entity.UnityObject.gameObject);
            m_netObjects.Remove(_netID);
            m_netObjectsList.Remove(entity);
        }

        public void RemoveAll()
        {
            for (int i = 0; i < m_netObjects.Count; i++)
            {
                if (m_netObjects.ElementAt(i).Value.UnityObject != null)
                    Object.Destroy(m_netObjects.ElementAt(i).Value.UnityObject.gameObject);
            }

            for (int i = 0; i < m_netOwnObjects.Count; i++)
            {
                if(m_netOwnObjects.ElementAt(i).Value.UnityObject != null)
                    Object.Destroy(m_netOwnObjects.ElementAt(i).Value.UnityObject.gameObject);
            }

            m_netObjects.Clear();
            m_netObjectsList.Clear();
            m_netOwnObjects.Clear();
            m_netOwnObjectsList.Clear();
        }
    }
}