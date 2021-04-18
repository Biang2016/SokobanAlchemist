using System;
using System.Collections.Generic;
using BiangLibrary.ObjectPool;
using UnityEngine;
using UnityEngine.UI;

public class DiscreteStatPointIndicator : PoolObject
{
    public Animator Anim;

    public Slider Slider;
    public Image SliderImage;
    public Image BackgroundImage;
    public Image FullImage;

    public Sprite ActionPointSprite;
    public Color ActionFullImageColor;
    public Sprite FireElementFragmentSprite;
    public Color FireElementFragmentFullImageColor;
    public Sprite IceElementFragmentSprite;
    public Color IceElementFragmentFullImageColor;
    public Sprite LightningElementFragmentSprite;
    public Color LightningElementFragmentFullImageColor;

    private Dictionary<EntityStatType, Sprite> SpriteDict = new Dictionary<EntityStatType, Sprite>();
    private Dictionary<EntityStatType, Color> FullImageColorDict = new Dictionary<EntityStatType, Color>();

    private bool available = false;

    public bool Available
    {
        get { return available; }
        set
        {
            if (available != value)
            {
                available = value;
                Anim.SetBool("Enable", available);
                Anim.SetTrigger("Jump");
            }
        }
    }

    private float ratio = 0f;

    public float Ratio
    {
        get { return ratio; }
        set
        {
            if (ratio != value)
            {
                ratio = value;
                Slider.value = ratio;
                BackgroundImage.enabled = ratio < 1;
                Anim.SetTrigger("JustJump");
            }
        }
    }

    private bool full;

    public bool Full
    {
        get { return full; }
        set
        {
            if (full != value)
            {
                full = value;
                FullImage.enabled = full;
            }
        }
    }

    public override void OnRecycled()
    {
        base.OnRecycled();
        Available = false;
        Ratio = 1f;
        Full = false;
    }

    public override void OnUsed()
    {
        base.OnUsed();
    }

    private EntityStatType EntityStatType;

    public void Initialize(EntityStatType statType)
    {
        Ratio = 1f;
        Slider.value = 1f;
        BackgroundImage.enabled = false;

        Full = false;
        FullImage.enabled = false;

        EntityStatType = statType;

        SpriteDict.Add(EntityStatType.ActionPoint, ActionPointSprite);
        SpriteDict.Add(EntityStatType.FireElementFragment, FireElementFragmentSprite);
        SpriteDict.Add(EntityStatType.IceElementFragment, IceElementFragmentSprite);
        SpriteDict.Add(EntityStatType.LightningElementFragment, LightningElementFragmentSprite);

        FullImageColorDict.Add(EntityStatType.ActionPoint, ActionFullImageColor);
        FullImageColorDict.Add(EntityStatType.FireElementFragment, FireElementFragmentFullImageColor);
        FullImageColorDict.Add(EntityStatType.IceElementFragment, IceElementFragmentFullImageColor);
        FullImageColorDict.Add(EntityStatType.LightningElementFragment, LightningElementFragmentFullImageColor);

        SliderImage.sprite = SpriteDict[EntityStatType];
        FullImage.color = FullImageColorDict[EntityStatType];
        Available = false;
    }

    public void JumpRed()
    {
        Anim.SetTrigger("JumpRed");
    }
}