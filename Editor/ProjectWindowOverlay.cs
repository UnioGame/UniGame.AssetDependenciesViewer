namespace UniModules.Tools.Editor
{
    using UnityEditor;
    using UnityEngine;

    [InitializeOnLoad]
    public static partial class ProjectWindowOverlay
    {
        private static bool enabled;

        public static bool Enabled
        {
            get => enabled = EditorPrefs.GetBool("AssetDependenciesViewerEnabled");
            set => EditorPrefs.SetBool("AssetDependenciesViewerEnabled", enabled = value);
        }

        static ProjectWindowOverlay()
        {
            enabled = Enabled;
            
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemOnGUI;
        }

        private static void OnProjectWindowItemOnGUI(string guid, Rect selectionRect)
        {
            if (enabled)
            {
                var assetInfo = AssetDependenciesViewer.GetAsset(AssetDatabase.GUIDToAssetPath(guid));
                if (assetInfo != null)
                {
                    var content = new GUIContent(assetInfo.IsIncludedInBuild ? EditorIcons.LinkBlue : EditorIcons.LinkBlack, assetInfo.IncludedStatus.ToString());
                    GUI.Label(new Rect(selectionRect.width + selectionRect.x - 20, selectionRect.y + 1, 16, 16), content);
                }
            }
        }
    }
}