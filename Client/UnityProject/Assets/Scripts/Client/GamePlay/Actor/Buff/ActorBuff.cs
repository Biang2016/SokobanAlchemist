using System;
using System.Collections.Generic;
using System.Linq;
using BiangStudio.CloneVariant;
using Sirenix.OdinInspector;

[Serializable]
public class ActorBuff : IClone<ActorBuff>
{
    #region GUID

    [ReadOnly]
    [HideInEditorMode]
    public uint GUID;

    private static uint guidGenerator = (uint) ConfigManager.GUID_Separator.Buff;

    private uint GetGUID()
    {
        return guidGenerator++;
    }

    #endregion

    [ValueDropdown("GetAllFXTypeNames")]
    [LabelText("Buff特效")]
    public string BuffFX;

    [LabelText("Buff特效尺寸")]
    public float BuffFXScale = 1.0f;

    public ActorBuff()
    {
        GUID = GetGUID();
    }

    public ActorBuff Clone()
    {
        Type type = GetType();
        ActorBuff newBuff = (ActorBuff) Activator.CreateInstance(type);
        newBuff.BuffFX = BuffFX;
        newBuff.BuffFXScale = BuffFXScale;
        ChildClone(newBuff);
        return newBuff;
    }

    protected virtual void ChildClone(ActorBuff newBuff)
    {
    }

    #region Utils

    private IEnumerable<string> GetAllBoxTypeNames()
    {
        ConfigManager.LoadAllConfigs();
        List<string> res = ConfigManager.BoxTypeDefineDict.TypeIndexDict.Keys.ToList();
        res.Insert(0, "None");
        return res;
    }

    private IEnumerable<string> GetAllFXTypeNames()
    {
        ConfigManager.LoadAllConfigs();
        List<string> res = ConfigManager.FXTypeDefineDict.TypeIndexDict.Keys.ToList();
        res.Insert(0, "None");
        return res;
    }

    #endregion
}

[Serializable]
public class ActorBuff_ChangeMoveSpeed : ActorBuff
{
    [LabelText("速度增加比率%")]
    public int Percent;

    protected override void ChildClone(ActorBuff newBuff)
    {
        base.ChildClone(newBuff);
        ActorBuff_ChangeMoveSpeed buff = ((ActorBuff_ChangeMoveSpeed) newBuff);
        buff.Percent = Percent;
    }
}