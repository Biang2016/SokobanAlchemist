using BiangLibrary.GameDataFormat.Grid;
using UnityEngine;

public class BoxColliderHelper : BoxMonoHelper, IBoxHelper
{
    [SerializeField]
    private GameObject NormalColliders;

    [SerializeField]
    private GameObject StaticColliders;

    [SerializeField]
    private Collider StaticBoxCollider;

    [SerializeField]
    private Collider DynamicCollider;

    [SerializeField]
    private Collider BoxOnlyDynamicCollider;

    public void OnBoxUsed()
    {
        NormalColliders.SetActive(true);
        StaticColliders.SetActive(true);
        BoxOnlyDynamicCollider.gameObject.SetActive(true);
    }

    public void OnBoxPoolRecycled()
    {
        StaticBoxCollider.enabled = false;
        DynamicCollider.enabled = false;
        BoxOnlyDynamicCollider.enabled = false;
        NormalColliders.SetActive(false);
        StaticColliders.SetActive(false);
        BoxOnlyDynamicCollider.gameObject.SetActive(false);
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
        }
        else
        {
            DynamicCollider.enabled = false;
            BoxOnlyDynamicCollider.enabled = true;
            StaticBoxCollider.enabled = true;
        }

        StaticBoxCollider.material.staticFriction = 0;
        StaticBoxCollider.material.dynamicFriction = 0;
        DynamicCollider.material.staticFriction = 0;
        DynamicCollider.material.dynamicFriction = 0;
    }

    public void OnDropComplete()
    {
        StaticBoxCollider.enabled = true;
        DynamicCollider.enabled = false;
        BoxOnlyDynamicCollider.enabled = true;
    }

    public void OnPush()
    {
        StaticBoxCollider.enabled = true;
        DynamicCollider.enabled = true;
        BoxOnlyDynamicCollider.enabled = true;
        DynamicCollider.material.dynamicFriction = 0f;
    }

    public void OnPushEnd()
    {
        StaticBoxCollider.enabled = true;
        DynamicCollider.enabled = false;
        BoxOnlyDynamicCollider.enabled = true;
        DynamicCollider.material.dynamicFriction = 0f;
    }

    public void OnKick()
    {
        StaticBoxCollider.enabled = false;
        DynamicCollider.enabled = true;
        BoxOnlyDynamicCollider.enabled = true;
        DynamicCollider.material.dynamicFriction = 0f;
    }

    public void OnBeingLift()
    {
        StaticBoxCollider.enabled = true;
        DynamicCollider.enabled = false;
        BoxOnlyDynamicCollider.enabled = true;
    }

    public void OnThrow()
    {
        StaticBoxCollider.enabled = false;
        DynamicCollider.enabled = true;
        BoxOnlyDynamicCollider.enabled = true;
        DynamicCollider.material.dynamicFriction = Box.Throw_Friction;
    }

    public void OnPut()
    {
        StaticBoxCollider.enabled = false;
        DynamicCollider.enabled = true;
        BoxOnlyDynamicCollider.enabled = true;
        DynamicCollider.material.dynamicFriction = Box.Throw_Friction;
    }

    public void OnDropFromDeadActor()
    {
        StaticBoxCollider.enabled = false;
        DynamicCollider.enabled = true;
        BoxOnlyDynamicCollider.enabled = true;
    }

    public void OnDropFromAir()
    {
        StaticBoxCollider.enabled = false;
        DynamicCollider.enabled = true;
        BoxOnlyDynamicCollider.enabled = true;
    }

    public void OnRigidbodyStop()
    {
        StaticBoxCollider.enabled = true;
        DynamicCollider.enabled = false;
        BoxOnlyDynamicCollider.enabled = false;
    }
}