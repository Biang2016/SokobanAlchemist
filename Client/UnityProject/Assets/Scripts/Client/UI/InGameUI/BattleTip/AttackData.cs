public struct AttackData
{
    public Actor Attacker;
    public Actor Hitter;
    public int DecHp;
    public BattleTipType BattleTipType;
    public int ElementType;
    public int ElementHP;

    public AttackData(Actor attacker, Actor hitter, int decHp, BattleTipType battleTipType, int elementType, int elementHp)
    {
        Attacker = attacker;
        Hitter = hitter;
        DecHp = decHp;
        BattleTipType = battleTipType;
        ElementType = elementType;
        ElementHP = elementHp;
    }
}