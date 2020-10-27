using BiangStudio.CloneVariant;
using UnityEngine;

public class UIBattleTipInfo : IClone<UIBattleTipInfo>
{
    public uint HitMCB_GUID;
    public BattleTipType BattleTipType;
    public Camp CasterCamp;
    public Camp ReceiverCamp;
    public int DiffHP;
    public int ElementHP;
    public float Scale;
    public int ElementType;
    public string SpriteImagePath;
    public Vector3 StartPos;
    public Vector2 Offset;
    public Vector2 RandomRange;
    public float DisappearTime = 1.5f;

    public UIBattleTipInfo(uint hitMcbGuid, BattleTipType battleTipType, Camp casterCamp, Camp receiverCamp, int diffHp, int elementHp, float scale, int elementType,
        string spriteImagePath, Vector3 startPos, Vector2 offset, Vector2 randomRange, float disappearTime)
    {
        HitMCB_GUID = hitMcbGuid;
        BattleTipType = battleTipType;
        CasterCamp = casterCamp;
        ReceiverCamp = receiverCamp;
        DiffHP = diffHp;
        ElementHP = elementHp;
        Scale = scale;
        ElementType = elementType;
        SpriteImagePath = spriteImagePath;
        StartPos = startPos;
        Offset = offset;
        RandomRange = randomRange;
        DisappearTime = disappearTime;
    }

    public UIBattleTipInfo Clone()
    {
        return new UIBattleTipInfo(HitMCB_GUID, BattleTipType, CasterCamp, ReceiverCamp, DiffHP, ElementHP, Scale, ElementType, SpriteImagePath, StartPos, Offset, RandomRange, DisappearTime);
    }
}