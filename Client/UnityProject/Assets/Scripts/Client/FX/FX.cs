using System;
using BiangLibrary.CloneVariant;
using BiangLibrary.ObjectPool;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class FX : PoolObject
{
    private ParticleSystem ParticleSystem;

    public UnityAction OnFXEnd;

    public override void OnRecycled()
    {
        Stop();
        OnFXEnd?.Invoke();
        base.OnRecycled();
        OnFXEnd = null;
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;
    }

    void Awake()
    {
        ParticleSystem = GetComponentInChildren<ParticleSystem>();
    }

    private void FixedUpdate()
    {
        if (!IsRecycled)
        {
            if (ParticleSystem.isStopped)
            {
                PoolRecycle();
            }
        }
    }

    public void Play()
    {
        ParticleSystem.Play(true);
    }

    public void Stop()
    {
        ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}

[Serializable]
public class FXConfig : IClone<FXConfig>
{
    public override string ToString()
    {
        return FXType.TypeName;
    }

    [SerializeField]
    [HideLabel]
    private TypeSelectHelper FXType = new TypeSelectHelper {TypeDefineType = TypeDefineType.FX};

    [SerializeField]
    [LabelText("使用曲线")]
    private bool UseCurve = false;

    [SerializeField]
    [LabelText("固定尺寸")]
    private float FXScale = 1.0f;

    [SerializeField]
    [LabelText("尺寸(曲线)")]
    private AnimationCurve AnimationCurve = new AnimationCurve();

    private FXConfig()
    {
    }

    public FXConfig(string defaultFXTypeName = "None", float fxScale = 1f, bool useCurve = false, AnimationCurve animationCurve = null)
    {
        FXScale = fxScale;
        UseCurve = useCurve;
        if (useCurve) AnimationCurve = animationCurve;
        FXType.TypeSelection = defaultFXTypeName;
        FXType.RefreshGUID();
    }

    public bool Empty => string.IsNullOrEmpty(FXType.TypeName) || FXType.TypeName == "None" || (!UseCurve && FXScale.Equals(0));
    public string TypeName => FXType.TypeName;

    public float GetScale(float evaluator = 0f)
    {
        if (UseCurve) return AnimationCurve.Evaluate(evaluator);
        else return FXScale;
    }

    public FXConfig Clone()
    {
        FXConfig newConfig = new FXConfig(FXType.TypeName, FXScale, UseCurve, AnimationCurve);
        return newConfig;
    }

    public void CopyDataFrom(FXConfig srcData)
    {
        FXType.CopyDataFrom(srcData.FXType);
        UseCurve = srcData.UseCurve;
        FXScale = srcData.FXScale;
        AnimationCurve = srcData.AnimationCurve;
    }
}