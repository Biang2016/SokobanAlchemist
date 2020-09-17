﻿using BiangStudio.ObjectPool;
using UnityEngine;
using UnityEngine.UI;

public class InGameHealthBar : PoolObject
{
    private ActorBattleHelper ActorBattleHelper;
    public RectTransform RectTransform;
    public Image MainSliderFillImage;
    public Slider MainSlider;
    public Slider SubSlider;
    public Gradient HealthBarGradient;

    private int Length;
    private int Height;

    public override void OnRecycled()
    {
        base.OnRecycled();
        ActorBattleHelper = null;
    }

    public void Initialize(ActorBattleHelper helper, int length, int height)
    {
        Length = length;
        Height = height;
        RectTransform.sizeDelta = new Vector2(length, height) * CameraManager.Instance.FieldCamera.InGameUISize;
        ActorBattleHelper = helper;
        SubSlider.value = 0;
        SetHealthSliderValue(helper.Health, helper.MaxHealth);
        helper.OnHealthChanged += SetHealthSliderValue;
        MainSliderFillImage.color = HealthBarGradient.Evaluate(MainSlider.value);
    }

    public void SetHealthSliderValue(int left, int total)
    {
        if (total == 0)
        {
            MainSlider.value = 0f;
        }
        else
        {
            MainSlider.value = (float) left / total;
        }

        MainSliderFillImage.color = HealthBarGradient.Evaluate(MainSlider.value);
    }

    private float smoothDampVelocity;

    void Update()
    {
        if (!IsRecycled)
        {
            Vector2 screenPos = CameraManager.Instance.MainCamera.WorldToScreenPoint(ActorBattleHelper.HealthBarPivot.position);
            //Debug.Log($"ScreenPos: {screenPos}");
            //Debug.Log($"ScreenSize: {Screen.width}, {Screen.height}");
            float p_X = screenPos.x / Screen.width;
            float p_Y = screenPos.y / Screen.height;
            float size_X = ((RectTransform) RectTransform.parent).rect.width * p_X;
            float size_Y = ((RectTransform) RectTransform.parent).rect.height * p_Y;
            RectTransform.anchoredPosition = new Vector2(size_X, size_Y);

            RectTransform.sizeDelta = new Vector2(Length, Height) * CameraManager.Instance.FieldCamera.InGameUISize;
        }
    }

    void FixedUpdate()
    {
        SubSlider.value = Mathf.SmoothDamp(SubSlider.value, MainSlider.value, ref smoothDampVelocity, 0.5f, 1, Time.fixedDeltaTime);
    }
}