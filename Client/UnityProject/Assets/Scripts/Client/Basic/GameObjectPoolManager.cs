using System;
using System.Collections.Generic;
using BiangStudio.GamePlay;
using BiangStudio.ObjectPool;
using BiangStudio.Singleton;
using UnityEngine;

public class GameObjectPoolManager : TSingletonBaseManager<GameObjectPoolManager>
{
    public enum PrefabNames
    {
        Player,
        InGameHealthBar,
        WorldCameraPOI,
        WorldModule,
        World,
        WorldWallCollider,
        WorldGroundCollider,
        WorldDeadZoneTrigger,
        BoxEffectHelper,
        DebugPanelColumn,
        DebugPanelButton,
        DebugPanelSlider,
    }

    public Dictionary<PrefabNames, int> PoolConfigs = new Dictionary<PrefabNames, int>
    {
        {PrefabNames.Player, 1},
        {PrefabNames.InGameHealthBar, 10},
        {PrefabNames.WorldCameraPOI, 2},
        {PrefabNames.WorldModule, 16},
        {PrefabNames.World, 1},
        {PrefabNames.WorldWallCollider, 5},
        {PrefabNames.WorldGroundCollider, 5},
        {PrefabNames.WorldDeadZoneTrigger, 5},
        {PrefabNames.BoxEffectHelper, 10},
        {PrefabNames.DebugPanelColumn, 4},
        {PrefabNames.DebugPanelButton, 4},
        {PrefabNames.DebugPanelSlider, 4},
    };

    public Dictionary<PrefabNames, int> PoolWarmUpDict = new Dictionary<PrefabNames, int>
    {
    };

    public Dictionary<PrefabNames, GameObjectPool> PoolDict = new Dictionary<PrefabNames, GameObjectPool>();
    public Dictionary<byte, GameObjectPool> BoxDict = new Dictionary<byte, GameObjectPool>();
    public Dictionary<byte, GameObjectPool> EnemyDict = new Dictionary<byte, GameObjectPool>();
    public Dictionary<byte, GameObjectPool> FXDict = new Dictionary<byte, GameObjectPool>();
    public Dictionary<MarkerType, GameObjectPool> MarkerDict = new Dictionary<MarkerType, GameObjectPool>();
    public Dictionary<ProjectileType, GameObjectPool> ProjectileDict = new Dictionary<ProjectileType, GameObjectPool>();
    public Dictionary<BattleTipPrefabType, GameObjectPool> BattleUIDict = new Dictionary<BattleTipPrefabType, GameObjectPool>();

    private Transform Root;

    public void Init(Transform root)
    {
        Root = root;
    }

    public bool IsInit = false;

    public override void Awake()
    {
        IsInit = true;
        foreach (KeyValuePair<PrefabNames, int> kv in PoolConfigs)
        {
            string prefabName = kv.Key.ToString();
            GameObject go_Prefab = PrefabManager.Instance.GetPrefab(prefabName);
            if (go_Prefab)
            {
                GameObject go = new GameObject("Pool_" + prefabName);
                GameObjectPool pool = go.AddComponent<GameObjectPool>();
                pool.transform.SetParent(Root);
                PoolDict.Add(kv.Key, pool);
                PoolObject po = go_Prefab.GetComponent<PoolObject>();
                pool.Initiate(po, kv.Value);
            }
        }

        foreach (KeyValuePair<byte, string> kv in ConfigManager.BoxTypeDefineDict.TypeNameDict)
        {
            string prefabName = kv.Value;
            GameObject go_Prefab = PrefabManager.Instance.GetPrefab(prefabName);
            if (go_Prefab)
            {
                GameObject go = new GameObject("Pool_" + prefabName);
                GameObjectPool pool = go.AddComponent<GameObjectPool>();
                pool.transform.SetParent(Root);
                BoxDict.Add(kv.Key, pool);
                PoolObject po = go_Prefab.GetComponent<PoolObject>();
                pool.Initiate(po, 20);
            }
        }

        foreach (KeyValuePair<byte, string> kv in ConfigManager.EnemyTypeDefineDict.TypeNameDict)
        {
            string prefabName = kv.Value;
            GameObject go_Prefab = PrefabManager.Instance.GetPrefab(prefabName);
            if (go_Prefab)
            {
                GameObject go = new GameObject("Pool_" + prefabName);
                GameObjectPool pool = go.AddComponent<GameObjectPool>();
                pool.transform.SetParent(Root);
                EnemyDict.Add(kv.Key, pool);
                PoolObject po = go_Prefab.GetComponent<PoolObject>();
                pool.Initiate(po, 20);
            }
        }

        foreach (KeyValuePair<byte, string> kv in ConfigManager.FXTypeDefineDict.TypeNameDict)
        {
            string prefabName = kv.Value;
            GameObject go_Prefab = PrefabManager.Instance.GetPrefab(prefabName);
            if (go_Prefab)
            {
                GameObject go = new GameObject("Pool_" + prefabName);
                GameObjectPool pool = go.AddComponent<GameObjectPool>();
                pool.transform.SetParent(Root);
                FXDict.Add(kv.Key, pool);
                PoolObject po = go_Prefab.GetComponent<PoolObject>();
                pool.Initiate(po, 20);
            }
        }

        foreach (string s in Enum.GetNames(typeof(MarkerType)))
        {
            MarkerType mk_Type = (MarkerType) Enum.Parse(typeof(MarkerType), s);
            GameObject go_Prefab = PrefabManager.Instance.GetPrefab(s);
            if (go_Prefab)
            {
                GameObject go = new GameObject("Pool_" + s);
                GameObjectPool pool = go.AddComponent<GameObjectPool>();
                pool.transform.SetParent(Root);
                MarkerDict.Add(mk_Type, pool);
                PoolObject po = go_Prefab.GetComponent<PoolObject>();
                pool.Initiate(po, 100);
            }
        }

        foreach (string s in Enum.GetNames(typeof(ProjectileType)))
        {
            ProjectileType projectileType = (ProjectileType) Enum.Parse(typeof(ProjectileType), s);
            GameObject go_Prefab = PrefabManager.Instance.GetPrefab(s);
            if (go_Prefab)
            {
                GameObject go = new GameObject("Pool_" + s);
                GameObjectPool pool = go.AddComponent<GameObjectPool>();
                pool.transform.SetParent(Root);
                ProjectileDict.Add(projectileType, pool);
                PoolObject po = go_Prefab.GetComponent<PoolObject>();
                pool.Initiate(po, 20);
            }
        }
       

        foreach (string s in Enum.GetNames(typeof(BattleTipPrefabType)))
        {
            BattleTipPrefabType bt_Type = (BattleTipPrefabType) Enum.Parse(typeof(BattleTipPrefabType), s);
            GameObject go_Prefab = PrefabManager.Instance.GetPrefab(s);
            if (go_Prefab)
            {
                GameObject go = new GameObject("Pool_" + s);
                GameObjectPool pool = go.AddComponent<GameObjectPool>();
                pool.transform.SetParent(Root);
                BattleUIDict.Add(bt_Type, pool);
                PoolObject po = go_Prefab.GetComponent<PoolObject>();
                pool.Initiate(po, 20);
            }
        }

        IsInit = true;
    }

    public void WarmUpPool()
    {
    }

    public void OptimizeAllGameObjectPools()
    {
        foreach (KeyValuePair<PrefabNames, GameObjectPool> kv in PoolDict)
        {
            kv.Value.OptimizePool();
        }
    }
}