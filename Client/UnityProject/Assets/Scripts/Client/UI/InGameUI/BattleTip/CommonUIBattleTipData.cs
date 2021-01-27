using UnityEngine;

public struct CommonUIBattleTipData
{
    public Camp Receiver;
    public Vector3 ReceiverPosition;
    public BattleTipType BattleTipType;

    public CommonUIBattleTipData(Camp receiver, Vector3 receiverPosition, BattleTipType battleTipType)
    {
        Receiver = receiver;
        ReceiverPosition = receiverPosition;
        BattleTipType = battleTipType;
    }
}