namespace UniModules.Tools.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    [Serializable]
    public class AssetInfo : ISerializationCallbackReceiver
    {
        private HashSet<string> _references = new HashSet<string>();
        private HashSet<string> _dependencies = new HashSet<string>();

        private IncludedInBuild _includedStatus;

        [SerializeField]
        private string[] _referencesArray;
        [SerializeField]
        private string[] _dependenciesArray;

        [SerializeField]
        private string _path;

        public IncludedInBuild IncludedStatus
        {
            get
            {
                if (_includedStatus != IncludedInBuild.Unknown)
                    return _includedStatus;

                _includedStatus = IncludedInBuild.NotIncluded;
                return _includedStatus = CheckIncludedStatus();
            }
        }

        public bool IsIncludedInBuild => (int) IncludedStatus >= 10;

        public string AssetPath => _path;

        public string Name => Path.GetFileName(_path);

        public int DependenciesCount => _dependencies.Count;
        public int ReferencesCount   => _references.Count;

        public AssetInfo(string path)
        {
            _path = path;
        }

        public void OnBeforeSerialize()
        {
            _referencesArray = _references.ToArray();
            _dependenciesArray = _dependencies.ToArray();
        }

        public void OnAfterDeserialize()
        {
            _references = new HashSet<string>(_referencesArray ?? new string[0]);
            _dependencies = new HashSet<string>(_dependenciesArray ?? new string[0]);
        }

        public string[] GetAssetDatabaseDependencies()
        {
            return AssetDatabase.GetDependencies(_path);
        }

        public IEnumerable<string> GetReferences()
        {
            foreach (var reference in _references)
            {
                yield return reference;
            }
        }

        public IEnumerable<string> GetDependencies()
        {
            foreach (var dependency in _dependencies)
            {
                yield return dependency;
            }
        }

        public void ClearIncludedStatus()
        {
            _includedStatus = IncludedInBuild.Unknown;
        }

        public void AddDependency(string dependency)
        {
            if(!_dependencies.Contains(dependency))
                _dependencies.Add(dependency);
        }

        public bool RemoveDependency(string dependency)
        {
            return _dependencies.Remove(dependency);
        }

        public void AddReference(string reference)
        {
            if (!_references.Contains(reference))
                _references.Add(reference);
        }

        public bool RemoveReference(string reference)
        {
            return _references.Remove(reference);
        }

        private IncludedInBuild CheckIncludedStatus()
        {
            foreach (var reference in _references)
            {
                var referenceInfo = AssetDependenciesViewer.GetAsset(reference);
                if (referenceInfo.IsIncludedInBuild)
                {
                    return IncludedInBuild.Referenced;
                }
            }

            bool isInEditor = false;

            var directories = _path.ToLower().Split('/');
            for (var i = 0; i < directories.Length - 1; i++)
            {
                switch (directories[i])
                {
                    case "editor":
                        isInEditor = true;
                        break;
                    case "resources":
                        return IncludedInBuild.ResourceAsset;
                }
            }

            var extension = Path.GetExtension(_path);
            switch (extension)
            {
                case ".cs":
                    if (isInEditor)
                    {
                        return IncludedInBuild.NotIncludable;
                    }
                    else
                    {
                        return IncludedInBuild.RuntimeScript;
                    }
                case ".unity":
                    if (EditorBuildSettings.scenes.Select(x => x.path).Contains(_path))
                        return IncludedInBuild.SceneInBuild;
                    break;
            }

            return IncludedInBuild.NotIncluded;
        }
    }
}