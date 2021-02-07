﻿using System;
using System.Collections;
using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.GamePlay;
using BiangLibrary.ObjectPool;
using BiangLibrary.Singleton;
using UnityEngine;

public class GameObjectPoolManager : TSingletonBaseManager<GameObjectPoolManager>
{
    public enum PrefabNames
    {
        Player,
        InGameHealthBar,
        WorldModule,
        World,
        OpenWorldModule,
        OpenWorld,
        WorldWallCollider,
        WorldGroundCollider,
        WorldDeadZoneTrigger,
        BoxEffectHelper,
        BattleIndicator,
        DebugPanelColumn,
        DebugPanelButton,
        DebugPanelSlider,
    }

    public Dictionary<PrefabNames, int> PoolConfigs = new Dictionary<PrefabNames, int>
    {
        {PrefabNames.Player, 1},
        {PrefabNames.InGameHealthBar, 10},
        {PrefabNames.WorldModule, 16},
        {PrefabNames.World, 1},
        {PrefabNames.OpenWorldModule, 1},
        {PrefabNames.OpenWorld, 1},
        {PrefabNames.WorldWallCollider, 5},
        {PrefabNames.WorldGroundCollider, 5},
        {PrefabNames.WorldDeadZoneTrigger, 5},
        {PrefabNames.BoxEffectHelper, 10},
        {PrefabNames.BattleIndicator, 10},
        {PrefabNames.DebugPanelColumn, 4},
        {PrefabNames.DebugPanelButton, 4},
        {PrefabNames.DebugPanelSlider, 4},
    };

    public Dictionary<string, int> WarmUpBoxConfig = new Dictionary<string, int>
    {
        {"BrickBox", 500},
        {"WoodenBox", 500},
        {"BorderBox", 100},
    };

    public Dictionary<PrefabNames, int> PoolWarmUpDict = new Dictionary<PrefabNames, int>
    {
    };

    public Dictionary<PrefabNames, GameObjectPool> PoolDict = new Dictionary<PrefabNames, GameObjectPool>();
    public Dictionary<ushort, GameObjectPool> BoxDict = new Dictionary<ushort, GameObjectPool>();
    public Dictionary<ushort, GameObjectPool> EnemyDict = new Dictionary<ushort, GameObjectPool>();
    public Dictionary<ushort, GameObjectPool> LevelTriggerDict = new Dictionary<ushort, GameObjectPool>();
    public Dictionary<ushort, GameObjectPool> FXDict = new Dictionary<ushort, GameObjectPool>();
    public Dictionary<ushort, GameObjectPool> BattleIndicatorDict = new Dictionary<ushort, GameObjectPool>();
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

        foreach (KeyValuePair<ushort, string> kv in ConfigManager.BoxTypeDefineDict.TypeNameDict)
        {
            string prefabName = kv.Value;
            GameObject go_Prefab = PrefabManager.Instance.GetPrefab(prefabName);
            if (go_Prefab)
            {
                GameObject go = new GameObject("Pool_" + prefabName);
                GameObjectPool pool = go.AddComponent<GameObjectPool>();
                pool.transform.SetParent(Root);
                BoxDict.Add(kv.Key, pool);
                Box box = go_Prefab.GetComponent<Box>();
                int warmUpNum = 0;
                if (WarmUpBoxConfig.ContainsKey(kv.Value))
                {
                    warmUpNum = WarmUpBoxConfig[kv.Value];
                }
                else
                {
                    warmUpNum = 20;
                }

                pool.Initiate(box, warmUpNum);
            }
        }

        foreach (KeyValuePair<ushort, string> kv in ConfigManager.EnemyTypeDefineDict.TypeNameDict)
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

        foreach (KeyValuePair<ushort, string> kv in ConfigManager.LevelTriggerTypeDefineDict.TypeNameDict)
        {
            string prefabName = kv.Value;
            GameObject go_Prefab = PrefabManager.Instance.GetPrefab(prefabName);
            if (go_Prefab)
            {
                GameObject go = new GameObject("Pool_" + prefabName);
                GameObjectPool pool = go.AddComponent<GameObjectPool>();
                pool.transform.SetParent(Root);
                LevelTriggerDict.Add(kv.Key, pool);
                PoolObject po = go_Prefab.GetComponent<PoolObject>();
                pool.Initiate(po, 20);
            }
        }

        foreach (KeyValuePair<ushort, string> kv in ConfigManager.BattleIndicatorTypeDefineDict.TypeNameDict)
        {
            string prefabName = kv.Value;
            GameObject go_Prefab = PrefabManager.Instance.GetPrefab(prefabName);
            if (go_Prefab)
            {
                GameObject go = new GameObject("Pool_" + prefabName);
                GameObjectPool pool = go.AddComponent<GameObjectPool>();
                pool.transform.SetParent(Root);
                BattleIndicatorDict.Add(kv.Key, pool);
                PoolObject po = go_Prefab.GetComponent<PoolObject>();
                pool.Initiate(po, 20);
            }
        }

        foreach (KeyValuePair<ushort, string> kv in ConfigManager.FXTypeDefineDict.TypeNameDict)
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

    public IEnumerator WarmUpPool()
    {
        int count = 0;
        foreach (KeyValuePair<ushort, string> kv in ConfigManager.BoxTypeDefineDict.TypeNameDict)
        {
            GameObjectPool pool = BoxDict[kv.Key];
            int warmUpNum = 0;
            if (WarmUpBoxConfig.ContainsKey(kv.Value))
            {
                warmUpNum = WarmUpBoxConfig[kv.Value];
            }
            else
            {
                warmUpNum = 50;
            }

            Box[] warmUpBoxes = new Box[warmUpNum];
            for (int i = 0; i < warmUpNum; i++)
            {
                Box warmUpBox = pool.AllocateGameObject<Box>(null);
                warmUpBoxes[i] = warmUpBox;
                warmUpBox.BoxColliderHelper.OnBoxPoolRecycled(); // 防止Collider过多重叠
                count++;
                if (count > 256)
                {
                    count = 0;
                    yield return null;
                }
            }

            for (int i = 0; i < warmUpNum; i++)
            {
                warmUpBoxes[i].PoolRecycle();
                count++;
                if (count > 256)
                {
                    count = 0;
                    yield return null;
                }
            }
        }

        WorldModule[] worldModule = new WorldModule[20];
        for (int i = 0; i < 20; i++)
        {
            GameObjectPool pool = PoolDict[PrefabNames.WorldModule];
            WorldModule module = pool.AllocateGameObject<WorldModule>(null);
            worldModule[i] = module;
        }

        yield return null;
        for (int i = 0; i < 20; i++)
        {
            worldModule[i].PoolRecycle();
        }
    }

    public void OptimizeAllGameObjectPools()
    {
        foreach (KeyValuePair<PrefabNames, GameObjectPool> kv in PoolDict)
        {
            kv.Value.OptimizePool();
        }
    }
}