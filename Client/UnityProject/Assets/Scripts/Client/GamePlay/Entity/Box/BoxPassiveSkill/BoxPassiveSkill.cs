using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BiangLibrary.CloneVariant;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class BoxPassiveSkill : IClone<BoxPassiveSkill>
{
    internal Box Box;

    [LabelText("技能描述")]
    [ShowInInspector]
    [PropertyOrder(-1)]
    protected abstract string Description { get; }

    [LabelText("特例类型")]
    [EnumToggleButtons]
    public BoxPassiveSkillBaseSpecialCaseType SpecialCaseType = BoxPassiveSkillBaseSpecialCaseType.None;

    public enum BoxPassiveSkillBaseSpecialCaseType
    {
        [LabelText("无")]
        None,

        [LabelText("模组特例")]
        Module,

        [LabelText("模组特例")]
        World,
    }

    #region Conditions

    public virtual void OnInit()
    {
    }

    public virtual void OnTick(float deltaTime)
    {
    }

    public virtual void OnRegisterLevelEventID()
    {
    }

    public virtual void OnUnRegisterLevelEventID()
    {
    }

    public virtual void OnBeingLift(Actor actor)
    {
    }

    public virtual void OnBeingKicked(Actor actor)
    {
    }

    public virtual void OnFlyingCollisionEnter(Collision collision)
    {
    }

    public virtual void OnBeingKickedCollisionEnter(Collision collision)
    {
    }

    public virtual void OnDroppingFromAirCollisionEnter(Collision collision)
    {
    }

    public virtual void OnBoxThornTrapTriggerEnter(Collider collider)
    {
    }

    public virtual void OnBoxThornTrapTriggerStay(Collider collider)
    {
    }

    public virtual void OnBoxThornTrapTriggerExit(Collider collider)
    {
    }

    public virtual void OnBeforeDestroyBox()
    {
    }

    public virtual void OnBeforeMergeBox()
    {
    }

    public virtual void OnDestroyBox()
    {
    }

    public virtual void OnMergeBox()
    {
    }

    #endregion

    public BoxPassiveSkill Clone()
    {
        Type type = GetType();
        BoxPassiveSkill newPS = (BoxPassiveSkill) Activator.CreateInstance(type);
        newPS.SpecialCaseType = SpecialCaseType;
        ChildClone(newPS);
        return newPS;
    }

    protected virtual void ChildClone(BoxPassiveSkill newPS)
    {
    }

    public virtual void CopyDataFrom(BoxPassiveSkill srcData)
    {
        SpecialCaseType = srcData.SpecialCaseType;
    }

#if UNITY_EDITOR

    public bool RenameBoxTypeName(string boxInstanceName, string srcBoxName, string targetBoxName, StringBuilder info, bool moduleSpecial = false, bool worldSpecial = false)
    {
        if (moduleSpecial && SpecialCaseType != BoxPassiveSkillBaseSpecialCaseType.Module) return false;
        if (worldSpecial && SpecialCaseType != BoxPassiveSkillBaseSpecialCaseType.World) return false;
        bool isDirty = false;
        foreach (FieldInfo fi in GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
        {
            foreach (Attribute a in fi.GetCustomAttributes(false))
            {
                if (a is BoxNameAttribute)
                {
                    if (fi.FieldType == typeof(string))
                    {
                        string fieldValue = (string) fi.GetValue(this);
                        if (fieldValue == srcBoxName)
                        {
                            info.Append($"替换{boxInstanceName}.BoxPassiveSkills.{GetType().Name}.{fi.Name} -> '{targetBoxName}'\n");
                            fi.SetValue(this, targetBoxName);
                            isDirty = true;
                        }
                    }
                }
                else if (a is BoxNameListAttribute)
                {
                    if (fi.FieldType == typeof(List<string>))
                    {
                        List<string> fieldValueList = (List<string>) fi.GetValue(this);
                        for (int i = 0; i < fieldValueList.Count; i++)
                        {
                            string fieldValue = fieldValueList[i];
                            if (fieldValue == srcBoxName)
                            {
                                info.Append($"替换于{boxInstanceName}.PassiveSkills.{GetType().Name}.{fi.Name}\n");
                                fieldValueList[i] = targetBoxName;
                                isDirty = true;
                            }
                        }
                    }
                }
            }
        }

        return isDirty;
    }

    public bool DeleteBoxTypeName(string boxInstanceName, string srcBoxName, StringBuilder info, bool moduleSpecial = false, bool worldSpecial = false)
    {
        if (moduleSpecial && SpecialCaseType != BoxPassiveSkillBaseSpecialCaseType.Module) return false;
        if (worldSpecial && SpecialCaseType != BoxPassiveSkillBaseSpecialCaseType.World) return false;
        bool isDirty = false;

        foreach (FieldInfo fi in GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
        {
            foreach (Attribute a in fi.GetCustomAttributes(false))
            {
                if (a is BoxNameAttribute)
                {
                    if (fi.FieldType == typeof(string))
                    {
                        string fieldValue = (string) fi.GetValue(this);
                        if (fieldValue == srcBoxName)
                        {
                            info.Append($"替换{boxInstanceName}.BoxPassiveSkills.{GetType().Name}.{fi.Name} -> 'None'\n");
                            fi.SetValue(this, "None");
                            isDirty = true;
                        }
                    }
                }
                else if (a is BoxNameListAttribute)
                {
                    if (fi.FieldType == typeof(List<string>))
                    {
                        List<string> fieldValueList = (List<string>) fi.GetValue(this);
                        for (int i = 0; i < fieldValueList.Count; i++)
                        {
                            string fieldValue = fieldValueList[i];
                            if (fieldValue == srcBoxName)
                            {
                                info.Append($"移除自{boxInstanceName}.PassiveSkills.{GetType().Name}.{fi.Name}\n");
                                fieldValueList.RemoveAt(i);
                                i--;
                                isDirty = true;
                            }
                        }
                    }
                }
            }
        }

        return isDirty;
    }

#endif
}