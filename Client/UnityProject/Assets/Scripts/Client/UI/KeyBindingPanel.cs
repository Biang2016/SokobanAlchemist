using BiangLibrary.GamePlay.UI;
using UnityEngine;

public class KeyBindingPanel : BaseUIPanel
{
    void Awake()
    {
        UIType.InitUIType(
            false,
            false,
            false,
            UIFormTypes.Fixed,
            UIFormShowModes.Normal,
            UIFormLucencyTypes.Penetrable);
    }

    public Animator Anim;

    public override void Display()
    {
        base.Display();
        Anim.SetTrigger("Show");
    }

    public override void Hide()
    {
        base.Hide();
        Anim.SetTrigger("Hide");
    }
}