using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

public class BoxSkinHelper : BoxMonoHelper
{
    [SerializeField]
    private Mesh NormalMesh;

    [SerializeField]
    private Mesh RoundedMesh;

    [SerializeField]
    private MeshRenderer MeshRenderer;

    [SerializeField]
    private MeshFilter MeshFilter;

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
    }

    [SerializeField]
    [OnValueChanged("SwitchBoxModelType")]
    private BoxModelType BoxModelType;

    private void SwitchBoxModelType()
    {
        if (MeshFilter)
        {
            SwitchBoxModelType(BoxModelType);
        }
    }

    public void SwitchBoxModelType(BoxModelType boxModelType)
    {
        if (MeshFilter)
        {
            switch (boxModelType)
            {
                case BoxModelType.Normal:
                {
                    MeshFilter.mesh = NormalMesh;
                    break;
                }
                case BoxModelType.Rounded:
                {
                    MeshFilter.mesh = RoundedMesh;
                    break;
                }
            }
        }

        BoxModelType = boxModelType;
    }
}

public enum BoxModelType
{
    Normal,
    Rounded,
}