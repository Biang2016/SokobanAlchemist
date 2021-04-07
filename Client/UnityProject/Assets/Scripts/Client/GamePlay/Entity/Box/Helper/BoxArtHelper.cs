using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoxArtHelper : EntityArtHelper
{
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
            int showIndex = Random.Range(0, ModelVariants.Length);
            for (int i = 0; i < ModelVariants.Length; i++)
            {
                ModelVariants[i].SetActive(i == showIndex);
            }
        }

        if (UseRandomScale)
        {
            transform.localScale = new Vector3(GaussianRandom.Range(RandomScaleMean.x, RandomScaleRadius.x), GaussianRandom.Range(RandomScaleMean.y, RandomScaleRadius.y), GaussianRandom.Range(RandomScaleMean.z, RandomScaleRadius.z));
        }

        if (UseRandomOrientation)
        {
            Entity.SwitchEntityOrientation((GridPosR.Orientation) Random.Range(0, 4));
        }
    }

    [BoxGroup("模型变种")]
    [LabelText("使用模型变种")]
    public bool UseModelVariants = false;

    [BoxGroup("模型变种")]
    [LabelText("模型变种类型")]
    [ShowIf("UseModelVariants")]
    public GameObject[] ModelVariants = new GameObject[1];

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