using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElementBottle : MonoBehaviour
{
    public TextMeshProUGUI KeyBind_Text;
    public TextMeshProUGUI CurrentValue_Text;
    public TextMeshProUGUI MaxValue_Text;
    public Image Image;
    public Sprite[] Sprites;
    public Animator FillAnim;

    void Start()
    {
        RefreshValue(0, 0, 1);
    }

    public void Initialize()
    {
        if (KeyBind_Text) KeyBind_Text.text = "";
    }

    public void Initialize(PlayerControllerHelper.KeyBind keyBind)
    {
        if (KeyBind_Text) KeyBind_Text.text = PlayerControllerHelper.KeyMappingStrDict[keyBind];
    }

    public void RefreshValue(int currentValue, int minValue, int maxValue)
    {
        int ratio = Mathf.CeilToInt((float) currentValue / maxValue * 10f);
        Image.sprite = Sprites[ratio];
        CurrentValue_Text.text = currentValue + "/";
        if (MaxValue_Text)
        {
            CurrentValue_Text.text = currentValue + "/";
            MaxValue_Text.text = maxValue.ToString();
        }
        else
        {
            CurrentValue_Text.text = currentValue.ToString();
        }
    }

    public void OnStatLowWarning()
    {
        if (FillAnim) FillAnim.SetTrigger("Warning");
    }
}