using System;
using System.Collections.Generic;
using System.Text;
using BiangLibrary;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;

#endif

public class Box_LevelEditor : Entity_LevelEditor
{
#if UNITY_EDITOR

    /// <summary>
    /// 专门作为工具使用，有时需要批量migrate数据
    /// </summary>
    /// <returns></returns>
    public bool RefreshData()
    {
        //bool isDirty = false;
        //foreach (EntityPassiveSkill bf in RawEntityExtraSerializeData.EntityPassiveSkills)
        //{
        //    if (bf is EntityPassiveSkill_Conditional condition)
        //    {
        //        foreach (EntityPassiveSkillAction action in condition.RawEntityPassiveSkillActions)
        //        {
        //            if (action is BoxPassiveSkillAction_ChangeBoxToEnemy cbt)
        //            {
        //                //cbt.RefreshABC();
        //                isDirty = true;
        //            }
        //        }
        //    }
        //}

        //return isDirty;
        return true;
    }

#endif
}