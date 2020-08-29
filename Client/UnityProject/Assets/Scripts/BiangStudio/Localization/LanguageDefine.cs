namespace BiangStudio.Localization
{
    internal enum LanguageShorts
    {
        zh,
        en
    }

    public static class LanguageManager_Common
    {
        public delegate string GetTextDelegate(string key);

        public static GetTextDelegate GetText;

        public delegate string GetCurrentLanguageDelegate();

        public static GetCurrentLanguageDelegate GetCurrentLanguage;
    }
}