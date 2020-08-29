namespace BiangStudio.GamePlay.UI
{
    /// <summary>
    /// UI窗体类型
    /// </summary>
    public enum UIFormTypes
    {
        Normal,
        Fixed,
        PopUp
    }

    /// <summary>
    /// UI窗体的如何显示
    /// </summary>
    public enum UIFormShowModes
    {
        Normal,
        Return,
        ReturnHideOther,
        HideOther
    }

    /// <summary>
    /// UI窗体透明度类型
    /// </summary>
    public enum UIFormLucencyTypes
    {
        Lucency, //完全透明，不能穿透
        Blur, //毛玻璃，不能穿透
        Translucence, //半透明，不能穿透
        ImPenetrable, //低透明度，不能穿透
        Penetrable //可以穿透
    }
}