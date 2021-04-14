using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using UnityEngine;

public class BoxFrozenBoxHelper : BoxMonoHelper
{
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
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
    }

    public void GenerateBoxIndicatorForFrozenActor(Actor frozenActor)
    {
        foreach (GridPos3D offset in frozenActor.GetEntityOccupationGPs_Rotated())
        {
            BoxIndicator boxIndicator = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.BoxIndicator].AllocateGameObject<BoxIndicator>(Box.BoxIndicatorHelper.transform);
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
}