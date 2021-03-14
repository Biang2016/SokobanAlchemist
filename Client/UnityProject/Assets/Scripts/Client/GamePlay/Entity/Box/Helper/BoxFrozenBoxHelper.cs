using System.Collections;
using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using UnityEngine;

public class BoxFrozenBoxHelper : BoxMonoHelper
{
    private List<BoxIndicator> FrozenBoxIndicators = new List<BoxIndicator>();

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
        foreach (BoxIndicator frozenBoxIndicator in FrozenBoxIndicators)
        {
            frozenBoxIndicator.PoolRecycle();
        }

        FrozenBoxIndicators.Clear();
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
}