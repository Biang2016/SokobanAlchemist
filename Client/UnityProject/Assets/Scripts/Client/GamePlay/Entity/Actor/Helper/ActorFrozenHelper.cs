﻿using System.Collections.Generic;
using BiangStudio.CloneVariant;
using UnityEngine;
using Sirenix.OdinInspector;

public class ActorFrozenHelper : EntityFrozenHelper
{
    internal Box FrozenBox;

    public SmoothMove IceBlockSmoothMove;

    public override void FrozeIntoIceBlock(int beforeFrozenLevel, int afterFrozenLevel)
    {
        Actor actor = (Actor) Entity;
        if (afterFrozenLevel == 0)
        {
            actor.AddRigidbody();
            actor.SetModelSmoothMoveLerpTime(0.02f);
            if (FrozenBox)
            {
                actor.transform.parent = BattleManager.Instance.ActorContainerRoot;
                FrozenBox.FrozenActor = null;
                FrozenBox.DeleteSelf();
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
                WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByGridPosition(actor.CurWorldGP);
                if (module)
                {
                    FrozenBox = module.GenerateBox(ConfigManager.Box_EnemyFrozenBoxIndex, module.WorldGPToLocalGP(actor.CurWorldGP));
                    if (FrozenBox)
                    {
                        List<BoxFunctionBase> actorFrozenBoxFunctions = actor.RawFrozenBoxFunctions.Clone();
                        foreach (BoxFunctionBase abf in actorFrozenBoxFunctions)
                        {
                            FrozenBox.AddNewBoxFunction(abf);
                            FrozenBox.BoxFunctions.Add(abf);
                        }

                        FrozenBox.FrozenActor = actor;
                        actor.transform.parent = FrozenBox.transform;
                        actor.CurMoveAttempt = Vector3.zero;
                        actor.ActorPushHelper.TriggerOut = false;
                        actor.MovementState = Actor.MovementStates.Frozen;
                        actor.RemoveRigidbody();
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