using System;
using System.Collections.Generic;
using System.Text;
using BiangLibrary;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif
public class Actor_LevelEditor : Entity_LevelEditor
{
    public override bool RefreshOrientation()
    {
        return true;
    }
}