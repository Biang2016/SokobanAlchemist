using BiangStudio.GameDataFormat.Grid;
using BiangStudio.ObjectPool;
using UnityEngine;

public class BoxColliderHelper : PoolObject, IBoxHelper
{
    public Box Box;

    [SerializeField]
    private GameObject NormalColliders;

    [SerializeField]
    private GameObject StaticColliders;

    [SerializeField]
    private Collider StaticBoxCollider;

    [SerializeField]
    private Collider StaticWedgeCollider;

    [SerializeField]
    private Collider DynamicCollider;

    [SerializeField]
    private Collider BoxOnlyDynamicCollider;

    [SerializeField]
    private Collider BoxIndicatorTrigger;

    public override void OnRecycled()
    {
        base.OnRecycled();
        StaticBoxCollider.enabled = false;
        StaticWedgeCollider.enabled = false;
        DynamicCollider.enabled = false;
        BoxOnlyDynamicCollider.enabled = false;
        NormalColliders.SetActive(false);
        StaticColliders.SetActive(false);
        BoxOnlyDynamicCollider.gameObject.SetActive(false);
        BoxIndicatorTrigger.gameObject.SetActive(false);
    }

    public void OnBoxPoolRecycle()
    {
        PoolRecycle();
    }

    public override void OnUsed()
    {
        base.OnUsed();
        NormalColliders.SetActive(true);
        StaticColliders.SetActive(true);
        BoxOnlyDynamicCollider.gameObject.SetActive(true);
        BoxIndicatorTrigger.gameObject.SetActive(true);
    }

    public void SwitchBoxShapeType()
    {
        switch (Box.BoxShapeType)
        {
            case BoxShapeType.Box:
            {
                StaticBoxCollider.gameObject.SetActive(true);
                StaticWedgeCollider.gameObject.SetActive(false);
                break;
            }
            case BoxShapeType.Wedge:
            {
                StaticBoxCollider.gameObject.SetActive(false);
                StaticWedgeCollider.gameObject.SetActive(true);
                break;
            }
        }
    }

    public void SwitchBoxOrientation()
    {
        GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, Box.BoxOrientation), StaticColliders.transform, 1);
    }

    public void ResetBoxOrientation()
    {
        GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, GridPosR.Orientation.Up), StaticColliders.transform, 1);
    }

    public void Initialize(bool passable, bool artOnly, bool isGround, bool drop, bool lerp)
    {
        NormalColliders.SetActive(!passable);
        StaticColliders.SetActive(!artOnly);
        DynamicCollider.gameObject.SetActive(!artOnly);
        BoxOnlyDynamicCollider.gameObject.SetActive(!artOnly && passable);

        if (lerp)
        {
            DynamicCollider.enabled = false;
            BoxOnlyDynamicCollider.enabled = true;
            StaticBoxCollider.enabled = !drop;
            StaticWedgeCollider.enabled = !drop;
        }
        else
        {
            DynamicCollider.enabled = false;
            BoxOnlyDynamicCollider.enabled = true;
            StaticBoxCollider.enabled = true;
            StaticWedgeCollider.enabled = true;
        }

        StaticBoxCollider.material.staticFriction = 0;
        StaticBoxCollider.material.dynamicFriction = 0;
        StaticWedgeCollider.material.staticFriction = 0;
        StaticWedgeCollider.material.dynamicFriction = 0;
        DynamicCollider.material.staticFriction = 0;
        DynamicCollider.material.dynamicFriction = 0;
    }

    public void OnDropComplete()
    {
        StaticBoxCollider.enabled = true;
        StaticWedgeCollider.enabled = true;
        DynamicCollider.enabled = false;
        BoxOnlyDynamicCollider.enabled = true;
    }

    public void OnPush()
    {
        StaticBoxCollider.enabled = true;
        StaticWedgeCollider.enabled = false;
        DynamicCollider.enabled = true;
        BoxOnlyDynamicCollider.enabled = true;
        DynamicCollider.material.dynamicFriction = 0f;
    }

    public void OnPushEnd()
    {
        StaticBoxCollider.enabled = true;
        StaticWedgeCollider.enabled = true;
        DynamicCollider.enabled = false;
        BoxOnlyDynamicCollider.enabled = true;
        DynamicCollider.material.dynamicFriction = 0f;
    }

    public void OnKick()
    {
        StaticBoxCollider.enabled = false;
        StaticWedgeCollider.enabled = false;
        DynamicCollider.enabled = true;
        BoxOnlyDynamicCollider.enabled = true;
        DynamicCollider.material.dynamicFriction = 0f;
    }

    public void OnBeingLift()
    {
        StaticBoxCollider.enabled = true;
        StaticWedgeCollider.enabled = true;
        DynamicCollider.enabled = false;
        BoxOnlyDynamicCollider.enabled = true;
    }

    public void OnThrow()
    {
        StaticBoxCollider.enabled = false;
        StaticWedgeCollider.enabled = false;
        DynamicCollider.enabled = true;
        BoxOnlyDynamicCollider.enabled = true;
        DynamicCollider.material.dynamicFriction = Box.Throw_Friction;
    }

    public void OnPut()
    {
        StaticBoxCollider.enabled = false;
        StaticWedgeCollider.enabled = false;
        DynamicCollider.enabled = true;
        BoxOnlyDynamicCollider.enabled = true;
        DynamicCollider.material.dynamicFriction = Box.Throw_Friction;
    }

    public void OnDropFromDeadActor()
    {
        StaticBoxCollider.enabled = false;
        StaticWedgeCollider.enabled = false;
        DynamicCollider.enabled = true;
        BoxOnlyDynamicCollider.enabled = true;
    }

    public void OnRigidbodyStop()
    {
        StaticBoxCollider.enabled = true;
        StaticWedgeCollider.enabled = true;
        DynamicCollider.enabled = false;
        BoxOnlyDynamicCollider.enabled = false;
    }
}