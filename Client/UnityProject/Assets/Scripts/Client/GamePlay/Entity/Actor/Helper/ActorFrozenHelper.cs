using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using UnityEngine;

public class ActorFrozenHelper : EntityFrozenHelper
{
    internal Box FrozenBox;

    public override void FrozeIntoIceBlock(int beforeFrozenLevel, int afterFrozenLevel)
    {
        Actor actor = (Actor) Entity;
        if (afterFrozenLevel <= 1)
        {
            actor.ForbidAction = false;
            actor.SetModelSmoothMoveLerpTime(actor.DefaultSmoothMoveLerpTime);
            if (FrozenBox)
            {
                actor.transform.parent = BattleManager.Instance.ActorContainerRoot;
                FrozenBox.FrozenActor = null;
                FrozenBox.DestroyBox();
                FrozenBox = null;
            }

            Thaw();
            FXManager.Instance.PlayFX(actor.ThawFX, transform.position);
        }
        else
        {
            if (FrozenBox)
            {
            }
            else
            {
                actor.SnapToGrid();
                WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(actor.WorldGP);
                if (module)
                {
                    EntityData entityData = new EntityData(ConfigManager.Box_EnemyFrozenBoxIndex, actor.EntityOrientation);
                    // triggerAppear参数填true以确保冰冻箱子能正常生成
                    FrozenBox = (Box) module.GenerateEntity(entityData, actor.WorldGP, true, false, false, actor.GetEntityOccupationGPs_Rotated());
                    if (FrozenBox)
                    {
                        List<EntityPassiveSkill> actorFrozenBoxPassiveSkills = actor.RawFrozenBoxPassiveSkills.Clone();
                        foreach (EntityPassiveSkill abf in actorFrozenBoxPassiveSkills)
                        {
                            FrozenBox.AddNewPassiveSkill(abf, true);
                        }

                        FrozenBox.FrozenActor = actor;
                        FrozenBox.BoxFrozenBoxHelper.GenerateBoxIndicatorForFrozenActor(actor);
                        FrozenBox.BoxFrozenBoxHelper.SetColliderSize_ForFrozenEnemyBox(actor.ActorWidth);
                        actor.transform.parent = FrozenBox.transform;
                        actor.CurMoveAttempt = Vector3.zero;
                        actor.ActorPushHelper.TriggerOut = false;
                        actor.ForbidAction = true;
                        transform.localRotation = Quaternion.identity;
                        FrozeModelRoot.SetActive(true);
                    }
                    else
                    {
                        Debug.Log("生成冻结箱子失败");
                    }
                }
                else
                {
                    Debug.LogError("角色不在任一模组内");
                }
            }

            for (int index = 0; index < FrozeModels.Length; index++)
            {
                GameObject frozeModel = FrozeModels[index];
                frozeModel.SetActive(index == afterFrozenLevel - 1);
            }

            FXManager.Instance.PlayFX(beforeFrozenLevel < afterFrozenLevel ? actor.FrozeFX : actor.ThawFX, transform.position);
        }
    }
}