using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SphereBottle : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI Value_Text;

    [SerializeField]
    private Image FillImage;

    [SerializeField]
    private Image SurfaceImage;

    [SerializeField]
    private Animator FillAnim;

    [SerializeField]
    private bool HealthLowWarning;

    void Start()
    {
        RefreshValue(0, 0, 1);
    }

    public void RefreshValue(int currentValue, int minValue, int maxValue)
    {
        float ratio = 0f;
        if (maxValue == 0)
        {
            ratio = 0f;
        }
        else
        {
            ratio = (float) currentValue / maxValue;
        }

        FillImage.fillAmount = ratio;

        if (HealthLowWarning)
        {
            if (ratio >= 0.2f)
            {
                if (FillAnim) FillAnim.SetTrigger("ValueChange");
            }
        }
        else
        {
            if (FillAnim) FillAnim.SetTrigger("ValueChange");
        }

        Value_Text.text = currentValue + "/" + maxValue;

        float width = SurfaceImage.rectTransform.sizeDelta.x;
        float r = width / 2f;
        float pos_y = 2 * r * ratio - r;
        float scale = Mathf.Sqrt(r * r - pos_y * pos_y) / r;
        SurfaceImage.rectTransform.localScale = Vector3.one * scale;
        SurfaceImage.rectTransform.anchoredPosition = new Vector2(0, pos_y);
    }

    public void OnStatLowWarning()
    {
        if (FillAnim) FillAnim.SetTrigger("LowWarning");
    }
}