using UnityEngine;
using Sirenix.OdinInspector;

public class BoxSkinHelper : MonoBehaviour, IBoxHelper
{
    public Mesh BoxMesh;
    public Mesh RoundedBoxMesh;

    public MeshRenderer MeshRenderer;
    public MeshFilter MeshFilter;

    public void PoolRecycle()
    {
    }

    public void SwitchModel(BoxModelType boxModelType)
    {
        if (MeshFilter)
        {
            switch (boxModelType)
            {
                case BoxModelType.Box:
                {
                    MeshFilter.mesh = BoxMesh;
                    break;
                }
                case BoxModelType.Rounded:
                {
                    MeshFilter.mesh = RoundedBoxMesh;
                    break;
                }
            }
        }
    }

    [Button("预览变圆")]
    private void SwitchToRound()
    {
        SwitchModel(BoxModelType.Rounded);
    }

    [Button("预览变方")]
    private void SwitchToBox()
    {
        SwitchModel(BoxModelType.Box);
    }

    public enum BoxModelType
    {
        Box,
        Rounded,
    }
}