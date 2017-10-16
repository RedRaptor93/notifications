namespace Plugin.Notifications
{
    static class AssetsExtentions
    {
        const string PREFIX = "ms-appx:";
        const string ASSETS = PREFIX + "///Assets/";
        const string AUDIO = ASSETS + "Audio/";

        public static string GetAssetsPath(this string original)
        {
            if (original.StartsWith(PREFIX))
                return original;

            return ASSETS + original;
        }

        public static string GetAssetsAudioPath(this string original)
        {
            if (original.StartsWith(PREFIX))
                return original;

            return AUDIO + original;
        }
    }
}
