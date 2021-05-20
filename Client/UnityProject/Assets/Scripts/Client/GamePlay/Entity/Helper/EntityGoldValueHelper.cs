using BiangLibrary;
using Sirenix.OdinInspector;
using UnityEngine;

public class EntityGoldValueHelper : BoxMonoHelper
{
    [SerializeField]
    private Renderer[] Renderers;

    public AnimationCurve GoldRatioMappingCurve;

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
    }

    [LabelText("上下限比例系数")]
    [MinMaxSlider(0, 2f, true)]
    public Vector2 RangeRatioFactor = new Vector2(0.7f, 1.3f);

    public RandomType RandomType = RandomType.Uniform;

    private bool isRandomTypeBinary => RandomType == RandomType.BinaryUp || RandomType == RandomType.BinaryDown;

    [Range(0, 1)]
    [LabelText("二项分布概率")]
    [ShowIf("isRandomTypeBinary")]
    public float BinaryP = 0.8f;

    [Range(0, 1)]
    [LabelText("完全无金概率")]
    public float NoGoldProbability;

    private MaterialPropertyBlock materialPropertyBlock;

    private void OnChangeGoldValue(int goldValue, float maxGoldValue)
    {
        float goldRatio = GoldRatioMappingCurve.Evaluate(goldValue);
        foreach (Renderer renderer in Renderers)
        {
            materialPropertyBlock = new MaterialPropertyBlock();
            materialPropertyBlock.SetFloat("_GoldRatio", goldRatio);
            renderer.SetPropertyBlock(materialPropertyBlock);
        }
    }

    public override void ApplyEntityExtraStates(EntityDataExtraStates entityDataExtraStates)
    {
        base.ApplyEntityExtraStates(entityDataExtraStates);
        int baseGoldValue = Entity.EntityStatPropSet.Gold.Value;
        float MinGoldProbability = baseGoldValue * RangeRatioFactor.x;
        float MaxGoldProbability = baseGoldValue * RangeRatioFactor.y;
        if (entityDataExtraStates.R_GoldValue)
        {
            Entity.EntityStatPropSet.Gold.SetValue(entityDataExtraStates.GoldValue);
            OnChangeGoldValue(Entity.EntityStatPropSet.Gold.Value, MaxGoldProbability);
        }
        else
        {
            int dropNum = 0;
            if (!NoGoldProbability.ProbabilityBool())
            {
                dropNum = CommonUtils.GetRandomFromFloatProbability(RandomType, MinGoldProbability, MaxGoldProbability, BinaryP);
            }

            Entity.EntityStatPropSet.Gold.SetValue(dropNum);
            OnChangeGoldValue(Entity.EntityStatPropSet.Gold.Value, MaxGoldProbability);
        }
    }

    public override void RecordEntityExtraStates(EntityDataExtraStates entityDataExtraStates)
    {
        base.RecordEntityExtraStates(entityDataExtraStates);
        entityDataExtraStates.R_GoldValue = true;
        entityDataExtraStates.GoldValue = Entity.EntityStatPropSet.Gold.Value;
    }
}