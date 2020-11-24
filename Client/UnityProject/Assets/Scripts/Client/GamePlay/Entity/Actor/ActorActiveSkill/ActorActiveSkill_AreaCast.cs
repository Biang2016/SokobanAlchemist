using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class ActorActiveSkill_AreaCast : ActorActiveSkill
{
    public enum CastType
    {
        SphereCast,
        BoxCast,
        FrontRectCast,
        FrontFanCast
    }

    [LabelText("判定范围类型")]
    public CastType AreaCastType;

    [LabelText("施法半径")]
    public int CastingRadius;

    [LabelText("效果半径")]
    public int EffectRadius;

    [LabelText("深度")]
    [ShowIf("AreaCastType", CastType.FrontRectCast)]
    public int Depth;

    [LabelText("扇形角度90/180/270")]
    [ShowIf("AreaCastType", CastType.FrontFanCast)]
    public int FanAngle;

    [LabelText("技能范围标识类型")]
    [ValueDropdown("GetAllBattleIndicatorNames", DropdownTitle = "选择技能范围标识类型")]
    public string BattleIndicatorType;

    private BattleIndicator battleIndicator;

    public override void OnInit()
    {
        base.OnInit();
    }

    protected override void WingUp()
    {
        base.WingUp();
        battleIndicator?.PoolRecycle();
        ushort bi = ConfigManager.GetBattleIndicatorTypeIndex(BattleIndicatorType);
        if (bi != 0)
        {
            switch (AreaCastType)
            {
                case CastType.SphereCast:
                {
                    break;
                }
                case CastType.BoxCast:
                {
                    break;
                }
                case CastType.FrontRectCast:
                {
                    break;
                }
                case CastType.FrontFanCast:
                {
                    break;
                }
            }
        }
        else
        {
            battleIndicator = null;
        }
    }

    protected override void ChildClone(ActorActiveSkill newAS)
    {
        base.ChildClone(newAS);
        ActorActiveSkill_AreaCast asAreaCast = (ActorActiveSkill_AreaCast) newAS;
        asAreaCast.AreaCastType = AreaCastType;
        asAreaCast.CastingRadius = CastingRadius;
        asAreaCast.EffectRadius = EffectRadius;
        asAreaCast.Depth = Depth;
        asAreaCast.FanAngle = FanAngle;
        asAreaCast.BattleIndicatorType = BattleIndicatorType;
    }

    public override void CopyDataFrom(ActorActiveSkill srcData)
    {
        base.CopyDataFrom(srcData);
        ActorActiveSkill_AreaCast asAreaCast = (ActorActiveSkill_AreaCast) srcData;
        AreaCastType = asAreaCast.AreaCastType;
        CastingRadius = asAreaCast.CastingRadius;
        EffectRadius = asAreaCast.EffectRadius;
        Depth = asAreaCast.Depth;
        FanAngle = asAreaCast.FanAngle;
        BattleIndicatorType = asAreaCast.BattleIndicatorType;
    }
}