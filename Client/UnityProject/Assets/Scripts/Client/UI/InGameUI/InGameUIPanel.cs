﻿using UnityEngine;
using System.Collections;
using BiangLibrary.GamePlay.UI;

public class InGameUIPanel : BaseUIPanel
{
    void Awake()
    {
        UIType.InitUIType(
            false,
            false,
            false,
            UIFormTypes.Fixed,
            UIFormShowModes.Normal,
            UIFormLucencyTypes.Penetrable);
    }
}