using System;
using System.Collections.Generic;
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
    public SortedDictionary<TerrainType, BoxMarchingTextureConfigSSO> Matrix = new SortedDictionary<TerrainType, BoxMarchingTextureConfigSSO>();

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