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
            actor.ActorArtHelper.SetIsFrozen(false);
            if (FrozenBox)
            {
                actor.transform.parent = BattleManager.Instance.ActorContainerRoot;
                FrozenBox.FrozenActor = null;
                FrozenBox.DestroyBox();
                FrozenBox = null;
            }

            Thaw();
            FXManager.Instance.PlayFX(actor.ThawFX, transform.position, 1f);
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
                    FrozenBox = module.GenerateBox(ConfigManager.Box_EnemyFrozenBoxIndex, actor.WorldGP, GridPosR.Orientation.Up);
                    if (FrozenBox)
                    {
                        List<EntityPassiveSkill> actorFrozenBoxPassiveSkills = actor.RawFrozenBoxPassiveSkills.Clone();
                        foreach (EntityPassiveSkill abf in actorFrozenBoxPassiveSkills)
                        {
                            FrozenBox.AddNewPassiveSkill(abf, true);
                        }

                        actor.ActorArtHelper.SetIsFrozen(true);
                        FrozenBox.FrozenActor = actor;
                        actor.transform.parent = FrozenBox.transform;
                        actor.CurMoveAttempt = Vector3.zero;
                        actor.ActorPushHelper.TriggerOut = false;
                        actor.MovementState = Actor.MovementStates.Frozen;
                        actor.ForbidAction = true;
                        transform.rotation = Quaternion.identity;
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

            FXManager.Instance.PlayFX(beforeFrozenLevel < afterFrozenLevel ? actor.FrozeFX : actor.ThawFX, transform.position, 1f);
        }
    }
}