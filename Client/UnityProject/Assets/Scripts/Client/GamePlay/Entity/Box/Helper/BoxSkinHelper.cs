using BiangStudio.GameDataFormat.Grid;
using UnityEngine;
using Sirenix.OdinInspector;

public class BoxSkinHelper : BoxMonoHelper
{
    [SerializeField]
    private Mesh[] NormalMeshes;

    [SerializeField]
    private Mesh[] RoundedMeshes;

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
                    MeshFilter.mesh = NormalMeshes[(int) Box.BoxShapeType];
                    break;
                }
                case BoxModelType.Rounded:
                {
                    MeshFilter.mesh = RoundedMeshes[(int) Box.BoxShapeType];
                    break;
                }
            }
        }

        BoxModelType = boxModelType;
    }

    public void RefreshBoxShapeType()
    {
        if (MeshFilter)
        {
            switch (BoxModelType)
            {
                case BoxModelType.Normal:
                {
                    MeshFilter.mesh = NormalMeshes[(int) Box.BoxShapeType];
                    break;
                }
                case BoxModelType.Rounded:
                {
                    MeshFilter.mesh = RoundedMeshes[(int) Box.BoxShapeType];
                    break;
                }
            }
        }
    }

    public void SwitchBoxOrientation()
    {
        if (MeshFilter)
        {
            GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, Box.BoxOrientation), MeshFilter.transform, 1);
        }
    }

    public void ResetBoxOrientation()
    {
        if (MeshFilter)
        {
            GridPosR.ApplyGridPosToLocalTrans(new GridPosR(0, 0, GridPosR.Orientation.Up), MeshFilter.transform, 1);
        }
    }
}

public enum BoxModelType
{
    Normal,
    Rounded,
}