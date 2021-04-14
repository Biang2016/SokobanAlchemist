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

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
        if (UseRandomScale) transform.localScale = Vector3.one;
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
        if (UseModelVariants)
        {
            ModelVariantProbability randomMV = CommonUtils.GetRandomWithProbabilityFromList(ModelVariants);
            if (randomMV != null)
            {
                foreach (ModelVariantProbability mv in ModelVariants)
                {
                    mv.GameObject.SetActive(mv == randomMV);
                }
            }
        }

        if (UseRandomScale) Pivot.localScale = new Vector3(GaussianRandom.Range(RandomScaleMean.x, RandomScaleRadius.x), GaussianRandom.Range(RandomScaleMean.y, RandomScaleRadius.y), GaussianRandom.Range(RandomScaleMean.z, RandomScaleRadius.z));
        if (UseRandomOrientation) Pivot.localRotation = Quaternion.Euler(0, 90f * Random.Range(0, 4), 0);
    }

    [BoxGroup("模型变种")]
    [LabelText("使用模型变种")]
    public bool UseModelVariants = false;

    [BoxGroup("模型变种")]
    [LabelText("模型变种类型")]
    [ShowIf("UseModelVariants")]
    [ListDrawerSettings(ShowIndexLabels = false)]
    public List<ModelVariantProbability> ModelVariants = new List<ModelVariantProbability>();

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