using System;
using System.Collections.Generic;
using BiangLibrary;
using BiangLibrary.CloneVariant;
using BiangLibrary.GameDataFormat;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoxArtHelper : EntityArtHelper
{
    public Transform Pivot;

    private int ModelIndex;
    private bool ShowDecoration;
    private int DecorationIndex;

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
        if (UseRandomScale) transform.localScale = Vector3.one;

        ModelIndex = 0;
        ShowDecoration = false;
        DecorationIndex = 0;
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
    }

    public override void ApplyEntityExtraSerializeData(EntityExtraSerializeData entityExtraSerializeData)
    {
        base.ApplyEntityExtraSerializeData(entityExtraSerializeData);
        if (UseModelVariants)
        {
            if (entityExtraSerializeData.EntityDataExtraStates.R_ModelIndex)
            {
                for (int i = 0; i < ModelVariants.Count; i++)
                {
                    ModelVariants[i].GameObject.SetActive(i == entityExtraSerializeData.EntityDataExtraStates.ModelIndex);
                }
            }
            else
            {
                ModelVariantProbability randomMV = CommonUtils.GetRandomWithProbabilityFromList(ModelVariants);
                if (randomMV != null)
                {
                    foreach (ModelVariantProbability mv in ModelVariants)
                    {
                        mv.GameObject.SetActive(mv == randomMV);
                    }

                    ModelIndex = ModelVariants.IndexOf(randomMV);
                }

                bool showDecoration = ShowDecorationProbabilityPercent.ProbabilityBool();
                ModelVariantProbability randomDec = CommonUtils.GetRandomWithProbabilityFromList(ProbablyShowModelDecorations);
                if (randomDec != null)
                {
                    foreach (ModelVariantProbability dec in ProbablyShowModelDecorations)
                    {
                        dec.GameObject.SetActive(showDecoration && dec == randomDec);
                    }

                    ShowDecoration = true;
                    DecorationIndex = ProbablyShowModelDecorations.IndexOf(randomDec);
                }
            }
        }

        if (UseRandomScale)
        {
            if (entityExtraSerializeData.EntityDataExtraStates.R_ModelScale)
            {
                Pivot.localScale = entityExtraSerializeData.EntityDataExtraStates.ModelScale;
            }
            else
            {
                Pivot.localScale = new Vector3(GaussianRandom.Range(RandomScaleMean.x, RandomScaleRadius.x), GaussianRandom.Range(RandomScaleMean.y, RandomScaleRadius.y), GaussianRandom.Range(RandomScaleMean.z, RandomScaleRadius.z));
            }
        }

        if (UseRandomOrientation)
        {
            if (entityExtraSerializeData.EntityDataExtraStates.R_ModelRotation)
            {
                Pivot.localRotation = entityExtraSerializeData.EntityDataExtraStates.ModelRotation;
            }
            else
            {
                Pivot.localRotation = Quaternion.Euler(0, 90f * Random.Range(0, 4), 0);
            }
        }
    }

    public override void RecordEntityExtraSerializeData(EntityExtraSerializeData entityExtraSerializeData)
    {
        base.RecordEntityExtraSerializeData(entityExtraSerializeData);
        if (UseModelVariants)
        {
            entityExtraSerializeData.EntityDataExtraStates.R_ModelIndex = true;
            entityExtraSerializeData.EntityDataExtraStates.ModelIndex = ModelIndex;

            entityExtraSerializeData.EntityDataExtraStates.R_DecoratorIndex = ShowDecoration;
            if (ShowDecoration) entityExtraSerializeData.EntityDataExtraStates.DecoratorIndex = DecorationIndex;
        }

        if (UseRandomScale)
        {
            entityExtraSerializeData.EntityDataExtraStates.R_ModelScale = true;
            entityExtraSerializeData.EntityDataExtraStates.ModelScale = Pivot.localScale;
        }

        if (UseRandomOrientation)
        {
            entityExtraSerializeData.EntityDataExtraStates.R_ModelRotation = true;
            entityExtraSerializeData.EntityDataExtraStates.ModelRotation = Pivot.localRotation;
        }
    }

    [BoxGroup("模型变种")]
    [LabelText("使用模型变种")]
    public bool UseModelVariants = false;

    [BoxGroup("模型变种")]
    [LabelText("模型变种类型")]
    [ShowIf("UseModelVariants")]
    [ListDrawerSettings(ShowIndexLabels = false)]
    public List<ModelVariantProbability> ModelVariants = new List<ModelVariantProbability>();

    [BoxGroup("模型变种")]
    [LabelText("出现装饰概率%")]
    [ShowIf("UseModelVariants")]
    [ListDrawerSettings(ShowIndexLabels = false)]
    public int ShowDecorationProbabilityPercent = 0;

    [BoxGroup("模型变种")]
    [LabelText("可能出现装饰")]
    [ShowIf("UseModelVariants")]
    [ListDrawerSettings(ShowIndexLabels = false)]
    public List<ModelVariantProbability> ProbablyShowModelDecorations = new List<ModelVariantProbability>();

    [BoxGroup("随机尺寸")]
    [LabelText("随机尺寸")]
    public bool UseRandomScale = false;

    [BoxGroup("随机尺寸")]
    [LabelText("均值")]
    [ShowIf("UseRandomScale")]
    public Vector3 RandomScaleMean = Vector3.one;

    [BoxGroup("随机尺寸")]
    [LabelText("取值半径")]
    [ShowIf("UseRandomScale")]
    public Vector3 RandomScaleRadius = Vector3.zero;

    [BoxGroup("随机朝向")]
    [LabelText("随机朝向")]
    public bool UseRandomOrientation = false;
}

[Serializable]
[InlineProperty]
public class ModelVariantProbability : Probability, IClone<ModelVariantProbability>
{
    [HideLabel]
    public GameObject GameObject;

    [SerializeField]
    private int probability;

    public int Probability
    {
        get { return probability; }
        set { probability = value; }
    }

    [SerializeField]
    private bool isSingleton;

    public bool IsSingleton
    {
        get { return isSingleton; }
        set { isSingleton = value; }
    }

    public Probability ProbabilityClone()
    {
        ModelVariantProbability newData = new ModelVariantProbability();
        newData.GameObject = GameObject;
        newData.probability = probability;
        newData.isSingleton = isSingleton;
        return newData;
    }

    public ModelVariantProbability Clone()
    {
        return (ModelVariantProbability) ProbabilityClone();
    }
}