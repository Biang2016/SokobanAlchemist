using System;
using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class EntityPassiveSkillAction_DropBox : EntityPassiveSkillAction, EntityPassiveSkillAction.IPureAction
{
    [LabelText("箱子类型概率")]
    [ListDrawerSettings(ListElementLabelName = "Description")]
    public List<BoxNameWithProbability> DropBoxList = new List<BoxNameWithProbability>();

    public int DropBoxCountMin;
    public int DropBoxCountMax;

    public float DropConeAngle = 0f;
    public float DropVelocity = 13f;

    public override void OnRecycled()
    {
    }

    protected override string Description => "掉落箱子";

    public void Execute()
    {
        WorldModule module = WorldManager.Instance.CurrentWorld.GetModuleByWorldGP(Entity.WorldGP);
        if (module)
        {
            int dropBoxCount = Random.Range(DropBoxCountMin, DropBoxCountMax + 1);
            for (int i = 0; i < dropBoxCount; i++)
            {
                BoxNameWithProbability bp = CommonUtils.GetRandomWithProbabilityFromList(DropBoxList);
                ushort boxIndex = ConfigManager.GetTypeIndex(TypeDefineType.Box, bp.BoxTypeName.TypeName);
                if (boxIndex == 0) return;
                Box box = GameObjectPoolManager.Instance.BoxDict[boxIndex].AllocateGameObject<Box>(null);
                GridPos3D worldGP = Entity.WorldGP;
                box.Setup(boxIndex, (GridPosR.Orientation) Random.Range(0, 4), Entity.InitWorldModuleGUID);
                box.Initialize(worldGP, module, 0, false, Box.LerpType.DropFromEntity);
                Vector2 horizontalVel = Random.insideUnitCircle.normalized * Mathf.Tan(DropConeAngle * Mathf.Deg2Rad);
                Vector3 dropVel = Vector3.up + new Vector3(horizontalVel.x, 0, horizontalVel.y);
                box.DropFromEntity(dropVel.normalized * DropVelocity);
            }
        }
    }

    protected override void ChildClone(EntityPassiveSkillAction newAction)
    {
        base.ChildClone(newAction);
        EntityPassiveSkillAction_DropBox action = ((EntityPassiveSkillAction_DropBox) newAction);
        action.DropBoxList = DropBoxList.Clone();
        action.DropBoxCountMin = DropBoxCountMin;
        action.DropBoxCountMax = DropBoxCountMax;
        action.DropConeAngle = DropConeAngle;
        action.DropVelocity = DropVelocity;
    }

    public override void CopyDataFrom(EntityPassiveSkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntityPassiveSkillAction_DropBox action = ((EntityPassiveSkillAction_DropBox) srcData);
        DropBoxList = action.DropBoxList.Clone();
        DropBoxCountMin = action.DropBoxCountMin;
        DropBoxCountMax = action.DropBoxCountMax;
        DropConeAngle = action.DropConeAngle;
        DropVelocity = action.DropVelocity;
    }
}