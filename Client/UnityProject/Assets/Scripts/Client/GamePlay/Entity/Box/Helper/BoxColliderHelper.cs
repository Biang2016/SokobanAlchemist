using BiangLibrary.GameDataFormat.Grid;
using UnityEngine;

public class BoxColliderHelper : BoxMonoHelper, IBoxHelper
{
    [SerializeField]
    private GameObject NormalColliderRoot;

    [SerializeField]
    private GameObject StaticColliderRoot;

    [SerializeField]
    private GameObject DynamicColliderRoot; // gameObject.setActive字段为此类专用

    [SerializeField]
    private Collider[] StaticColliders; // enable 字段为此类专用；gameObject.setActive字段为其他特殊开关专用

    [SerializeField]
    private Collider[] DynamicColliders; // enable 字段为此类专用；gameObject.setActive字段为其他特殊开关专用

    [SerializeField]
    private Collider BoxOnlyDynamicCollider;

    private bool staticColliderEnable;

    private bool StaticColliderEnable
    {
        get { return staticColliderEnable; }
        set
        {
            staticColliderEnable = value;
            foreach (Collider sc in StaticColliders)
            {
                sc.enabled = value;
            }
        }
    }

    private float StaticColliderStaticFriction
    {
        set
        {
            foreach (Collider sc in StaticColliders)
            {
                sc.material.staticFriction = value;
            }
        }
    }

    private float StaticColliderDynamicFriction
    {
        set
        {
            foreach (Collider sc in StaticColliders)
            {
                sc.material.dynamicFriction = value;
            }
        }
    }

    private bool dynamicColliderEnable;

    private bool DynamicColliderEnable
    {
        get { return dynamicColliderEnable; }
        set
        {
            dynamicColliderEnable = value;
            foreach (Collider dc in DynamicColliders)
            {
                dc.enabled = value;
            }
        }
    }

    private float DynamicColliderStaticFriction
    {
        set
        {
            foreach (Collider dc in DynamicColliders)
            {
                dc.material.staticFriction = value;
            }
        }
    }

    private float DynamicColliderDynamicFriction
    {
        set
        {
            foreach (Collider dc in DynamicColliders)
            {
                dc.material.dynamicFriction = value;
            }
        }
    }

    public void OnBoxUsed()
    {
        NormalColliderRoot.SetActive(true);
        StaticColliderRoot.SetActive(true);
        BoxOnlyDynamicCollider.gameObject.SetActive(true);
    }

    public void OnBoxPoolRecycled()
    {
        StaticColliderEnable = false;
        DynamicColliderEnable = false;
        BoxOnlyDynamicCollider.enabled = false;
        NormalColliderRoot.SetActive(false);
        StaticColliderRoot.SetActive(false);
        BoxOnlyDynamicCollider.gameObject.SetActive(false);
    }

    public void Initialize(bool passable, bool artOnly, bool isGround, bool drop, bool lerp)
    {
        NormalColliderRoot.SetActive(!passable);
        StaticColliderRoot.SetActive(!artOnly);
        DynamicColliderRoot.gameObject.SetActive(!artOnly);
        BoxOnlyDynamicCollider.gameObject.SetActive(!artOnly && passable);

        if (lerp)
        {
            DynamicColliderEnable = false;
            BoxOnlyDynamicCollider.enabled = true;
            StaticColliderEnable = !drop;
        }
        else
        {
            DynamicColliderEnable = false;
            BoxOnlyDynamicCollider.enabled = true;
            StaticColliderEnable = true;
        }

        StaticColliderStaticFriction = 0;
        StaticColliderDynamicFriction = 0;
        DynamicColliderStaticFriction = 0;
        DynamicColliderDynamicFriction = 0;
    }

    public void OnDropComplete()
    {
        StaticColliderEnable = true;
        DynamicColliderEnable = false;
        BoxOnlyDynamicCollider.enabled = true;
    }

    public void OnPush()
    {
        StaticColliderEnable = true;
        DynamicColliderEnable = true;
        BoxOnlyDynamicCollider.enabled = true;
        DynamicColliderDynamicFriction = 0f;
    }

    public void OnPushEnd()
    {
        StaticColliderEnable = true;
        DynamicColliderEnable = false;
        BoxOnlyDynamicCollider.enabled = true;
        DynamicColliderDynamicFriction = 0f;
    }

    public void OnKick()
    {
        StaticColliderEnable = false;
        DynamicColliderEnable = true;
        BoxOnlyDynamicCollider.enabled = true;
        DynamicColliderDynamicFriction = 0f;
    }

    public void OnBeingLift()
    {
        StaticColliderEnable = true;
        DynamicColliderEnable = false;
        BoxOnlyDynamicCollider.enabled = true;
    }

    public void OnThrow()
    {
        StaticColliderEnable = false;
        DynamicColliderEnable = true;
        BoxOnlyDynamicCollider.enabled = true;
        DynamicColliderDynamicFriction = Box.Throw_Friction;
    }

    public void OnPut()
    {
        StaticColliderEnable = false;
        DynamicColliderEnable = true;
        BoxOnlyDynamicCollider.enabled = true;
        DynamicColliderDynamicFriction = Box.Throw_Friction;
    }

    public void OnDropFromDeadActor()
    {
        StaticColliderEnable = false;
        DynamicColliderEnable = true;
        BoxOnlyDynamicCollider.enabled = true;
    }

    public void OnDropFromAir()
    {
        StaticColliderEnable = false;
        DynamicColliderEnable = true;
        BoxOnlyDynamicCollider.enabled = true;
    }

    public void OnRigidbodyStop()
    {
        StaticColliderEnable = true;
        DynamicColliderEnable = false;
        BoxOnlyDynamicCollider.enabled = false;
    }
}