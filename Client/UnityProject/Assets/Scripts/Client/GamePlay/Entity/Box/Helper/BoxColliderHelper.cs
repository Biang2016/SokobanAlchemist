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
    private GameObject BoxOnlyDynamicColliderRoot;

    [SerializeField]
    private Collider[] StaticColliders; // enable 字段为此类专用；gameObject.setActive字段为其他特殊开关专用

    [SerializeField]
    private Collider[] DynamicColliders; // enable 字段为此类专用；gameObject.setActive字段为其他特殊开关专用

    [SerializeField]
    private Collider[] BoxOnlyDynamicColliders; // enable 字段为此类专用；gameObject.setActive字段为其他特殊开关专用

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

    private bool boxOnlyDynamicCollidersEnable;

    private bool BoxOnlyDynamicCollidersEnable
    {
        get { return boxOnlyDynamicCollidersEnable; }
        set
        {
            boxOnlyDynamicCollidersEnable = value;
            foreach (Collider bodc in BoxOnlyDynamicColliders)
            {
                bodc.enabled = value;
            }
        }
    }

    public void OnBoxUsed()
    {
        NormalColliderRoot.SetActive(true);
        StaticColliderRoot.SetActive(true);
        BoxOnlyDynamicColliderRoot.SetActive(true);
    }

    public void OnBoxPoolRecycled()
    {
        StaticColliderEnable = false;
        DynamicColliderEnable = false;
        BoxOnlyDynamicCollidersEnable = false;
        NormalColliderRoot.SetActive(false);
        StaticColliderRoot.SetActive(false);
        BoxOnlyDynamicColliderRoot.SetActive(false);
    }

    public void Initialize(bool passable, bool artOnly, bool isGround, bool drop, bool lerp)
    {
        NormalColliderRoot.SetActive(!passable);
        StaticColliderRoot.SetActive(!artOnly);
        DynamicColliderRoot.gameObject.SetActive(!artOnly);
        BoxOnlyDynamicColliderRoot.SetActive(!artOnly && passable);

        if (lerp)
        {
            DynamicColliderEnable = false;
            BoxOnlyDynamicCollidersEnable = true;
            StaticColliderEnable = !drop;
        }
        else
        {
            DynamicColliderEnable = false;
            BoxOnlyDynamicCollidersEnable = true;
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
        BoxOnlyDynamicCollidersEnable = true;
    }

    public void OnPush()
    {
        StaticColliderEnable = true;
        DynamicColliderEnable = true;
        BoxOnlyDynamicCollidersEnable = true;
        DynamicColliderDynamicFriction = 0f;
    }

    public void OnPushEnd()
    {
        StaticColliderEnable = true;
        DynamicColliderEnable = false;
        BoxOnlyDynamicCollidersEnable = true;
        DynamicColliderDynamicFriction = 0f;
    }

    public void OnKick()
    {
        StaticColliderEnable = false;
        DynamicColliderEnable = true;
        BoxOnlyDynamicCollidersEnable = true;
        DynamicColliderDynamicFriction = 0f;
    }

    public void OnKick_ToGrind() // 带有碾压性质的踢出
    {
        BoxOnlyDynamicColliderRoot.SetActive(true);
        StaticColliderEnable = false;
        DynamicColliderEnable = false;
        BoxOnlyDynamicCollidersEnable = true;
        DynamicColliderDynamicFriction = 0f;
    }

    public void OnKick_ToGrind_End()
    {
        BoxOnlyDynamicColliderRoot.SetActive(false);
    }

    public void OnBeingLift()
    {
        StaticColliderEnable = true;
        DynamicColliderEnable = false;
        BoxOnlyDynamicCollidersEnable = true;
    }

    public void OnThrow()
    {
        StaticColliderEnable = false;
        DynamicColliderEnable = true;
        BoxOnlyDynamicCollidersEnable = true;
        DynamicColliderDynamicFriction = Box.Throw_Friction;
    }

    public void OnPut()
    {
        StaticColliderEnable = false;
        DynamicColliderEnable = true;
        BoxOnlyDynamicCollidersEnable = true;
        DynamicColliderDynamicFriction = Box.Throw_Friction;
    }

    public void OnDropFromDeadActor()
    {
        StaticColliderEnable = false;
        DynamicColliderEnable = true;
        BoxOnlyDynamicCollidersEnable = true;
    }

    public void OnDropFromAir()
    {
        StaticColliderEnable = false;
        DynamicColliderEnable = true;
        BoxOnlyDynamicCollidersEnable = true;
    }

    public void OnMerge()
    {
        StaticColliderEnable = true;
        DynamicColliderEnable = false;
        BoxOnlyDynamicCollidersEnable = false;
        DynamicColliderDynamicFriction = 0f;
    }

    public void OnRigidbodyStop()
    {
        StaticColliderEnable = true;
        DynamicColliderEnable = false;
        BoxOnlyDynamicCollidersEnable = false; 
    }
}