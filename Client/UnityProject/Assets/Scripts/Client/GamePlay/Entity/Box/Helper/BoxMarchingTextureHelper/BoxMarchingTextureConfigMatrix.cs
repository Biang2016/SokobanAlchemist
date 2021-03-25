using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[CreateAssetMenu(menuName = "BoxMarchingTextureConfigMatrix")]
public class BoxMarchingTextureConfigMatrix : SerializedScriptableObject
{
    [InfoBox("$Tips")]
    [OdinSerialize]
    [NonSerialized]
    [TableMatrix(SquareCells = true)]
    public BoxMarchingTextureConfigSSO[,] Matrix = new BoxMarchingTextureConfigSSO[10, 10];

    public Texture2D[] PureTerrain = new Texture2D[10];

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