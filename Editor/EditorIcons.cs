namespace UniModules.Tools.Editor
{
    using UnityEngine;

    public static class EditorIcons
    {
        private static Texture2D linkBlack;
        private static Texture2D linkWhite;
        private static Texture2D linkBlue;

        public static Texture2D LinkBlack => linkBlack ? linkBlack : linkBlack = Resources.Load<Texture2D>("link_black");
        public static Texture2D LinkWhite => linkWhite ? linkWhite : linkWhite = Resources.Load<Texture2D>("link_white");
        public static Texture2D LinkBlue  => linkBlue ? linkBlue : linkBlue = Resources.Load<Texture2D>("link_blue");
    }
}