using UnityEngine;
using UnityEditor;
using System.IO;

namespace SkyNet.Editor
{
    public static class Extensions
    {
        public static GameObject Find(this PrefabDatabase _pDB, string _name)
        {
            for (int i = 0; i < _pDB.Prefabs.Count; i++)
            {
                if (Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(_pDB.Prefabs[i])) == _name)
                {
                    return _pDB.Prefabs[i];
                }
            }
            return null;
        }

        public static string[] GetNames(this PrefabDatabase _pDB)
        {
            string[] names = new string[0];
            if (_pDB.Prefabs.Count == 0)
                return names;

            names = new string[_pDB.Prefabs.Count];
            for (int i = 0; i < _pDB.Prefabs.Count; i++)
            {
                names[i] = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(_pDB.Prefabs[i]));
            }
            return names;
        }
    }
}
