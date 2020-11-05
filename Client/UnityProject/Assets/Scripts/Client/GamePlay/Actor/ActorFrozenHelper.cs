using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

public class ActorFrozenHelper : ActorMonoHelper
{
    private Box FrozenBox;

    [BoxGroup("冻结")]
    [LabelText("冻结Root")]
    public GameObject FrozeModelRoot;

    [BoxGroup("冻结")]
    [LabelText("冻结MeshRenderer")]
    public GameObject[] FrozeModels;

    [BoxGroup("冻结")]
    [LabelText("冻结特效")]
    [ValueDropdown("GetAllFXTypeNames", IsUniqueList = true, DropdownTitle = "选择FX类型", DrawDropdownForListElements = false, ExcludeExistingValuesInList = true)]
    public string FrozeFX;

    [BoxGroup("冻结")]
    [LabelText("解冻特效")]
    [ValueDropdown("GetAllFXTypeNames", IsUniqueList = true, DropdownTitle = "选择FX类型", DrawDropdownForListElements = false, ExcludeExistingValuesInList = true)]
    public string ThawFX;

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
            FXManager.Instance.PlayFX(ThawFX, transform.position, 1f);
        }
        else
        {
            Actor.CurMoveAttempt = Vector3.zero;
            Actor.MovementState = Actor.MovementStates.Frozen;
            Actor.ActorPushHelper.PushTriggerReset();

            if (FrozenBox)
            {
            }
            else
            {
                Actor.SnapToGrid();
                WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByGridPosition(Actor.CurWorldGP);
                if (module)
                {
                    FrozenBox = module.GenerateBox(ConfigManager.Box_EnemyFrozenBoxIndex, module.WorldGPToLocalGP(Actor.CurWorldGP));
                    FrozenBox.FrozenActor = Actor;
                    Actor.transform.parent = FrozenBox.transform;
                    Actor.RemoveRigidbody();
                    transform.rotation = Quaternion.identity;
                    FrozeModelRoot.SetActive(true);
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

            FXManager.Instance.PlayFX(beforeFrozenLevel < afterFrozenLevel ? FrozeFX : ThawFX, transform.position, 1f);
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