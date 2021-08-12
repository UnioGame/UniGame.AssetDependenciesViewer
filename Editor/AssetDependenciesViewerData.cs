namespace UniModules.Tools.Editor
{
    using System;
    using System.IO;
    using UnityEngine;

    [Serializable]
    public class AssetDependenciesViewerData
    {
        private const string JsonPath = "Library/AssetDependenciesViewerSettings.json";

        [SerializeField]
        private bool _isUpToDate;
        [SerializeField]
        private bool _enableProcessor;

        [SerializeField]
        private AssetInfo[] _assetInfos;
        
        private static AssetDependenciesViewerData instance;

        private static AssetDependenciesViewerData Instance
        {
            get
            {
                if (instance == null)
                {
                    if (File.Exists(JsonPath))
                    {
                        instance = JsonUtility.FromJson<AssetDependenciesViewerData>(File.ReadAllText(JsonPath));
                    }
                    else
                    {
                        instance = new AssetDependenciesViewerData();
                        File.WriteAllText(JsonPath, JsonUtility.ToJson(instance));
                    }
                }

                return instance;
            }
        }

        public static bool IsUpToDate
        {
            get => Instance._isUpToDate;
            set => Instance._isUpToDate = value;
        }

        public static bool EnableProcessor
        {
            get => Instance._enableProcessor;
            set => Instance._enableProcessor = value;
        }

        public static AssetInfo[] AssetInfos
        {
            get => Instance._assetInfos ?? (Instance._assetInfos = new AssetInfo[0]);
            set => Instance._assetInfos = value;
        }

        public static void Save()
        {
            File.WriteAllText(JsonPath, JsonUtility.ToJson(Instance));
        }
    }
}