using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscreteStatPointBar : MonoBehaviour
{
    public Transform PointIndicatorContainer;
    private List<DiscreteStatPointIndicator> PointIndicators = new List<DiscreteStatPointIndicator>();

    private EntityStatType EntityStatType;

    public void Initialize(EntityStatType entityStatType)
    {
        EntityStatType = entityStatType;
    }

    public void SetStat(int current, int min, int max)
    {
        if (PointIndicators.Count > max)
        {
            while (PointIndicators.Count != max)
            {
                PointIndicators[PointIndicators.Count - 1].PoolRecycle();
                PointIndicators.RemoveAt(PointIndicators.Count - 1);
            }
        }
        else if (PointIndicators.Count < max)
        {
            while (PointIndicators.Count != max)
            {
                DiscreteStatPointIndicator indicator = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.DiscreteStatPointIndicator].AllocateGameObject<DiscreteStatPointIndicator>(PointIndicatorContainer);
                indicator.Initialize(EntityStatType);
                PointIndicators.Add(indicator);
            }
        }

        for (int i = 0; i < PointIndicators.Count; i++)
        {
            PointIndicators[i].Available = i < current;
        }
    }

    public void OnStatLowWarning()
    {
        foreach (DiscreteStatPointIndicator indicator in PointIndicators)
        {
            indicator.JumpRed();
        }
    }
}