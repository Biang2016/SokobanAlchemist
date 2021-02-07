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

    public void SetProgress(float progress, string text)
    {
        ProgressBar.value = progress;
        InformationText.text = $" {(progress * 100):##.#}%" + text;
    }
}