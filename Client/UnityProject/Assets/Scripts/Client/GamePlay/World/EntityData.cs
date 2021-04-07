using System;
using System.Collections;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[Serializable]
public class EntityData : LevelComponentData
{
    [BoxGroup("额外数据")]
    [HideLabel]
    public EntityExtraSerializeData RawEntityExtraSerializeData = new EntityExtraSerializeData(); // 干数据，禁修改

    [LabelText("朝向")]
    [EnumToggleButtons]
    public GridPosR.Orientation EntityOrientation;
}