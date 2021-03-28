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

#if UNITY_EDITOR
    [Button("Create TextureArray")]
    public void CreateTextureArray()
    {
        TextureFormat tf = Matrix[0, 0].format;
        Texture2DArray array = new Texture2DArray(512, 512, 55, tf, false);
        int index = 0;
        for (int i = 0; i < Matrix.GetLength(0); i++)
        for (int j = 0; j < Matrix.GetLength(1); j++)
        {
            Texture2D texture = Matrix[i, j];
            if (texture != null)
            {
                    //Graphics.CopyTexture(texture, 0, 0, array, index, 0);
                    array.SetPixels(texture.GetPixels(), index);
                    index++;
            }
        }

        AssetDatabase.CreateAsset(array, "Assets/EntityArts/Boxes/CombinedGroundBox/TerrainTextureArray.asset");
    }
#endif
}