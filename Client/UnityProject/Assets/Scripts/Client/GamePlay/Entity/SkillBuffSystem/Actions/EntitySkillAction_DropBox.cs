using System;
using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.GamePlay;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class EntitySkillAction_DropBox : EntitySkillAction, EntitySkillAction.IPureAction
{
    [LabelText("箱子类型概率")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<DropItemProbability> DropBoxList = new List<DropItemProbability>();

    public override void OnRecycled()
    {
    }

    protected override string Description => "掉落箱子";

    private static List<ushort> cached_DropBoxIndexList = new List<ushort>(16);

    public void Execute()
    {
        WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(Entity.WorldGP);
        if (module)
        {
            cached_DropBoxIndexList.Clear();
            foreach (DropItemProbability dropItemProbability in DropBoxList)
            {
                float dropProbability = Random.Range(dropItemProbability.ProbabilityMin, dropItemProbability.ProbabilityMax);
                ushort boxTypeIndex = ConfigManager.GetTypeIndex(TypeDefineType.Box, dropItemProbability.ItemType.TypeName);
                if (boxTypeIndex == 0) continue;
                while (dropProbability > 1)
                {
                    dropProbability -= 1;
                    cached_DropBoxIndexList.Add(boxTypeIndex);
                }

                if (dropProbability.ProbabilityBool())
                {
                    cached_DropBoxIndexList.Add(boxTypeIndex);
                }
            }

            int dropConeAngle = 0;
            if (cached_DropBoxIndexList.Count == 1) dropConeAngle = 0;
            else if (cached_DropBoxIndexList.Count <= 4) dropConeAngle = 15;
            else if (cached_DropBoxIndexList.Count <= 10) dropConeAngle = 30;
            else dropConeAngle = 45;

            foreach (ushort boxTypeIndex in cached_DropBoxIndexList)
            {
                GridPos3D worldGP = Entity.transform.position.ToGridPos3D();
                if (WorldManager.Instance.CurrentWorld.GenerateEntityOnWorldGPWithoutOccupy(boxTypeIndex, (GridPosR.Orientation) Random.Range(0, 4), worldGP, out Entity dropEntity))
                {
                    Vector2 horizontalVel = Random.insideUnitCircle.normalized * Mathf.Tan(dropConeAngle * Mathf.Deg2Rad);
                    Vector3 dropVel = Vector3.up + new Vector3(horizontalVel.x, 0, horizontalVel.y);
                    Box dropBox = (Box) dropEntity;
                    dropBox.DropOutFromEntity(dropVel.normalized * ClientGameManager.Instance.dropSpeed); // 抛射速度写死
                }
            }
        }
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_DropBox action = ((EntitySkillAction_DropBox) newAction);
        action.DropBoxList = DropBoxList.Clone<DropItemProbability, DropItemProbability>();
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_DropBox action = ((EntitySkillAction_DropBox) srcData);
        DropBoxList = action.DropBoxList.Clone<DropItemProbability, DropItemProbability>();
    }
}