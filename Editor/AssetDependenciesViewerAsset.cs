namespace UniModules.Tools.AssetDependenciesViewer.Editor
{
    using System;
    using System.Collections.Generic;
    using UniModules.Editor;
    using UniModules.UniGame.Core.Editor.EditorProcessors;
    using UnityEditor;
    using UnityEngine;

    [Serializable]
    [GeneratedAssetInfo("Assets/UniGame.Generated/AssetDependenciesViewer/Editor")]
    public class AssetDependenciesViewerAsset : GeneratedAsset<AssetDependenciesViewerAsset>
    {
        [SerializeField]
        public List<string> _filters = new List<string>();
        
        public IReadOnlyList<string> Filters => _filters;

        public void Save()
        {
            EditorUtility.SetDirty(this);
        }
    }
}