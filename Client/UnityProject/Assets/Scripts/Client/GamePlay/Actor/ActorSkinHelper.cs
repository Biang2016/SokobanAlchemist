using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActorSkinHelper : MonoBehaviour
{
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
}
