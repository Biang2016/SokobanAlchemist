using System;
using System.Collections.Generic;
using BiangLibrary;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(menuName = "BoxMarchingTextureConfigSSO")]
public class BoxMarchingTextureConfigSSO : SerializedScriptableObject
{
    public Dictionary<MarchingTextureCase, Texture> TextureDict = new Dictionary<MarchingTextureCase, Texture>();

#if UNITY_EDITOR
    [Button("按文件夹自动识别")]
    public void InitAddAllCases()
    {
        if (!string.IsNullOrWhiteSpace(TextureFolder))
        {
            if (Directory.Exists(TextureFolder))
            {
                DirectoryInfo di = new DirectoryInfo(Application.dataPath.Replace("Assets", "") + TextureFolder);
                foreach (FileInfo fi in di.GetFiles("*.png", SearchOption.AllDirectories))
                {
                    string relativePath = CommonUtils.ConvertAbsolutePathToProjectPath(fi.FullName);
                    Texture2D texture = (Texture2D) AssetDatabase.LoadAssetAtPath<Texture2D>(relativePath);
                    if (Enum.TryParse(texture.name, out MarchingTextureCase textureCase))
                    {
                        if (!TextureDict.ContainsKey(textureCase))
                        {
                            TextureDict.Add(textureCase, texture);
                        }
                        else
                        {
                            TextureDict[textureCase] = texture;
                        }
                    }
                }
            }
        }

        foreach (MarchingTextureCase value in Enum.GetValues(typeof(MarchingTextureCase)))
        {
            if (!TextureDict.ContainsKey(value))
            {
                TextureDict.Add(value, null);
            }
        }
    }

    [FolderPath(AbsolutePath = false, RequireExistingPath = true)]
    public string TextureFolder;
#endif
}