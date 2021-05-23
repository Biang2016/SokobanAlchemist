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
public class EntityDataExtraStates : IClone<EntityDataExtraStates>
{
    public bool R_DoorOpen = false;
    public bool DoorOpen = false;

    public bool R_TransportBoxClosed = false;
    public bool TransportBoxClosed = false;

    public bool R_GoldValue = false;
    public int GoldValue = 0;

    #region BoxArtHelper

    public bool R_ModelIndex = false;
    public int ModelIndex = 0;

    public bool R_DecoratorIndex = false;
    public int DecoratorIndex = 0;

    public bool R_ModelScale = false;
    public Vector3 ModelScale = Vector3.one;

    public bool R_ModelRotation = false;
    public Quaternion ModelRotation = Quaternion.identity;

    #endregion

    #region ESPS

    public bool R_HealthDurability = false;
    public int HealthDurability;

    public bool R_FiringValue = false;
    public int FiringValue;

    public bool R_FrozenValue = false;
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