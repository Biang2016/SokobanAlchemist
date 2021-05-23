using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using UnityEngine;

public class BoxFrozenBoxHelper : BoxMonoHelper
{
    internal bool InitFrozen = false;
    internal Actor FrozenActor; // EnemyFrozenBox将敌人冻住包裹

    internal List<GridPos3D> FrozenBoxOccupation = new List<GridPos3D>();

    private List<BoxIndicator> FrozenBoxIndicators = new List<BoxIndicator>();

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
        foreach (BoxIndicator frozenBoxIndicator in FrozenBoxIndicators)
        {
            frozenBoxIndicator.PoolRecycle();
        }

        FrozenBoxIndicators.Clear();
        FrozenBoxOccupation = null;
        if (FrozenActor != null)
        {
            FrozenActor.DestroySelfByModuleRecycle();
            FrozenActor = null;
        }

        InitFrozen = false;
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
    }

    public void GenerateBoxIndicatorForFrozenActor(Actor frozenActor)
    {
        foreach (GridPos3D offset in frozenActor.GetEntityOccupationGPs_Rotated())
        {
            BoxIndicator boxIndicator = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.BoxIndicator].AllocateGameObject<BoxIndicator>(Box.EntityIndicatorHelper.transform);
            FrozenBoxIndicators.Add(boxIndicator);
            boxIndicator.transform.position = frozenActor.WorldGP + offset;
        }
    }

    [SerializeField]
    private BoxCollider StaticBoxCollider_OnlyForEnemyFrozenBox;

    [SerializeField]
    private BoxCollider DynamicBoxCollider_OnlyForEnemyFrozenBox;

    [SerializeField]
    private BoxCollider BoxOnlyDynamicCollider_OnlyForEnemyFrozenBox;

    public void SetColliderSize_ForFrozenEnemyBox(int actorWidth)
    {
        StaticBoxCollider_OnlyForEnemyFrozenBox.size = (actorWidth - 0.02f) * Vector3.one;
        DynamicBoxCollider_OnlyForEnemyFrozenBox.size = (actorWidth - 0.2f) * Vector3.one;
        BoxOnlyDynamicCollider_OnlyForEnemyFrozenBox.size = (actorWidth - 0.2f) * Vector3.one;

        StaticBoxCollider_OnlyForEnemyFrozenBox.center = (actorWidth - 1) / 2f * Vector3.one;
        DynamicBoxCollider_OnlyForEnemyFrozenBox.center = (actorWidth - 1) / 2f * Vector3.one;
        BoxOnlyDynamicCollider_OnlyForEnemyFrozenBox.center = (actorWidth - 1) / 2f * Vector3.one;
    }

    public override void ApplyEntityExtraSerializeData(EntityExtraSerializeData entityExtraSerializeData)
    {
        base.ApplyEntityExtraSerializeData(entityExtraSerializeData);
        if (((Box) Entity).WorldModule.IsIniting && entityExtraSerializeData.FrozenActorData != null) // 模组初始化时就带有FrozenActorData的话，需要顺带创建FrozenActor出来。其他情况下一般是由Actor创建出FrozenBox
        {
            bool generateFrozenActorSuc = WorldManager.Instance.CurrentWorld.GenerateFrozenActor(entityExtraSerializeData.FrozenActorData.Clone(), Entity.WorldGP, (Box) Entity, out Actor frozenActor);
            if (!generateFrozenActorSuc) Debug.LogError("生成冻结角色失败");
        }
    }

    public override void RecordEntityExtraSerializeData(EntityExtraSerializeData entityExtraSerializeData)
    {
        base.RecordEntityExtraSerializeData(entityExtraSerializeData);
        if (FrozenActor != null)
        {
            FrozenActor.RecordEntityExtraSerializeData();
            entityExtraSerializeData.FrozenActorData = FrozenActor.CurrentEntityData.Clone();
        }
    }
}