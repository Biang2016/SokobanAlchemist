using UnityEngine;
using BiangLibrary.GamePlay.UI;
using UnityEngine.UI;

public class LoadingMapPanel : BaseUIPanel
{
    void Awake()
    {
        UIType.InitUIType(
            false,
            false,
            false,
            UIFormTypes.Normal,
            UIFormShowModes.Normal,
            UIFormLucencyTypes.ImPenetrable);
        InformationText.text = "";
        ProgressBar.value = 0;
    }

    [SerializeField]
    private Image BackgroundImage;

    [SerializeField]
    private Text InformationText;

    [SerializeField]
    private Slider ProgressBar;

    private float progressRatio;
    private float progressRatio_SmoothDampVelocity;
    private string currentText;

    public void Clear()
    {
        progressRatio = 0;
        progressRatio_SmoothDampVelocity = 0;
        ProgressBar.value = 0;
        currentText = "";
        RefreshTick = 0;
    }

    public void SetBackgroundAlpha(float alpha)
    {
        BackgroundImage.color = new Color(BackgroundImage.color.r, BackgroundImage.color.g, BackgroundImage.color.b, alpha);
    }

    public void SetProgress(float progress, string text)
    {
        progressRatio = progress;
        currentText = text;
        Refresh();
    }

    private float RefreshInterval = 0.2f;
    private float RefreshTick = 0.2f;

    void FixedUpdate()
    {
        RefreshTick += Time.fixedDeltaTime;
        if (RefreshTick > RefreshInterval)
        {
            RefreshTick -= RefreshInterval;
            Refresh();
        }
    }

    public void Refresh()
    {
        ProgressBar.value = Mathf.SmoothDamp(ProgressBar.value, progressRatio, ref progressRatio_SmoothDampVelocity, 0.5f, 1, Time.fixedDeltaTime);
        InformationText.text = $"{(ProgressBar.value * 100):##.#}%  " + currentText;
    }
}