namespace UniModules.Tools.Editor
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;

    public class AssetDependenciesProcessor : AssetModificationProcessor
    {
        private static readonly Queue<Action> Actions = new Queue<Action>();
        
        [InitializeOnLoadMethod]
        public static void Init()
        {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            try
            {
                while (Actions.Count > 0)
                    Actions.Dequeue()?.Invoke();
            }
            finally
            {
                Actions.Clear();
            }
        }

        private static string[] OnWillSaveAssets(string[] paths)
        {
            if (AssetDependenciesViewerData.IsUpToDate && AssetDependenciesViewerData.EnableProcessor)
            {
                Actions.Enqueue(() =>
                {
                    foreach (var path in paths)
                    {
                        AssetDependenciesViewer.RemoveAssetFromDatabase(path);
                        AssetDependenciesViewer.AddAssetToDatabase(path);
                    }
                    
                    AssetDependenciesViewer.SaveDatabase();
                });
            }

            return paths;
        }

        private static void OnWillCreateAsset(string assetName)
        {
            if (AssetDependenciesViewerData.IsUpToDate && AssetDependenciesViewerData.EnableProcessor)
            {
                Actions.Enqueue(() =>
                {
                    AssetDependenciesViewer.AddAssetToDatabase(assetName);
                    AssetDependenciesViewer.SaveDatabase();
                });
            }
        }

        private static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
        {
            if (AssetDependenciesViewerData.IsUpToDate && AssetDependenciesViewerData.EnableProcessor)
            {
                AssetDependenciesViewer.RemoveAssetFromDatabase(assetPath);
                AssetDependenciesViewer.SaveDatabase();
            }

            return AssetDeleteResult.DidNotDelete;
        }

        private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
        {
            if (AssetDependenciesViewerData.IsUpToDate && AssetDependenciesViewerData.EnableProcessor)
            {
                Actions.Enqueue(() =>
                {
                    AssetDependenciesViewer.RemoveAssetFromDatabase(sourcePath);
                    AssetDependenciesViewer.AddAssetToDatabase(destinationPath);
                    
                    AssetDependenciesViewer.SaveDatabase();
                });
            }

            return AssetMoveResult.DidNotMove;
        }
    }
}