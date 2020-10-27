public struct NumeralUIBattleTipData
{
    public Actor Caster;
    public Actor Receiver;
    public int MainNum;
    public BattleTipType BattleTipType;
    public int SubNumType;
    public int SubNum;

    public NumeralUIBattleTipData(Actor caster, Actor receiver, int mainNum, BattleTipType battleTipType, int subNumType, int subNum)
    {
        Caster = caster;
        Receiver = receiver;
        MainNum = mainNum;
        BattleTipType = battleTipType;
        SubNumType = subNumType;
        SubNum = subNum;
    }
}