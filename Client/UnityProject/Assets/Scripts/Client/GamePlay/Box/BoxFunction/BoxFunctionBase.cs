using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class BoxFunctionBase
{
    private IEnumerable<string> GetAllBoxTypeNames()
    {
        ConfigManager.LoadAllConfigs();
        List<string> res = ConfigManager.BoxTypeDefineDict.TypeIndexDict.Keys.ToList();
        return res;
    }
}

[Serializable]
[LabelText("举起箱子掉落踢技能")]
public class BoxFunction_LiftDropSkill : BoxFunctionBase
{
    [GUIColor(0, 1.0f, 0)]
    [LabelText("举起箱子掉落踢技能")]
    [ValueDropdown("GetAllBoxTypeNames", IsUniqueList = true, DropdownTitle = "选择箱子类型", DrawDropdownForListElements = false, ExcludeExistingValuesInList = true)]
    public string LiftGetLiftBoxAbility;
}

[Serializable]
[LabelText("举起箱子更换皮肤")]
public class BoxFunction_LiftDropSkin : BoxFunctionBase
{
    [GUIColor(0, 1.0f, 0)]
    [LabelText("举起箱子更换皮肤")]
    public Material DieDropMaterial;
}

[Serializable]
[LabelText("举箱子回复生命")]
public class BoxFunction_LiftGainHealth : BoxFunctionBase
{
    [LabelText("举箱子回复生命")]
    public int HealLifeCountWhenLifted;
}

[Serializable]
[LabelText("爆炸推力")]
public class BoxFunction_ExplodePushForce : BoxFunctionBase
{
    [LabelText("碰撞爆炸推力")]
    public bool ExplodePushForce = false;

    [ShowIf("ExplodePushForce")]
    [LabelText("碰撞爆炸推力半径")]
    public int ExplodePushRadius = 3;
}

[Serializable]
[LabelText("荆棘伤害")]
public class BoxFunction_ThornDamage : BoxFunctionBase
{
    [LabelText("每次伤害")]
    public int Damage = 0;

    [LabelText("伤害间隔时间/s")]
    public float DamageInterval = 1f;
}