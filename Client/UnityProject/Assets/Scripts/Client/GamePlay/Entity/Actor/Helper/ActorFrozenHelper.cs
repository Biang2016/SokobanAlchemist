using System.Collections.Generic;
using BiangLibrary.CloneVariant;
using UnityEngine;

public class ActorFrozenHelper : EntityFrozenHelper
{
    internal Box FrozenBox;

    public override void FrozeIntoIceBlock(int beforeFrozenLevel, int afterFrozenLevel, int min, int max)
    {
        base.FrozeIntoIceBlock(beforeFrozenLevel, afterFrozenLevel, min, max);
        Actor actor = (Actor) Entity;

        if (afterFrozenLevel < 1)
        {
            actor.ForbidAction = false;
            actor.SetModelSmoothMoveLerpTime(actor.DefaultSmoothMoveLerpTime);
            if (FrozenBox)
            {
                actor.transform.parent = BattleManager.Instance.ActorContainerRoot;
                FrozenBox.FrozenActor = null;
                FrozenBox.DestroySelf();
                FrozenBox = null;
            }

            actor.RegisterInModule(actor.WorldGP, actor.EntityOrientation);
            Thaw();
            FXManager.Instance.PlayFX(actor.ThawFX, transform.position);
        }
        else
        {
            if (FrozenBox)
            {
                if (!FrozenBox.BoxFrozenBoxHelper.InitFrozen)
                {
                    InitFrozen(false);
                    FrozenBox.BoxFrozenBoxHelper.InitFrozen = true;
                }
            }
            else
            {
                InitFrozen(true);
            }

            for (int index = 0; index < FrozeModels.Length; index++)
            {
                GameObject frozeModel = FrozeModels[index];
                frozeModel.SetActive(index == afterFrozenLevel);
            }

            if (afterFrozenLevel >= FrozeModels.Length)
            {
                FrozeModels[FrozeModels.Length - 1].SetActive(true);
            }

            FXManager.Instance.PlayFX(beforeFrozenLevel < afterFrozenLevel ? actor.FrozeFX : actor.ThawFX, transform.position);
        }

        void InitFrozen(bool needFrozenBox)
        {
            actor.SnapToGrid();
            actor.UnRegisterFromModule(actor.WorldGP, actor.EntityOrientation);
            if (needFrozenBox)
            {
                WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(actor.WorldGP);
                if (module)
                {
                    EntityData entityData = new EntityData(ConfigManager.Box_EnemyFrozenBoxIndex, actor.EntityOrientation);
                    entityData.RawEntityExtraSerializeData.FrozenActorData = actor.CurrentEntityData;
                    FrozenBox = (Box) module.GenerateEntity(entityData, actor.WorldGP, true, false, false); // triggerAppear参数填true以确保冰冻箱子能正常生成
                    if (!FrozenBox) Debug.Log("生成冻结箱子失败");
                }
                else
                {
                    Debug.LogError("角色不在任一模组内");
                }
            }

            if (FrozenBox)
            {
                FrozenBox.FrozenActor = actor;
                FrozenBox.BoxFrozenBoxHelper.GenerateBoxIndicatorForFrozenActor(actor);
                FrozenBox.BoxFrozenBoxHelper.SetColliderSize_ForFrozenEnemyBox(actor.ActorWidth);
                actor.transform.parent = FrozenBox.transform;
                actor.CurMoveAttempt = Vector3.zero;
                actor.ActorPushHelper.TriggerOut = false;
                actor.ForbidAction = true;
                transform.localRotation = Quaternion.identity; // todo 这行干啥用
                FrozeModelRoot.SetActive(true);
                FrozenBox.BoxFrozenBoxHelper.InitFrozen = true;
            }
        }
    }
}