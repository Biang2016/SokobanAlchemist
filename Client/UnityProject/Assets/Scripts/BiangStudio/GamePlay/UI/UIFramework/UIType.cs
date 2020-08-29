namespace BiangStudio.GamePlay.UI
{
    public class UIType
    {
        //是否清空“栈集合”
        public bool IsClearStack = false;

        //是否可用ESC关闭
        public bool IsESCClose = true;

        //是否可以点击其他地方关闭
        public bool IsClickElsewhereClose = true;

        //UI窗体（位置）类型
        public UIFormTypes UIForms_Type = UIFormTypes.Normal;

        //UI窗体显示类型
        public UIFormShowModes UIForms_ShowMode = UIFormShowModes.Normal;

        //UI窗体透明度类型
        public UIFormLucencyTypes UIForm_LucencyType = UIFormLucencyTypes.Lucency;

        public void InitUIType(bool isClearStack, bool isESCClose, bool isClickElsewhereClose, UIFormTypes uiForms_Type, UIFormShowModes uiForms_ShowMode, UIFormLucencyTypes uiForm_LucencyType)
        {
            IsClearStack = isClearStack;
            IsESCClose = isESCClose;
            IsClickElsewhereClose = isClickElsewhereClose;
            UIForms_Type = uiForms_Type;
            UIForms_ShowMode = uiForms_ShowMode;
            UIForm_LucencyType = uiForm_LucencyType;
        }
    }
}