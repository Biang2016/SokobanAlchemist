using System.Collections.Generic;
using System.Linq;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

public class EntityIndicatorHelper : EntityMonoHelper
{
    [LabelText("占位信息")]
    public EntityOccupationData EntityOccupationData = new EntityOccupationData();

    private List<Collider> IndicatorColliders = new List<Collider>();

    public bool IsSpecialBoxIndicator = false; // MegaGroundBox 专用，为了减少Indicator数量而采取的合并措施

    void Awake()
    {
        IndicatorColliders = GetComponentsInChildren<Collider>().ToList();
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
        if (EntityOccupationData.IsTriggerEntity)
        {
            IsOn = false;
        }
        else
        {
            IsOn = true;
        }
    }

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
        IsOn = false;
    }

    private bool isOn = true;

    public bool IsOn
    {
        get { return isOn; }
        set
        {
            isOn = value;
            foreach (Collider ic in IndicatorColliders)
            {
                ic.enabled = value;
            }

            gameObject.SetActive(value);
        }
    }

#if UNITY_EDITOR
    public void RefreshEntityIndicatorOccupationData()
    {
        EntityOccupationData.Clear();
        if (IsSpecialBoxIndicator)
        {
            for (int x = 0; x < WorldModule.MODULE_SIZE; x++)
            for (int z = 0; z < WorldModule.MODULE_SIZE; z++)
            {
                GridPos3D gp = new GridPos3D(x, 0, z);
                EntityOccupationData.EntityIndicatorGPs.Add(gp);
            }

            EntityOccupationData.IsShapeCuboid = true;
            EntityOccupationData.IsShapePlanSquare = true;
            EntityOccupationData.LocalGeometryCenter = Vector3.zero;
            EntityOccupationData.BoundsInt = EntityOccupationData.EntityIndicatorGPs.GetBoundingRectFromListGridPos(GridPos3D.Zero);
        }
        else
        {
            Collider[] colliders = GetComponentsInChildren<Collider>();
            bool validOccupation = false;
            foreach (Collider c in colliders)
            {
                GridPos3D gp = new GridPos3D(Mathf.RoundToInt(c.transform.position.x), Mathf.RoundToInt(c.transform.position.y), Mathf.RoundToInt(c.transform.position.z));
                EntityOccupationData.EntityIndicatorGPs.Add(gp);
                if (gp == GridPos3D.Zero) validOccupation = true;
            }

            if (!validOccupation)
            {
                Entity entity = gameObject.GetComponentInParent<Entity>();
                Debug.LogWarning($"{entity.name}的占位配置错误，必须要有一个EntityIndicator位于(0,0,0)");
                EntityOccupationData.IsShapeCuboid = false;
                EntityOccupationData.IsShapePlanSquare = false;
                return;
            }

            EntityOccupationData.IsShapeCuboid = false;
            EntityOccupationData.IsShapePlanSquare = false;
            EntityOccupationData.BoundsInt = EntityOccupationData.EntityIndicatorGPs.GetBoundingRectFromListGridPos(GridPos3D.Zero);
            Vector3 localGeometryCenter = Vector3.zero;
            foreach (GridPos3D offset in EntityOccupationData.EntityIndicatorGPs)
            {
                localGeometryCenter += offset;
            }

            EntityOccupationData.LocalGeometryCenter = localGeometryCenter / EntityOccupationData.EntityIndicatorGPs.Count;

            bool[,,] occupationMatrix = new bool[EntityOccupationData.BoundsInt.size.x, EntityOccupationData.BoundsInt.size.y, EntityOccupationData.BoundsInt.size.z];
            foreach (GridPos3D offset in EntityOccupationData.EntityIndicatorGPs)
            {
                occupationMatrix[offset.x - EntityOccupationData.BoundsInt.xMin, offset.y - EntityOccupationData.BoundsInt.yMin, offset.z - EntityOccupationData.BoundsInt.zMin] = true;
            }

            EntityOccupationData.IsShapeCuboid = true;
            EntityOccupationData.IsShapePlanSquare = EntityOccupationData.BoundsInt.size.x == EntityOccupationData.BoundsInt.size.z;
            foreach (bool b in occupationMatrix)
            {
                if (!b)
                {
                    EntityOccupationData.IsShapeCuboid = false;
                    EntityOccupationData.IsShapePlanSquare = false;
                    break;
                }
            }
        }
    }
#endif
}