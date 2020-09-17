using UnityEngine;
using System.Collections;

public class BoxSkinHelper : MonoBehaviour
{
    public Mesh BoxMesh;
    public Mesh RoundedBoxMesh;

    public MeshRenderer MeshRenderer;
    public MeshFilter MeshFilter;

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