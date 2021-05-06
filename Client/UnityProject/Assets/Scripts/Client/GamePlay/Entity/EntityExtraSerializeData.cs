using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class EntityExtraSerializeData : IClone<EntityExtraSerializeData>
{
    [BoxGroup("额外被动技能")]
    [LabelText("额外被动技能")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    [SerializeReference]
    public List<EntityPassiveSkill> EntityPassiveSkills = new List<EntityPassiveSkill>();

    public EntityDataExtraStates EntityDataExtraStates = new EntityDataExtraStates();

    public EntityExtraSerializeData Clone()
    {
        return new EntityExtraSerializeData
        {
            EntityPassiveSkills = EntityPassiveSkills.Clone<EntityPassiveSkill, EntitySkill>(),
            EntityDataExtraStates = EntityDataExtraStates.Clone(),
        };
    }
}

[Serializable]
public class EntityDataExtraStates : IClone<EntityDataExtraStates>
{
    public bool DoorOpen = false;
    public bool TransportBoxClosed = false;

    public EntityDataExtraStates Clone()
    {
        return new EntityDataExtraStates
        {
            DoorOpen = DoorOpen,
            TransportBoxClosed = TransportBoxClosed,
        };
    }
}