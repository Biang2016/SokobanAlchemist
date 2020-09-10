using System;
using UnityEngine;
using System.Collections.Generic;

public class ActorSkinHelper : ActorHelper
{
    public Renderer MainSwitchSkin;
    public List<SkinConfig> SkinConfigs = new List<SkinConfig>();

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
            sc.Renderer.material = sc.Materials[(int) playerNumber];
        }
    }

    public void SwitchSkin(Material mat)
    {
        MainSwitchSkin.material = mat;
    }
}