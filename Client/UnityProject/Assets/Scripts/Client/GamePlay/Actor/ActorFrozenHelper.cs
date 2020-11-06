using System.Collections.Generic;
using BiangStudio.CloneVariant;
using UnityEngine;
using Sirenix.OdinInspector;

public class ActorFrozenHelper : ActorMonoHelper
{
    internal Box FrozenBox;

    [BoxGroup("冻结")]
    [LabelText("冻结Root")]
    public GameObject FrozeModelRoot;

    [BoxGroup("冻结")]
    [LabelText("冻结MeshRenderer")]
    public GameObject[] FrozeModels;

    public SmoothMove IceBlockSmoothMove;

    public override void OnRecycled()
    {
        base.OnRecycled();
        Thaw();
    }

    public override void OnUsed()
    {
        base.OnUsed();
        Thaw();
    }

    public void FrozeIntoIceBlock(int beforeFrozenLevel, int afterFrozenLevel)
    {
        if (afterFrozenLevel == 0)
        {
            Actor.AddRigidbody();
            Actor.SetModelSmoothMoveLerpTime(0.02f);
            if (FrozenBox)
            {
                Actor.transform.parent = BattleManager.Instance.ActorContainerRoot;
                FrozenBox.FrozenActor = null;
                FrozenBox.DeleteSelf();
                FrozenBox = null;
            }

            Thaw();
            FXManager.Instance.PlayFX(Actor.ThawFX, transform.position, 1f);
        }
        else
        {
            if (FrozenBox)
            {
            }
            else
            {
                Debug.Log("Actor Frozen: " + Actor.name);
                Actor.SnapToGrid();
                WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByGridPosition(Actor.CurWorldGP);
                if (module)
                {
                    FrozenBox = module.GenerateBox(ConfigManager.Box_EnemyFrozenBoxIndex, module.WorldGPToLocalGP(Actor.CurWorldGP));
                    if (FrozenBox)
                    {
                        List<BoxFunctionBase> actorFrozenBoxFunctions = Actor.RawFrozenBoxFunctions.Clone();
                        foreach (BoxFunctionBase abf in actorFrozenBoxFunctions)
                        {
                            FrozenBox.AddNewBoxFunction(abf);
                            FrozenBox.BoxFunctions.Add(abf);
                        }

                        FrozenBox.FrozenActor = Actor;
                        Actor.transform.parent = FrozenBox.transform;
                        Actor.CurMoveAttempt = Vector3.zero;
                        Actor.ActorPushHelper.TriggerOut = false;
                        Actor.MovementState = Actor.MovementStates.Frozen;
                        Actor.RemoveRigidbody();
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

            FXManager.Instance.PlayFX(beforeFrozenLevel < afterFrozenLevel ? Actor.FrozeFX : Actor.ThawFX, transform.position, 1f);
        }
    }

    private void Thaw()
    {
        for (int index = 0; index < FrozeModels.Length; index++)
        {
            GameObject frozeModel = FrozeModels[index];
            frozeModel.SetActive(false);
        }

        FrozeModelRoot.SetActive(false);
    }
}