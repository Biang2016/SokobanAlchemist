public struct CommonUIBattleTipData
{
    public Actor Caster;
    public Actor Receiver;
    public BattleTipType BattleTipType;

    public CommonUIBattleTipData(Actor caster, Actor receiver, BattleTipType battleTipType)
    {
        Caster = caster;
        Receiver = receiver;
        BattleTipType = battleTipType;
    }
}