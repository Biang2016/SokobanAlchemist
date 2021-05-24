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

    public Dictionary<ushort, int> WarmUpConfigDict_ushort = new Dictionary<ushort, int>();

    public Dictionary<PrefabNames, int> PoolConfigs = new Dictionary<PrefabNames, int>();

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
        foreach (PrefabNames prefabName in Enum.GetValues(typeof(PrefabNames)))
        {
            string prefabNameStr = prefabName.ToString();
            GameObject go_Prefab = PrefabManager.Instance.GetPrefab(prefabNameStr);
            if (go_Prefab)
            {
                GameObject go = new GameObject("Pool_" + prefabName);
                GameObjectPool pool = go.AddComponent<GameObjectPool>();
                pool.transform.SetParent(Root);
                PoolDict.Add(prefabName, pool);
                PoolObject po = go_Prefab.GetComponent<PoolObject>();
                pool.Initiate(po, po.WarmUpInstanceNum);
                PoolConfigs.Add(prefabName, po.WarmUpInstanceNum);
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
                PoolObject po = go_Prefab.GetComponent<PoolObject>();
                pool.Initiate(po, po.WarmUpInstanceNum);
                WarmUpConfigDict_ushort.Add(kv.Key, po.WarmUpInstanceNum);
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
                pool.Initiate(po, po.WarmUpInstanceNum);
                WarmUpConfigDict_ushort.Add(kv.Key, po.WarmUpInstanceNum);
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
                pool.Initiate(po, po.WarmUpInstanceNum);
                WarmUpConfigDict_ushort.Add(kv.Key, po.WarmUpInstanceNum);
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
                pool.Initiate(po, po.WarmUpInstanceNum);
                WarmUpConfigDict_ushort.Add(kv.Key, po.WarmUpInstanceNum);
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
                pool.Initiate(po, po.WarmUpInstanceNum);
                WarmUpConfigDict_ushort.Add(kv.Key, po.WarmUpInstanceNum);
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
                pool.Initiate(po, po.WarmUpInstanceNum);
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
                pool.Initiate(po, po.WarmUpInstanceNum);
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
                pool.Initiate(po, po.WarmUpInstanceNum);
            }
        }

        IsInit = true;
    }

    private bool alreadyWarmedUp = false;

    public IEnumerator WarmUpPool()
    {
        if (alreadyWarmedUp) yield break;
        int warmUpPerFrame = 0;
        int totalWarmUpTask = 0;
        int totalWarmUpTaskCount = 0;
        foreach (KeyValuePair<PrefabNames, GameObjectPool> kv in PoolDict) totalWarmUpTask += PoolConfigs[kv.Key] * 2;
        foreach (KeyValuePair<ushort, GameObjectPool> kv in BoxDict) totalWarmUpTask += WarmUpConfigDict_ushort[kv.Key] * 2;
        foreach (KeyValuePair<ushort, GameObjectPool> kv in ActorDict) totalWarmUpTask += WarmUpConfigDict_ushort[kv.Key] * 2;
        foreach (KeyValuePair<ushort, GameObjectPool> kv in CollectableItemDict) totalWarmUpTask += WarmUpConfigDict_ushort[kv.Key] * 2;
        foreach (KeyValuePair<ushort, GameObjectPool> kv in FXDict) totalWarmUpTask += WarmUpConfigDict_ushort[kv.Key] * 2;
        foreach (KeyValuePair<ushort, GameObjectPool> kv in BattleIndicatorDict) totalWarmUpTask += WarmUpConfigDict_ushort[kv.Key] * 2;

        foreach (KeyValuePair<PrefabNames, GameObjectPool> kv in PoolDict)
        {
            int warmUpNum = PoolConfigs[kv.Key];
            PoolObject[] warmUpPoolObjects = new PoolObject[warmUpNum];
            for (int i = 0; i < warmUpNum; i++)
            {
                PoolObject warmUpPoolObject = kv.Value.AllocateGameObject<PoolObject>(null);
                warmUpPoolObjects[i] = warmUpPoolObject;
                warmUpPerFrame++;
                totalWarmUpTaskCount++;
                if (warmUpPerFrame > 256)
                {
                    warmUpPerFrame = 0;
                    ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.01f + 0.49f * totalWarmUpTaskCount / totalWarmUpTask, "Warm Up Pool");
                    yield return null;
                }
            }

            for (int i = 0; i < warmUpNum; i++)
            {
                warmUpPoolObjects[i].PoolRecycle();
                warmUpPerFrame++;
                totalWarmUpTaskCount++;
                if (warmUpPerFrame > 256)
                {
                    warmUpPerFrame = 0;
                    ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.01f + 0.49f * totalWarmUpTaskCount / totalWarmUpTask, "Warm Up Pool");
                    yield return null;
                }
            }
        }

        foreach (KeyValuePair<ushort, GameObjectPool> kv in BoxDict)
        {
            int warmUpNum = WarmUpConfigDict_ushort[kv.Key];
            Box[] warmUpBoxes = new Box[warmUpNum];
            for (int i = 0; i < warmUpNum; i++)
            {
                Box warmUpBox = kv.Value.AllocateGameObject<Box>(null);
                warmUpBoxes[i] = warmUpBox;
                warmUpBox.Setup(new EntityData(kv.Key, GridPosR.Orientation.Up), GridPos3D.Zero);
                warmUpBox.BoxColliderHelper.OnHelperRecycled(); // 防止Collider过多重叠
                warmUpBox.EntityIndicatorHelper.OnHelperRecycled(); // 防止Collider过多重叠
                warmUpPerFrame++;
                totalWarmUpTaskCount++;
                if (warmUpPerFrame > 256)
                {
                    warmUpPerFrame = 0;
                    ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.01f + 0.49f * totalWarmUpTaskCount / totalWarmUpTask, "Warm Up Pool");
                    yield return null;
                }
            }

            for (int i = 0; i < warmUpNum; i++)
            {
                warmUpBoxes[i].PoolRecycle();
                warmUpPerFrame++;
                totalWarmUpTaskCount++;
                if (warmUpPerFrame > 256)
                {
                    warmUpPerFrame = 0;
                    ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.01f + 0.49f * totalWarmUpTaskCount / totalWarmUpTask, "Warm Up Pool");
                    yield return null;
                }
            }
        }

        foreach (KeyValuePair<ushort, GameObjectPool> kv in ActorDict)
        {
            int warmUpNum = WarmUpConfigDict_ushort[kv.Key];
            Actor[] warmUpActors = new Actor[warmUpNum];
            for (int i = 0; i < warmUpNum; i++)
            {
                Actor warmUpActor = kv.Value.AllocateGameObject<Actor>(null);
                warmUpActors[i] = warmUpActor;
                foreach (Collider collider in warmUpActor.ActorMoveColliders) // 防止Collider过多重叠
                {
                    collider.enabled = false;
                }

                warmUpActor.EntityIndicatorHelper.OnHelperRecycled(); // 防止Collider过多重叠
                warmUpPerFrame++;
                totalWarmUpTaskCount++;
                if (warmUpPerFrame > 256)
                {
                    warmUpPerFrame = 0;
                    ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.01f + 0.49f * totalWarmUpTaskCount / totalWarmUpTask, "Warm Up Pool");
                    yield return null;
                }
            }

            for (int i = 0; i < warmUpNum; i++)
            {
                warmUpActors[i].PoolRecycle();
                warmUpPerFrame++;
                totalWarmUpTaskCount++;
                if (warmUpPerFrame > 256)
                {
                    warmUpPerFrame = 0;
                    ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.01f + 0.49f * totalWarmUpTaskCount / totalWarmUpTask, "Warm Up Pool");
                    yield return null;
                }
            }
        }

        foreach (KeyValuePair<ushort, GameObjectPool> kv in CollectableItemDict)
        {
            int warmUpNum = WarmUpConfigDict_ushort[kv.Key];
            CollectableItem[] warmUpCollectableItems = new CollectableItem[warmUpNum];
            for (int i = 0; i < warmUpNum; i++)
            {
                CollectableItem warmUpCollectableItem = kv.Value.AllocateGameObject<CollectableItem>(null);
                warmUpCollectableItems[i] = warmUpCollectableItem;
                warmUpPerFrame++;
                totalWarmUpTaskCount++;
                if (warmUpPerFrame > 256)
                {
                    warmUpPerFrame = 0;
                    ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.01f + 0.49f * totalWarmUpTaskCount / totalWarmUpTask, "Warm Up Pool");
                    yield return null;
                }
            }

            for (int i = 0; i < warmUpNum; i++)
            {
                warmUpCollectableItems[i].PoolRecycle();
                warmUpPerFrame++;
                totalWarmUpTaskCount++;
                if (warmUpPerFrame > 256)
                {
                    warmUpPerFrame = 0;
                    ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.01f + 0.49f * totalWarmUpTaskCount / totalWarmUpTask, "Warm Up Pool");
                    yield return null;
                }
            }
        }

        foreach (KeyValuePair<ushort, GameObjectPool> kv in FXDict)
        {
            int warmUpNum = WarmUpConfigDict_ushort[kv.Key];
            FX[] warmUpFXs = new FX[warmUpNum];
            for (int i = 0; i < warmUpNum; i++)
            {
                FX warmUpFX = kv.Value.AllocateGameObject<FX>(null);
                warmUpFXs[i] = warmUpFX;
                warmUpPerFrame++;
                totalWarmUpTaskCount++;
                if (warmUpPerFrame > 256)
                {
                    warmUpPerFrame = 0;
                    ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.01f + 0.49f * totalWarmUpTaskCount / totalWarmUpTask, "Warm Up Pool");
                    yield return null;
                }
            }

            for (int i = 0; i < warmUpNum; i++)
            {
                warmUpFXs[i].PoolRecycle();
                warmUpPerFrame++;
                totalWarmUpTaskCount++;
                if (warmUpPerFrame > 256)
                {
                    warmUpPerFrame = 0;
                    ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.01f + 0.49f * totalWarmUpTaskCount / totalWarmUpTask, "Warm Up Pool");
                    yield return null;
                }
            }
        }

        foreach (KeyValuePair<ushort, GameObjectPool> kv in BattleIndicatorDict)
        {
            int warmUpNum = WarmUpConfigDict_ushort[kv.Key];
            BattleIndicator[] warmUpBattleIndicators = new BattleIndicator[warmUpNum];
            for (int i = 0; i < warmUpNum; i++)
            {
                BattleIndicator warmUpBattleIndicator = kv.Value.AllocateGameObject<BattleIndicator>(null);
                warmUpBattleIndicators[i] = warmUpBattleIndicator;
                warmUpPerFrame++;
                totalWarmUpTaskCount++;
                if (warmUpPerFrame > 256)
                {
                    warmUpPerFrame = 0;
                    ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.01f + 0.49f * totalWarmUpTaskCount / totalWarmUpTask, "Warm Up Pool");
                    yield return null;
                }
            }

            for (int i = 0; i < warmUpNum; i++)
            {
                warmUpBattleIndicators[i].PoolRecycle();
                warmUpPerFrame++;
                totalWarmUpTaskCount++;
                if (warmUpPerFrame > 256)
                {
                    warmUpPerFrame = 0;
                    ClientGameManager.Instance.LoadingMapPanel.SetProgress(0.01f + 0.49f * totalWarmUpTaskCount / totalWarmUpTask, "Warm Up Pool");
                    yield return null;
                }
            }
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