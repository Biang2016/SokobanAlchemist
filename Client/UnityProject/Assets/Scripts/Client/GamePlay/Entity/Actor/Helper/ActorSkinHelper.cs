using System;
using UnityEngine;
using System.Collections.Generic;

public class ActorSkinHelper : ActorMonoHelper
{
    public Renderer MainSwitchSkin;
    public List<SkinConfig> SkinConfigs = new List<SkinConfig>();

    public Transform MainArtTransform;

    [Serializable]
    public class SkinConfig
    {
        public Renderer Renderer;
        public Material[] Materials;
    }

    public void Initialize(PlayerNumber playerNumber)
    {
        foreach (SkinConfig sc in SkinConfigs)
        {
            if (sc.Renderer != null)
            {
                sc.Renderer.material = sc.Materials[(int)playerNumber];
            }
        }
    }

    public void SwitchSkin(Material mat)
    {
        if (MainSwitchSkin)
        {
            MainSwitchSkin.material = mat;
        }
    }
}