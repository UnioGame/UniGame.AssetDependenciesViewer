using UniModules.Tools.AssetDependenciesViewer.Editor;

namespace UniModules.Tools.Editor
{
    using System;
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using UnityEngine;

    [Serializable]
    public class FilteringWindow : OdinEditorWindow
    {
        [SerializeField]
        [InlineEditor(InlineEditorObjectFieldModes.Boxed, Expanded = true, DrawHeader = false)]
        [HideLabel]
        private AssetDependenciesViewerAsset _asset;
        
        public static void ShowWindow()
        {
            GetWindow<FilteringWindow>("ADV Filters");
        }
        
        protected override void Initialize()
        {
            base.Initialize();
            _asset = AssetDependenciesViewerAsset.Asset;
        }
    }
}