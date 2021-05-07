using System;
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
        InGameHealthBar,
        BoxIndicator,
        WorldModule,
        World,
        OpenWorldModule,
        OpenWorld,
        WorldZoneTrigger,
        WorldDeadZoneTrigger,
        WorldWallCollider,
        WorldGroundCollider,
        BoxTrail,
        BattleIndicator,
        EntityLightning,
        DiscreteStatPointIndicator,
        MarchingSquareTerrainTile,
        DebugPanelColumn,
        DebugPanelButton,
        DebugPanelSlider,
        EntitySkillRow,
        LearnSkillUpgradePage,
    }

    public Dictionary<PrefabNames, int> PoolConfigs = new Dictionary<PrefabNames, int>
    {
        {PrefabNames.InGameHealthBar, 20},
        {PrefabNames.BoxIndicator, 20},
        {PrefabNames.WorldModule, 16},
        {PrefabNames.World, 1},
        {PrefabNames.OpenWorldModule, 1},
        {PrefabNames.OpenWorld, 1},
        {PrefabNames.WorldZoneTrigger, 16},
        {PrefabNames.WorldDeadZoneTrigger, 16},
        {PrefabNames.WorldWallCollider, 16},
        {PrefabNames.WorldGroundCollider, 16},
        {PrefabNames.BoxTrail, 128},
        {PrefabNames.BattleIndicator, 10},
        {PrefabNames.EntityLightning, 10},
        {PrefabNames.DiscreteStatPointIndicator, 50},
        {PrefabNames.MarchingSquareTerrainTile, 2048},
        {PrefabNames.DebugPanelColumn, 4},
        {PrefabNames.DebugPanelButton, 4},
        {PrefabNames.DebugPanelSlider, 4},
        {PrefabNames.EntitySkillRow, 16},
        {PrefabNames.LearnSkillUpgradePage, 4},
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
    public Dictionary<ushort, GameObjectPool> ActorDict = new Dictionary<ushort, GameObjectPool>();
    public Dictionary<ushort, GameObjectPool> CollectableItemDict = new Dictionary<ushort, GameObjectPool>();
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

        foreach (KeyValuePair<ushort, string> kv in ConfigManager.TypeDefineConfigs[TypeDefineType.Box].TypeNameDict)
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

        foreach (KeyValuePair<ushort, string> kv in ConfigManager.TypeDefineConfigs[TypeDefineType.Actor].TypeNameDict)
        {
            string prefabName = kv.Value;
            GameObject go_Prefab = PrefabManager.Instance.GetPrefab(prefabName);
            if (go_Prefab)
            {
                GameObject go = new GameObject("Pool_" + prefabName);
                GameObjectPool pool = go.AddComponent<GameObjectPool>();
                pool.transform.SetParent(Root);
                ActorDict.Add(kv.Key, pool);
                PoolObject po = go_Prefab.GetComponent<PoolObject>();
                pool.Initiate(po, 20);
            }
        }

        foreach (KeyValuePair<ushort, string> kv in ConfigManager.TypeDefineConfigs[TypeDefineType.CollectableItem].TypeNameDict)
        {
            string prefabName = kv.Value;
            GameObject go_Prefab = PrefabManager.Instance.GetPrefab(prefabName);
            if (go_Prefab)
            {
                GameObject go = new GameObject("Pool_" + prefabName);
                GameObjectPool pool = go.AddComponent<GameObjectPool>();
                pool.transform.SetParent(Root);
                CollectableItemDict.Add(kv.Key, pool);
                PoolObject po = go_Prefab.GetComponent<PoolObject>();
                pool.Initiate(po, 20);
            }
        }

        foreach (KeyValuePair<ushort, string> kv in ConfigManager.TypeDefineConfigs[TypeDefineType.BattleIndicator].TypeNameDict)
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

        foreach (KeyValuePair<ushort, string> kv in ConfigManager.TypeDefineConfigs[TypeDefineType.FX].TypeNameDict)
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

    private bool alreadyWarmedUp = false;

    public IEnumerator WarmUpPool()
    {
        int GetBoxWarmUpNum(KeyValuePair<ushort, string> kv)
        {
            int warmUpNum = 0;
            if (WarmUpBoxConfig.ContainsKey(kv.Value))
            {
                warmUpNum = WarmUpBoxConfig[kv.Value];
            }
            else
            {
                warmUpNum = 50;
            }

            return warmUpNum;
        }

        if (alreadyWarmedUp) yield break;
        int worldModuleWarmUpCount = 20;
        int boxWarmUpPerFrame = 0;
        int totalWarmUpTask = 0;
        int totalWarmUpTaskCount = 0;
        foreach (KeyValuePair<ushort, string> kv in ConfigManager.TypeDefineConfigs[TypeDefineType.Box].TypeNameDict)
        {
            totalWarmUpTask += GetBoxWarmUpNum(kv) * 2;
        }

        totalWarmUpTask += worldModuleWarmUpCount * 2;

        foreach (KeyValuePair<ushort, string> kv in ConfigManager.TypeDefineConfigs[TypeDefineType.Box].TypeNameDict)
        {
            GameObjectPool pool = BoxDict[kv.Key];
            int warmUpNum = GetBoxWarmUpNum(kv);
            Box[] warmUpBoxes = new Box[warmUpNum];
            for (int i = 0; i < warmUpNum; i++)
            {
                Box warmUpBox = pool.AllocateGameObject<Box>(null);
                warmUpBoxes[i] = warmUpBox;
                warmUpBox.BoxColliderHelper.OnBoxPoolRecycled(); // 防止Collider过多重叠
                warmUpBox.EntityIndicatorHelper.OnHelperRecycled(); // 防止Collider过多重叠
                boxWarmUpPerFrame++;
                totalWarmUpTaskCount++;
                if (boxWarmUpPerFrame > 256)
                {
                    boxWarmUpPerFrame = 0;
                    ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.01f + 0.49f * totalWarmUpTaskCount / totalWarmUpTask, "Warm Up Pool");
                    yield return null;
                }
            }

            for (int i = 0; i < warmUpNum; i++)
            {
                warmUpBoxes[i].PoolRecycle();
                boxWarmUpPerFrame++;
                totalWarmUpTaskCount++;
                if (boxWarmUpPerFrame > 256)
                {
                    boxWarmUpPerFrame = 0;
                    ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.01f + 0.49f * totalWarmUpTaskCount / totalWarmUpTask, "Warm Up Pool");
                    yield return null;
                }
            }
        }

        WorldModule[] worldModule = new WorldModule[worldModuleWarmUpCount];
        for (int i = 0; i < worldModuleWarmUpCount; i++)
        {
            GameObjectPool pool = PoolDict[PrefabNames.WorldModule];
            WorldModule module = pool.AllocateGameObject<WorldModule>(null);
            worldModule[i] = module;
            totalWarmUpTaskCount++;
        }

        ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.01f + 0.49f * totalWarmUpTaskCount / totalWarmUpTask, "Warm Up Pool");
        yield return null;
        for (int i = 0; i < worldModuleWarmUpCount; i++)
        {
            worldModule[i].PoolRecycle();
            totalWarmUpTaskCount++;
        }

        ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.01f + 0.49f * totalWarmUpTaskCount / totalWarmUpTask, "Warm Up Pool");
        yield return null;
        alreadyWarmedUp = true;
    }

    public void OptimizeAllGameObjectPools()
    {
        foreach (KeyValuePair<PrefabNames, GameObjectPool> kv in PoolDict)
        {
            kv.Value.OptimizePool();
        }
    }
}