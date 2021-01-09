using UnityEngine;
using System.Collections;
using BiangLibrary.GamePlay.UI;
using UnityEngine.UI;

public class WinLosePanel : BaseUIPanel
{
    void Awake()
    {
        UIType.InitUIType(
            false,
            false,
            false,
            UIFormTypes.Normal,
            UIFormShowModes.Normal,
            UIFormLucencyTypes.Penetrable);

        InformationText.enabled = false;
    }

    [SerializeField]
    private Text InformationText;

    IEnumerator Co_ShowText(string text)
    {
        InformationText.text = text;
        InformationText.enabled = true;
        yield return new WaitForSeconds(2f);
        InformationText.enabled = false;
    }

    public IEnumerator Co_LoseGame()
    {
        yield return Co_ShowText("You Lose!");
    }

    public IEnumerator Co_WinGame()
    {
        yield return Co_ShowText("You Win!");
    }
}