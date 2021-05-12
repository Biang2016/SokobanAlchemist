using BiangLibrary.GamePlay.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConfirmPanel : BaseUIPanel
{
    [SerializeField]
    private TextMeshProUGUI DescText;

    [SerializeField]
    private Text LeftButtonText;

    [SerializeField]
    private Text RightButtonText;

    [SerializeField]
    private Button LeftButton;

    [SerializeField]
    private Button RightButton;

    [SerializeField]
    private InputField InputField1;

    [SerializeField]
    private InputField InputField2;

    [SerializeField]
    private InputField InputField3;

    [SerializeField]
    private Text InputFieldPlaceHolderText1;

    [SerializeField]
    private Text InputFieldPlaceHolderText2;

    [SerializeField]
    private Text InputFieldPlaceHolderText3;

    void Awake()
    {
        UIType.InitUIType(
            isClearStack: false,
            isESCClose: true,
            isClickElsewhereClose: true,
            uiForms_Type: UIFormTypes.PopUp,
            uiForms_ShowMode: UIFormShowModes.Return,
            uiForm_LucencyType: UIFormLucencyTypes.Blur);
    }

    private UnityAction ConfirmClick = null;

    public void Initialize(string descText, string leftButtonText, string rightButtonText, UnityAction leftButtonClick, UnityAction rightButtonClick, string inputFieldPlaceHolderText1 = null, string inputFieldPlaceHolderText2 = null, string inputFieldPlaceHolderText3 = null)
    {
        UIType.InitUIType(
            isClearStack: false,
            isESCClose: true,
            isClickElsewhereClose: true,
            uiForms_Type: UIFormTypes.PopUp,
            uiForms_ShowMode: UIFormShowModes.Return,
            uiForm_LucencyType: UIFormLucencyTypes.Blur);
        ConfirmClick = leftButtonClick;
        DescText.text = descText;

        LeftButton.onClick.RemoveAllListeners();
        LeftButton.gameObject.SetActive(leftButtonText != null);
        if (LeftButtonText) LeftButtonText.text = leftButtonText;
        LeftButton.onClick.AddListener(leftButtonClick ?? delegate { });

        RightButton.onClick.RemoveAllListeners();
        RightButton.gameObject.SetActive(rightButtonText != null);
        if (RightButtonText) RightButtonText.text = rightButtonText;
        RightButton.onClick.AddListener(rightButtonClick ?? delegate { });

        if (inputFieldPlaceHolderText1 != null)
        {
            InputFieldPlaceHolderText1.text = inputFieldPlaceHolderText1;
            InputField1.ActivateInputField();
        }

        InputField1.gameObject.SetActive(inputFieldPlaceHolderText1 != null);

        if (inputFieldPlaceHolderText2 != null)
        {
            InputFieldPlaceHolderText2.text = inputFieldPlaceHolderText2;
            InputField2.ActivateInputField();
        }

        InputField2.gameObject.SetActive(inputFieldPlaceHolderText2 != null);

        if (InputFieldPlaceHolderText3 != null)
        {
            InputFieldPlaceHolderText3.text = inputFieldPlaceHolderText3;
            InputField3.ActivateInputField();
        }

        InputField3.gameObject.SetActive(inputFieldPlaceHolderText3 != null);
    }

    public AK.Wwise.Event OnDisplay;
    public AK.Wwise.Event OnHide;

    public override void Display()
    {
        base.Display();
        OnDisplay?.Post(gameObject);
        LeftButton.Select();
    }

    public override void Hide()
    {
        base.Hide();
        OnHide?.Post(gameObject);
    }

    public void OnButtonHover()
    {
        WwiseAudioManager.Instance.PlayCommonAudioSound(WwiseAudioManager.CommonAudioEvent.UI_ButtonHover, WwiseAudioManager.Instance.gameObject);
    }

    public void OnButtonClick()
    {
        WwiseAudioManager.Instance.PlayCommonAudioSound(WwiseAudioManager.CommonAudioEvent.UI_ButtonClick, WwiseAudioManager.Instance.gameObject);
    }

    public string InputText1 => InputField1.text;
    public string InputText2 => InputField2.text;
    public string InputText3 => InputField3.text;
}