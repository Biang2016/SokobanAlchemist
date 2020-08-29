using BiangStudio.Singleton;
using UnityEngine;
using UnityEngine.UI;

namespace BiangStudio.GamePlay.UI
{
    public class UIMaskMgr : MonoSingleton<UIMaskMgr>
    {
        [SerializeField] private GameObject UIRoot; //UI根节点对象
        [SerializeField] private GameObject MaskPanel; //遮罩面板
        private Image MaskPanel_Image; //遮罩面板Image
        [SerializeField] private GameObject MaskPanel_Blur; //毛玻璃遮罩面板
        private Image MaskPanel_Blur_Image; //毛玻璃遮罩面板Image

        void Awake()
        {
            MaskPanel_Blur_Image = MaskPanel_Blur.GetComponent<Image>();
            MaskPanel_Image = MaskPanel.GetComponent<Image>();
        }

        /// <summary>
        /// 设置遮罩状态
        /// </summary>
        /// <param name="goDisplayUIForms">需要显示的UI窗体</param>
        /// <param name="lucencyType">显示透明度属性</param>
        public void SetMaskWindow(GameObject goDisplayUIForms, UIFormTypes uiFormType, UIFormLucencyTypes lucencyType)
        {
            bool needMask = uiFormType == UIFormTypes.PopUp || uiFormType == UIFormTypes.Fixed;
            bool needMask_Blur = uiFormType == UIFormTypes.PopUp;
            //顶层窗体下移
            UIRoot.transform.SetAsLastSibling();
            //启用遮罩窗体以及设置透明度
            switch (lucencyType)
            {
                //完全透明，不能穿透
                case UIFormLucencyTypes.Lucency:
                    MaskPanel.SetActive(needMask);
                    Color newColor0 = new Color(1, 1, 1, 0);
                    MaskPanel_Image.color = newColor0;
                    MaskPanel_Blur.SetActive(false);
                    break;
                //毛玻璃效果，不能穿透
                case UIFormLucencyTypes.Blur:
                    MaskPanel.SetActive(false);
                    MaskPanel_Blur.SetActive(needMask_Blur);
                    break;
                //半透明，不能穿透
                case UIFormLucencyTypes.Translucence:
                    MaskPanel.SetActive(needMask);
                    Color newColor2 = new Color(0, 0, 0, 0.2f);
                    MaskPanel_Image.color = newColor2;
                    MaskPanel_Blur.SetActive(false);
                    break;
                //低透明，不能穿透
                case UIFormLucencyTypes.ImPenetrable:
                    MaskPanel.SetActive(needMask);
                    Color newColor3 = new Color(0, 0, 0, 0.8f);
                    MaskPanel_Image.color = newColor3;
                    MaskPanel_Blur.SetActive(false);
                    break;
                //可以穿透
                case UIFormLucencyTypes.Penetrable:
                    MaskPanel.SetActive(false);
                    MaskPanel_Blur.SetActive(false);

                    break;
            }

            //遮罩窗体下移
            MaskPanel.transform.SetAsLastSibling();
            MaskPanel_Blur.transform.SetAsLastSibling();
            //显示窗体的下移
            goDisplayUIForms.transform.SetAsLastSibling();
        }

        /// <summary>
        /// 取消遮罩状态
        /// </summary>
        public void CancelAllMaskWindow(UIFormLucencyTypes uiFormLucencyType)
        {
            //顶层窗体上移
            switch (uiFormLucencyType)
            {
                case UIFormLucencyTypes.ImPenetrable:
                    if (MaskPanel.activeInHierarchy)
                    {
                        MaskPanel.SetActive(false);
                    }

                    UIRoot.transform.SetAsFirstSibling();
                    break;

                case UIFormLucencyTypes.Blur:
                    if (MaskPanel_Blur.activeInHierarchy)
                    {
                        MaskPanel_Blur.SetActive(false);
                    }

                    UIRoot.transform.SetAsFirstSibling();
                    break;

                case UIFormLucencyTypes.Penetrable:
                    break;
                case UIFormLucencyTypes.Lucency:
                    if (MaskPanel.activeInHierarchy)
                    {
                        MaskPanel.SetActive(false);
                    }

                    UIRoot.transform.SetAsFirstSibling();
                    break;
                case UIFormLucencyTypes.Translucence:
                    if (MaskPanel.activeInHierarchy)
                    {
                        MaskPanel.SetActive(false);
                    }

                    UIRoot.transform.SetAsFirstSibling();

                    break;
            }
        }
    }
}