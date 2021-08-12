namespace UniModules.Tools.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    public static class AssetDependenciesViewer
    {
        [NonSerialized]
        private static readonly Dictionary<string, AssetInfo> PathToAssetInfo = new Dictionary<string, AssetInfo>();

        static AssetDependenciesViewer()
        {
            var assetInfos = AssetDependenciesViewerData.AssetInfos;
            foreach (var assetInfo in assetInfos)
            {
                PathToAssetInfo.Add(assetInfo.AssetPath, assetInfo);
            }
        }

        public static AssetInfo GetAsset(string path)
        {
            if (PathToAssetInfo.TryGetValue(path, out var assetInfo))
            {
                return assetInfo;
            }

            return null;
        }

        public static void AddAssetToDatabase(string path)
        {
            if (!PathToAssetInfo.TryGetValue(path, out var assetInfo))
            {
                PathToAssetInfo.Add(path, assetInfo = new AssetInfo(path));
            }
            
            var dependencies = assetInfo.GetAssetDatabaseDependencies();

            foreach (var dependency in dependencies)
            {
                if(dependency == assetInfo.AssetPath)
                    continue;

                if (PathToAssetInfo.TryGetValue(dependency, out var dependencyInfo))
                {
                    assetInfo.AddDependency(dependency);
                    dependencyInfo.AddReference(assetInfo.AssetPath);
                    
                    dependencyInfo.ClearIncludedStatus();
                }
            }
        }

        public static void RemoveAssetFromDatabase(string path)
        {
            if (PathToAssetInfo.TryGetValue(path, out var assetInfo))
            {
                foreach (var reference in assetInfo.GetReferences())
                {
                    if (PathToAssetInfo.TryGetValue(reference, out var referenceInfo))
                    {
                        if (referenceInfo.RemoveDependency(path))
                        {
                            referenceInfo.ClearIncludedStatus();
                        }
                        else
                        {
                            Debug.LogWarning($"Asset '{reference}' that depends on '{path}' doesn't have it as a dependency.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Asset '{reference}' that depends on '{path}' is not present in the database.");
                    }
                }

                foreach (var dependency in assetInfo.GetDependencies())
                {
                    if (PathToAssetInfo.TryGetValue(dependency, out var dependencyInfo))
                    {
                        if (dependencyInfo.RemoveReference(path))
                        {
                            dependencyInfo.ClearIncludedStatus();
                        }
                        else
                        {
                            Debug.LogWarning($"Asset '{dependency}' that is referenced by '{path}' doesn't have it as a reference.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Asset '{dependency}' that is referenced by '{path}' is not present in the database.");
                    }
                }
                
                PathToAssetInfo.Remove(path);
            }
            else
            {
                Debug.LogWarning($"Asset '{path}' is not present in the database.");
            }
        }

        public static void ClearDatabase()
        {
            PathToAssetInfo.Clear();
        }

        public static void RebuildDatabase()
        {
            PathToAssetInfo.Clear();

            var allAssetPaths = AssetDatabase.GetAllAssetPaths().Where(x=> x.StartsWith("Assets/") && !Directory.Exists(x)).ToArray();
            
            EditorUtility.DisplayProgressBar("Building Dependency Database", "Gathering All Assets...", 0f);

            foreach (var path in allAssetPaths)
            {
                var assetInfo = new AssetInfo(path);
                PathToAssetInfo.Add(path, assetInfo);
            }

            for (var i = 0; i < allAssetPaths.Length; i++)
            {
                if (i % 10 == 0)
                {
                    var cancel = EditorUtility.DisplayCancelableProgressBar("Building Dependency Database", allAssetPaths[i], (float) i / allAssetPaths.Length);
                    if (cancel)
                    {
                        PathToAssetInfo.Clear();
                        break;
                    }
                }
                
                AddAssetToDatabase(allAssetPaths[i]);
            }
            
            EditorUtility.ClearProgressBar();
            
            SaveDatabase();
        }

        public static void SaveDatabase()
        {
            AssetDependenciesViewerData.IsUpToDate = true;
            
            var assetInfos = new AssetInfo[PathToAssetInfo.Count];

            var i = 0;
            foreach (var info in PathToAssetInfo.Values)
            {
                assetInfos[i] = info;
                i++;
            }

            AssetDependenciesViewerData.AssetInfos = assetInfos;
            AssetDependenciesViewerData.Save();
        }
    }
}