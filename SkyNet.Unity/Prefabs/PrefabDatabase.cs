using System.Collections.Generic;
using UnityEngine;

namespace SkyNet
{
    public class PrefabDatabase : ScriptableObject
    {
        private static PrefabDatabase m_instance;

        [SerializeField]
        internal List<GameObject> Prefabs = new List<GameObject>();

        public static PrefabDatabase Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = (PrefabDatabase)Resources.Load("PrefabDatabase", typeof(PrefabDatabase));
                    if (m_instance == null)
                        SkyLog.Error("Could not find resource 'PrefabDatabase'");
                }
                return m_instance;
            }
        }

        public static GameObject Find(PrefabId id)
        {
            //if (Instance.Prefabs == null || Instance.Prefabs.Count <= 0)
            //    UpdateLookup();

            GameObject gameObject = null;

            bool found = false;
            for (int i = 0; i < Instance.Prefabs.Count; i++)
            {
                if(Instance.Prefabs[i].GetComponent<SkyEntity>().m_prefabId == id.Value)
                {
                    gameObject = Instance.Prefabs[i];
                    found = true;
                    break;
                }
            }

            if (!found)
                SkyLog.Error("Could not find game object for " + id);

            return gameObject;
        }

        //internal static void UpdateLookup()
        //{
        //    m_lookup = new Dictionary<PrefabId, GameObject>();
        //    for (int i = 0; i < Instance.Prefabs.Count; ++i)
        //    {
        //        PrefabId prefabId = Instance.Prefabs[i].GetComponent<SkyEntity>().prefabId;
        //        if (m_lookup.ContainsKey(prefabId))
        //        {
        //            SkyLog.Error("Duplicate " + prefabId + " for " + Instance.Prefabs[i].GetComponent<SkyEntity>() + " and " + m_lookup[prefabId].GetComponent<SkyEntity>());
        //            continue;
        //        }
        //        m_lookup.Add(prefabId, Instance.Prefabs[i]);
        //    }
        //}

        internal static bool Contains(SkyEntity entity)
        {
            for (int i = 0; i < Instance.Prefabs.Count; i++)
            {
                if (Instance.Prefabs[i].GetComponent<SkyEntity>().m_prefabId == entity.m_prefabId)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
