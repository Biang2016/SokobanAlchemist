using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
#if UNITY_EDITOR
using UnityEditor;

#endif

[CreateAssetMenu(menuName = "BoxMarchingTextureConfigMatrix")]
public class BoxMarchingTextureConfigMatrix : SerializedScriptableObject
{
    [InfoBox("$Tips")]
    [OdinSerialize]
    [NonSerialized]
    [TableMatrix(SquareCells = true)]
    public Texture2D[,] Matrix = new Texture2D[10, 10];

    private string Tips
    {
        get
        {
            string tip = "";
            foreach (TerrainType value in Enum.GetValues(typeof(TerrainType)))
            {
                tip += (int) value + "-" + value.ToString() + "; ";
            }

            return tip;
        }
    }
}