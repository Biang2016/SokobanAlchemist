using System;
using System.Collections.Generic;
using BiangLibrary.ObjectPool;
using UnityEngine;
using UnityEngine.UI;

public class DiscreteStatPointIndicator : PoolObject
{
    public Animator Anim;

    public Image Image;

    public Sprite ActionPointSprite;
    public Sprite FireElementFragmentSprite;
    public Sprite IceElementFragmentSprite;
    public Sprite LightningElementFragmentSprite;

    private Dictionary<EntityStatType, Sprite> SpriteDict = new Dictionary<EntityStatType, Sprite>();

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

    public override void OnRecycled()
    {
        base.OnRecycled();
        Available = false;
    }

    public override void OnUsed()
    {
        base.OnUsed();
    }

    private EntityStatType EntityStatType;

    public void Initialize(EntityStatType statType)
    {
        EntityStatType = statType;
        SpriteDict.Add(EntityStatType.ActionPoint, ActionPointSprite);
        SpriteDict.Add(EntityStatType.FireElementFragment, FireElementFragmentSprite);
        SpriteDict.Add(EntityStatType.IceElementFragment, IceElementFragmentSprite);
        SpriteDict.Add(EntityStatType.LightningElementFragment, LightningElementFragmentSprite);
        Image.sprite = SpriteDict[EntityStatType];
        Available = false;
    }

    public void JumpRed()
    {
        Anim.SetTrigger("JumpRed");
    }
}