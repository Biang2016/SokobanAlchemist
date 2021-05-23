using System;
using System.Collections;
using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class EntitySkillAction_GenerateBox : EntitySkillAction, EntitySkillAction.IWorldGPAction
{
    [LabelText("箱子类型概率")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<BoxNameWithProbability> GenerateBoxList = new List<BoxNameWithProbability>();

    [LabelText("箱子堆叠层数")]
    public int GenerateBoxMaxLayer;

    [LabelText("生成延迟")]
    public float GenerateDelay = 0f;

    public override void OnRecycled()
    {
        if (coroutine != null) Entity.StopCoroutine(coroutine);
    }

    protected override string Description => "生成箱子";

    private Coroutine coroutine;

    public void ExecuteOnWorldGP(GridPos3D worldGP)
    {
        coroutine = Entity.StartCoroutine(Co_GenerateBox(worldGP));
    }

    IEnumerator Co_GenerateBox(GridPos3D worldGP)
    {
        for (int i = 0; i < GenerateBoxMaxLayer; i++)
        {
            yield return new WaitForSeconds(GenerateDelay);
            GridPos3D gridGP = worldGP + GridPos3D.Up * i;
            BoxNameWithProbability bp = CommonUtils.GetRandomWithProbabilityFromList(GenerateBoxList);
            ushort boxIndex = ConfigManager.GetTypeIndex(TypeDefineType.Box, bp.BoxTypeName.TypeName);
            if (boxIndex == 0) continue;

            EntityOccupationData entityOccupationData = ConfigManager.GetEntityOccupationData(boxIndex);
            WorldModule module = null;
            GridPos3D localGP = GridPos3D.Zero;
            Entity existedEntity = null;
            if (entityOccupationData.IsTriggerEntity)
            {
                WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(gridGP, 0, out module, out localGP);
            }
            else
            {
                existedEntity = WorldManager.Instance.CurrentWorld.GetImpassableEntityByGridPosition(gridGP, 0, out module, out localGP);
            }

            if (module != null && existedEntity == null)
            {
                EntityData entityData = new EntityData(boxIndex, (GridPosR.Orientation) Random.Range(0, 4));
                entityData.WorldGP = gridGP;
                entityData.LocalGP = localGP;
                module.GenerateEntity(entityData, gridGP, false, false, false);
            }
        }
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_GenerateBox action = ((EntitySkillAction_GenerateBox) newAction);
        action.GenerateBoxList = GenerateBoxList.Clone<BoxNameWithProbability, BoxNameWithProbability>();
        action.GenerateBoxMaxLayer = GenerateBoxMaxLayer;
        action.GenerateDelay = GenerateDelay;
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_GenerateBox action = ((EntitySkillAction_GenerateBox) srcData);
        GenerateBoxList = action.GenerateBoxList.Clone<BoxNameWithProbability, BoxNameWithProbability>();
        GenerateBoxMaxLayer = action.GenerateBoxMaxLayer;
        GenerateDelay = action.GenerateDelay;
    }
}