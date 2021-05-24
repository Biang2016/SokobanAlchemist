using System;
using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
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

    [OdinSerialize]
    [NonSerialized]
    internal EntityData FrozenActorData;

    public EntityExtraSerializeData Clone()
    {
        return new EntityExtraSerializeData
        {
            EntityPassiveSkills = EntityPassiveSkills.Clone<EntityPassiveSkill, EntitySkill>(),
            EntityDataExtraStates = EntityDataExtraStates.Clone(),
            FrozenActorData = FrozenActorData?.Clone(),
        };
    }
}

[Serializable]
public struct EntityDataExtraStates : IClone<EntityDataExtraStates>
{
    public bool R_DoorOpen;
    public bool DoorOpen;

    public bool R_TransportBoxClosed;
    public bool TransportBoxClosed;

    public bool R_GoldValue;
    public int GoldValue;

    #region BoxArtHelper

    public bool R_ModelIndex;
    public int ModelIndex;

    public bool R_DecoratorIndex;
    public int DecoratorIndex;

    public bool R_ModelScale;
    public Vector3 ModelScale;

    public bool R_ModelRotation;
    public Quaternion ModelRotation;

    #endregion

    #region ESPS

    public bool R_HealthDurability;
    public int HealthDurability;

    public bool R_FiringValue;
    public int FiringValue;

    public bool R_FrozenValue;
    public int FrozenValue;

    #endregion

    public EntityDataExtraStates Clone()
    {
        return new EntityDataExtraStates
        {
            R_DoorOpen = R_DoorOpen,
            DoorOpen = DoorOpen,

            R_TransportBoxClosed = R_TransportBoxClosed,
            TransportBoxClosed = TransportBoxClosed,

            R_GoldValue = R_GoldValue,
            GoldValue = GoldValue,

            #region BoxArtHelper

            R_ModelIndex = R_ModelIndex,
            ModelIndex = ModelIndex,

            R_DecoratorIndex = R_DecoratorIndex,
            DecoratorIndex = DecoratorIndex,

            R_ModelScale = R_ModelScale,
            ModelScale = ModelScale,

            R_ModelRotation = R_ModelRotation,
            ModelRotation = ModelRotation,

            #endregion

            #region ESPS

            R_HealthDurability = R_HealthDurability,
            HealthDurability = HealthDurability,

            R_FiringValue = R_FiringValue,
            FiringValue = FiringValue,

            R_FrozenValue = R_FrozenValue,
            FrozenValue = FrozenValue,

            #endregion
        };
    }
}