using BiangLibrary.GamePlay.UI;
using UnityEngine;
using UnityEngine.UI;

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
    public AK.Wwise.Event OnDisplay;

    public override void Display()
    {
        base.Display();
        Anim.SetTrigger("Show");
        OnDisplay?.Post(gameObject);

        if (ControlManager.Instance.CurrentControlScheme == ControlManager.ControlScheme.KeyboardMouse)
        {
            KeyBindImage.sprite = Sprite_Keyboard;
        }
        else if (ControlManager.Instance.CurrentControlScheme == ControlManager.ControlScheme.GamePad)
        {
            KeyBindImage.sprite = Sprite_Controller;
        }
    }

    public Image KeyBindImage;
    public Sprite Sprite_Keyboard;
    public Sprite Sprite_Controller;

    public override void Hide()
    {
        base.Hide();
        Anim.SetTrigger("Hide");
    }
}