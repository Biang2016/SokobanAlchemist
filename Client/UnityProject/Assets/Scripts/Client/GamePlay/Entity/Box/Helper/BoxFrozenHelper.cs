using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


public class BoxFrozenHelper : EntityFrozenHelper
{
    public override void FrozeIntoIceBlock(int beforeFrozenLevel, int afterFrozenLevel)
    {
        //if (afterFrozenLevel == 0)
        //{
        //    Box.AddRigidbody();
        //    Box.SetModelSmoothMoveLerpTime(0.02f);
        //    if (FrozenBox)
        //    {
        //        Box.transform.parent = BattleManager.Instance.BoxContainerRoot;
        //        FrozenBox.FrozenBox = null;
        //        FrozenBox.DeleteSelf();
        //        FrozenBox = null;
        //    }

        //    Thaw();
        //    FXManager.Instance.PlayFX(Box.ThawFX, transform.position, 1f);
        //}
        //else
        //{
        //    if (FrozenBox)
        //    {
        //    }
        //    else
        //    {
        //        Box.SnapToGrid();
        //        WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByGridPosition(Box.CurWorldGP);
        //        if (module)
        //        {
        //            FrozenBox = module.GenerateBox(ConfigManager.Box_EnemyFrozenBoxIndex, module.WorldGPToLocalGP(Box.CurWorldGP));
        //            if (FrozenBox)
        //            {
        //                List<BoxFunctionBase> boxFrozenBoxFunctions = Box.RawFrozenBoxFunctions.Clone();
        //                foreach (BoxFunctionBase abf in boxFrozenBoxFunctions)
        //                {
        //                    FrozenBox.AddNewBoxFunction(abf);
        //                    FrozenBox.BoxFunctions.Add(abf);
        //                }

        //                FrozenBox.FrozenBox = Box;
        //                Box.transform.parent = FrozenBox.transform;
        //                Box.CurMoveAttempt = Vector3.zero;
        //                Box.BoxPushHelper.TriggerOut = false;
        //                Box.MovementState = Box.MovementStates.Frozen;
        //                Box.RemoveRigidbody();
        //                transform.rotation = Quaternion.identity;
        //                FrozeModelRoot.SetActive(true);
        //            }
        //            else
        //            {
        //                Debug.Log("生成冻结箱子失败");
        //            }
        //        }
        //        else
        //        {
        //            Debug.LogError("角色不在任一模组内");
        //        }
        //    }

        //    for (int index = 0; index < FrozeModels.Length; index++)
        //    {
        //        GameObject frozeModel = FrozeModels[index];
        //        frozeModel.SetActive(index == afterFrozenLevel - 1);
        //    }

        //    FXManager.Instance.PlayFX(beforeFrozenLevel < afterFrozenLevel ? Box.FrozeFX : Box.ThawFX, transform.position, 1f);
        //}
    }
}