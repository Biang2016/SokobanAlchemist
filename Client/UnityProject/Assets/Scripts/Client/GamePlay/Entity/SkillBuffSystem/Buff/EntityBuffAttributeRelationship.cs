using Sirenix.OdinInspector;

public enum EntityBuffAttributeRelationship
{
    [LabelText("相容")]
    Compatible, // Buff相容

    [LabelText("互斥")]
    Mutex, // 直接替换

    [LabelText("排斥")]
    Repel, // 后者无法添加

    [LabelText("抵消")]
    SetOff, // 两者同时消失

    [LabelText("大值优先")]
    MaxDominant, // 仅针对同种ActorBuffAttribute，允许多buff共存但同一时刻仅最大值生效，各buff分别计时
}