using System;
using System.Collections;
using BiangStudio.ObjectPool;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class BattleIndicator : PoolObject
{
    [SerializeField]
    protected Transform ArtRoot;

    [SerializeField]
    protected MeshRenderer MainMeshRenderer;

    void Awake()
    {
        mpb = new MaterialPropertyBlock();
    }

    public void SetShown(bool shown)
    {
        ArtRoot.gameObject.SetActive(shown);
    }

    public override void OnRecycled()
    {
        ArtRoot.transform.localPosition = Vector3.zero;
        ConstantYRotAngularVel = 0f;
        transform.DOPause();
        StopAllCoroutines();
        SetShown(false);
        OnProcess(0);
        base.OnRecycled();
    }

    public override void OnUsed()
    {
        base.OnUsed();
        SetShown(true);
    }

    public virtual void OnProcess(float ratio)
    {
    }

    protected MaterialPropertyBlock mpb;

    public void SetColor(Color color)
    {
        MainMeshRenderer.GetPropertyBlock(mpb);
        mpb.SetColor("_Color", color);
        mpb.SetColor("_EmissionColor", color);
        MainMeshRenderer.SetPropertyBlock(mpb);
    }

    public BattleIndicator SetColor(Gradient gradient, float duration, bool loop)
    {
        StartCoroutine(Co_ChangeColorByGradient(gradient, duration, loop));
        return this;
    }

    IEnumerator Co_ChangeColorByGradient(Gradient gradient, float duration, bool loop)
    {
        float tick = 0f;
        while (tick < duration || loop)
        {
            tick += Time.deltaTime;
            SetColor(gradient.Evaluate(tick / duration));
            yield return null;
        }
    }

    private float ConstantYRotAngularVel;

    public BattleIndicator SetArtPositionOffset(Vector3 offset)
    {
        ArtRoot.transform.localPosition = offset;
        return this;
    }

    public BattleIndicator SetInitYRot(float yRot)
    {
        transform.eulerAngles = new Vector3(0f, yRot, 0);
        return this;
    }

    public BattleIndicator RotateConstantly(float constantYRotAngularVel = 0)
    {
        ConstantYRotAngularVel = constantYRotAngularVel;
        return this;
    }

    void Update()
    {
        transform.Rotate(0, ConstantYRotAngularVel * Time.deltaTime, 0);
    }
}