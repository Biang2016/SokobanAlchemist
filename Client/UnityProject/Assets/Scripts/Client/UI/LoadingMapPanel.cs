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
    private Text InformationText;

    [SerializeField]
    private Slider ProgressBar;

    private float progressRatio;
    private float progressRatio_SmoothDampVelocity;
    private string currentText;

    public void Clear()
    {
        ProgressBar.value = 0;
        currentText = "";
    }

    public void SetProgress(float progress, string text)
    {
        progressRatio = progress;
        ProgressBar.value = progress;
        currentText = text;
        Refresh();
    }

    void FixedUpdate()
    {
        Refresh();
    }

    public void Refresh()
    {
        ProgressBar.value = Mathf.SmoothDamp(ProgressBar.value, progressRatio, ref progressRatio_SmoothDampVelocity, 0.5f, 1, Time.deltaTime);
        InformationText.text = $"{(ProgressBar.value * 100):##.#}%  " + currentText;
    }
}