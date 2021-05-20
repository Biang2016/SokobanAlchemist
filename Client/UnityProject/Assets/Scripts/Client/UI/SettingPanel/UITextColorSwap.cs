using TMPro;
using UnityEngine;

public class UITextColorSwap : MonoBehaviour
{
    public TextMeshProUGUI Text;
    public Color HoverColor;
    public Color NormalColor;
    public Color PressedColor;

    public void OnMouseEnterButton()
    {
        Text.color = HoverColor;
    }

    public void OnMouseExitButton()
    {
        Text.color = NormalColor;
    }

    public void OnMousePressButton()
    {
        Text.color = PressedColor;
    }
}