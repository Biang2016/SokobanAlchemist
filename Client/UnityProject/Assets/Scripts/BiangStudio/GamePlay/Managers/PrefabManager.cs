using System.Collections.Generic;
using BiangStudio.Singleton;
using UnityEngine;

namespace BiangStudio.GamePlay
{
    public class PrefabManager : TSingletonBaseManager<PrefabManager>
    {
        private const string ResourcePath = "Prefabs/";
        private static Dictionary<string, GameObject> PrefabNameDict = new Dictionary<string, GameObject>();

        public bool IsInit = false;

        public override void Awake()
        {
            LoadPrefabs();
            IsInit = true;
        }

        public GameObject GetPrefab(string prefabName)
        {
            if (PrefabNameDict.ContainsKey(prefabName))
            {
                return PrefabNameDict[prefabName];
            }

            return null;
        }

        public void LoadPrefabs()
        {
            ClearPrefabDict();
            GameObject[] prefabs = Resources.LoadAll<GameObject>(ResourcePath);
            foreach (GameObject prefab in prefabs)
            {
                AddPrefabRes(prefab.name, prefab);
            }
        }

        private static void ClearPrefabDict()
        {
            PrefabNameDict.Clear();
        }

        private static void AddPrefabRes(string prefabName, GameObject prefab)
        {
            PrefabNameDict.Add(prefabName, prefab);
        }
    }
}