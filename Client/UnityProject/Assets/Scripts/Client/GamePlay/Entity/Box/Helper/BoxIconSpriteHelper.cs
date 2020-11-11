using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class BoxIconSpriteHelper : BoxMonoHelper
{
    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
    }

    [ValueDropdown("GetAllBoxIconTypeNames")]
    [OnValueChanged("ChangeSprite")]
    [LabelText("图标类型")]
    public string BoxIconType;

    [OnValueChanged("ChangeColor")]
    [LabelText("图标颜色")]
    public Color SpriteColor = Color.white;

    public void ChangeSprite()
    {
        Sprite sprite = Resources.Load<Sprite>($"BoxIcons/{BoxIconType}");
        Top.sprite = sprite;
        Bottom.sprite = sprite;
        Left.sprite = sprite;
        Right.sprite = sprite;
        Front.sprite = sprite;
        Back.sprite = sprite;
    }

    public SpriteRenderer Top;
    public SpriteRenderer Bottom;
    public SpriteRenderer Left;
    public SpriteRenderer Right;
    public SpriteRenderer Front;
    public SpriteRenderer Back;

    public void ChangeColor()
    {
        Top.color = SpriteColor;
        Bottom.color = SpriteColor;
        Left.color = SpriteColor;
        Right.color = SpriteColor;
        Front.color = SpriteColor;
        Back.color = SpriteColor;
    }
}