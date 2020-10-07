using UnityEngine;
using System.Collections;

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

    public enum BoxModelType
    {
        Box,
        Rounded,
    }
}