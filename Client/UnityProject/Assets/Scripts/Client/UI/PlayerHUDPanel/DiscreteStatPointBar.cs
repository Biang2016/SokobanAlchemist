using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscreteStatPointBar : MonoBehaviour
{
    public Transform PointIndicatorContainer;
    private List<DiscreteStatPointIndicator> PointIndicators = new List<DiscreteStatPointIndicator>();

    private EntityStatType EntityStatType;
    private int AmountPerGrid;

    public void Initialize(EntityStatType entityStatType, int amountPerGrid)
    {
        EntityStatType = entityStatType;
        AmountPerGrid = amountPerGrid;
    }

    public void SetStat(int current, int min, int max)
    {
        int currentGrid = current / AmountPerGrid;
        float lastRatio = (float) (current - currentGrid * AmountPerGrid) / AmountPerGrid;
        if (lastRatio > 0) currentGrid += 1;
        int maxGrid = max / AmountPerGrid;
        float maxLastRatio = (float) (max - maxGrid * AmountPerGrid) / AmountPerGrid;
        if (maxLastRatio > 0) maxGrid += 1;

        if (PointIndicators.Count > maxGrid)
        {
            while (PointIndicators.Count != maxGrid)
            {
                PointIndicators[PointIndicators.Count - 1].PoolRecycle();
                PointIndicators.RemoveAt(PointIndicators.Count - 1);
            }
        }
        else if (PointIndicators.Count < maxGrid)
        {
            while (PointIndicators.Count != maxGrid)
            {
                DiscreteStatPointIndicator indicator = GameObjectPoolManager.Instance.PoolDict[GameObjectPoolManager.PrefabNames.DiscreteStatPointIndicator].AllocateGameObject<DiscreteStatPointIndicator>(PointIndicatorContainer);
                indicator.Initialize(EntityStatType);
                PointIndicators.Add(indicator);
            }
        }

        for (int i = 0; i < PointIndicators.Count; i++)
        {
            PointIndicators[i].Available = i < currentGrid;
            if (i < currentGrid - 1)
            {
                PointIndicators[i].Ratio = 1f;
            }
            else if(i >= currentGrid)
            {
                PointIndicators[i].Ratio = 0f;
            }

            if (i == currentGrid - 1)
            {
                if (lastRatio > 0)
                {
                    PointIndicators[i].Ratio = lastRatio;
                }
                else
                {
                    PointIndicators[i].Ratio = 1f;
                }
            }

            if (i == maxGrid - 1 && maxLastRatio > 0)
            {
                if (maxLastRatio > 0)
                {
                    PointIndicators[i].Ratio = maxLastRatio;
                }
                else
                {
                    PointIndicators[i].Ratio = 1f;
                }
            }

            PointIndicators[i].Full = AmountPerGrid > 1 && ((i < currentGrid - 1 && lastRatio > 0) || (i < currentGrid && lastRatio.Equals(0)));
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