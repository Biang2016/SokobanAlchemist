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
            UIFormTypes.Fixed,
            UIFormShowModes.Normal,
            UIFormLucencyTypes.Penetrable);
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
    private float currentMinimumLoadingDuration;
    private float currentLoadingTick;

    public void Clear()
    {
        progressRatio = 0;
        progressRatio_SmoothDampVelocity = 0;
        ProgressBar.value = 0;
        currentText = "";
        RefreshTick = 0;
        currentMinimumLoadingDuration = 0;
        currentLoadingTick = 0;
    }

    public void SetBackgroundAlpha(float alpha)
    {
        BackgroundImage.color = new Color(BackgroundImage.color.r, BackgroundImage.color.g, BackgroundImage.color.b, alpha);
    }

    public void SetMinimumLoadingDuration(float duration)
    {
        currentMinimumLoadingDuration = duration;
    }

    public float GetRemainingLoadingDuration()
    {
        if (currentLoadingTick < currentMinimumLoadingDuration)
        {
            return currentMinimumLoadingDuration - currentLoadingTick;
        }
        else
        {
            return 0.1f;
        }
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
        currentLoadingTick += Time.fixedDeltaTime;
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