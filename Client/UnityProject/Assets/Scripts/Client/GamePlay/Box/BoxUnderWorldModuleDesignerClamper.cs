using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class BoxUnderWorldModuleDesignerClamper : MonoBehaviour
{
    private WorldModuleDesignHelper ParentWorldModuleDesignHelper;
    private WorldDesignHelper ParentWorldDesignHelper;

    internal Vector3 DefaultPosition;
    internal Quaternion DefaultRotation;
    internal Vector3 DefaultScale;

    void Awake()
    {
        if (!Application.isPlaying)
        {
            DefaultPosition = transform.localPosition;
            DefaultRotation = transform.localRotation;
            DefaultScale = transform.localScale;
            ParentWorldDesignHelper = GetComponentInParent<WorldDesignHelper>();
            ParentWorldModuleDesignHelper = GetComponentInParent<WorldModuleDesignHelper>();
        }
    }

    void OnTransformParentChanged()
    {
        ParentWorldDesignHelper = GetComponentInParent<WorldDesignHelper>();
        ParentWorldModuleDesignHelper = GetComponentInParent<WorldModuleDesignHelper>();
    }

    private void LateUpdate()
    {
        if (ParentWorldDesignHelper && ParentWorldModuleDesignHelper)
        {
            transform.localPosition = DefaultPosition;
        }
        else
        {
            transform.localPosition = new Vector3(
                Mathf.Clamp(transform.localPosition.x, 0, WorldModule.MODULE_SIZE - 1),
                Mathf.Clamp(transform.localPosition.y, 0, WorldModule.MODULE_SIZE - 1),
                Mathf.Clamp(transform.localPosition.z, 0, WorldModule.MODULE_SIZE - 1));
        }

        transform.localRotation = DefaultRotation;
        transform.localScale = DefaultScale;
    }
}