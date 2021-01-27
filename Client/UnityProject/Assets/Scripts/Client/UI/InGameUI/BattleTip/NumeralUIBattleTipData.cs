using UnityEngine;

public struct NumeralUIBattleTipData
{
    public Camp Receiver;
    public Vector3 ReceiverPosition;
    public int MainNum;
    public BattleTipType BattleTipType;
    public int SubNumType;
    public int SubNum;

    public NumeralUIBattleTipData(Camp receiver,Vector3 receiverPosition ,int mainNum, BattleTipType battleTipType, int subNumType, int subNum)
    {
        Receiver = receiver;
        ReceiverPosition = receiverPosition;
        MainNum = mainNum;
        BattleTipType = battleTipType;
        SubNumType = subNumType;
        SubNum = subNum;
    }
}