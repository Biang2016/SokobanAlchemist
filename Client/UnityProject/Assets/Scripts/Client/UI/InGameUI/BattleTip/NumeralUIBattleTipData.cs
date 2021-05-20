using UnityEngine;

public struct NumeralUIBattleTipData
{
    public Camp Receiver;
    public Vector3 ReceiverPosition;
    public int MainNum;
    public BattleTipType BattleTipType;
    public string ExtraInfo_Before;
    public string ExtraInfo_After;

    public NumeralUIBattleTipData(Camp receiver, Vector3 receiverPosition, int mainNum, BattleTipType battleTipType, string extraInfo_Before, string extraInfo_After)
    {
        Receiver = receiver;
        ReceiverPosition = receiverPosition;
        MainNum = mainNum;
        BattleTipType = battleTipType;
        ExtraInfo_Before = extraInfo_Before;
        ExtraInfo_After = extraInfo_After;
    }
}