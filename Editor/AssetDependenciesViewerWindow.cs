using UniModules.Tools.AssetDependenciesViewer.Editor;

namespace UniModules.Tools.Editor
{
    using System.IO;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEngine;

    public class AssetDependenciesViewerWindow : EditorWindow, IHasCustomMenu
    {
        private Vector2 _scroll;

        private bool _dependenciesOpen = true;
        private bool _referencesOpen   = true;

        private string _searchString;

        private bool _hierarchicalViewEnable;

        private AssetDependenciesViewerAsset _asset;

        private static GUIStyle titleStyle;
        private static GUIStyle itemStyle;

        private static GUIStyle TitleStyle => titleStyle ?? (titleStyle = new GUIStyle(EditorStyles.label) {fontSize = 13});
        private static GUIStyle ItemStyle  => itemStyle ?? (itemStyle = new GUIStyle(EditorStyles.label) {margin     = new RectOffset(32, 0, 0, 0)});

        [MenuItem("UniGame/Tools/AssetDependenciesViewer")]
        private static void ShowWindow()
        {
            var window = GetWindow<AssetDependenciesViewerWindow>("Asset Dependencies Viewer");
            window.Initialize();
            window.Show();
        }

        public void Initialize()
        {
            _asset = AssetDependenciesViewerAsset.Asset;
            Selection.selectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            Repaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));

            AssetDependenciesViewerData.EnableProcessor = GUILayout.Toggle(AssetDependenciesViewerData.EnableProcessor, "Enable Processor", "ToolbarButton", GUILayout.Width(120));
            
            if (GUILayout.Button("Manage filters", "ToolbarButton", GUILayout.Width(100)))
            {
                FilteringWindow.ShowWindow();
            }

            _searchString = GUILayout.TextField(_searchString, GUI.skin.FindStyle("ToolbarSeachTextField"));
            
            if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
            {
                _searchString = string.Empty;
                
                GUI.FocusControl(null);
            }

            _hierarchicalViewEnable = GUILayout.Toggle(_hierarchicalViewEnable, EditorGUIUtility.IconContent("UnityEditor.SceneHierarchyWindow"), "ToolbarButton", GUILayout.Width(28));

            EditorGUILayout.EndHorizontal();
            
            // TODO: add caching
            var selectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if(string.IsNullOrEmpty(selectedPath))
                return;

            GUILayout.Space(2);

            GUILayout.BeginHorizontal("In BigTitle");
            GUILayout.Label(AssetDatabase.GetCachedIcon(selectedPath), GUILayout.Width(36), GUILayout.Height(36));
            GUILayout.BeginVertical();
            GUILayout.Label(Path.GetFileName(selectedPath), TitleStyle);
            GUILayout.Label(Regex.Match(Path.GetDirectoryName(selectedPath), "(\\\\.*)$").Value);

            var rect = GUILayoutUtility.GetLastRect();
            
            GUILayout.EndVertical();
            GUILayout.Space(44);
            GUILayout.EndHorizontal();
            
            if(Directory.Exists(selectedPath))
                return;
            
            var selectedAssetInfo = AssetDependenciesViewer.GetAsset(selectedPath);
            
            if (selectedAssetInfo == null)
            {
                var rebuildClicked = HelpBoxWithButton(new GUIContent("You must rebuild database to obtain information on this asset", EditorGUIUtility.IconContent("console.warnicon").image),
                    new GUIContent("Rebuild Database"));
                if (rebuildClicked)
                {
                    AssetDependenciesViewer.RebuildDatabase();
                }
                return;
            }
            
            var content = new GUIContent(selectedAssetInfo.IsIncludedInBuild ? EditorIcons.LinkBlue : EditorIcons.LinkBlack, selectedAssetInfo.IncludedStatus.ToString());
            GUI.Label(new Rect(position.width - 20, rect.y + 1, 16, 16), content);

            _scroll = GUILayout.BeginScrollView(_scroll);

            _referencesOpen = EditorGUILayout.Foldout(_referencesOpen, $"References ({selectedAssetInfo.ReferencesCount})");
            if (_referencesOpen)
            {
                if (_hierarchicalViewEnable)
                {
                    // TODO: hierarchical view
                }
                else
                {
                    // TODO: filter
                    foreach (var reference in selectedAssetInfo.GetReferences())
                    {
                        if (GUILayout.Button(Path.GetFileName(reference), ItemStyle))
                        {
                            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(reference);
                        }
                    
                        rect = GUILayoutUtility.GetLastRect();
                    
                        GUI.DrawTexture(new Rect(rect.x - 16, rect.y, rect.height, rect.height), AssetDatabase.GetCachedIcon(reference));
                    
                        var referenceInfo = AssetDependenciesViewer.GetAsset(reference);
                        content = new GUIContent(referenceInfo.IsIncludedInBuild ? EditorIcons.LinkBlue : EditorIcons.LinkBlack, referenceInfo.IncludedStatus.ToString());
                    
                        GUI.Label(new Rect(rect.width + rect.x - 20, rect.y + 1, 16, 16), content);
                    }
                }
            }
            
            GUILayout.Space(6);
            
            _dependenciesOpen = EditorGUILayout.Foldout(_dependenciesOpen, $"Dependencies ({selectedAssetInfo.DependenciesCount})");
            if (_dependenciesOpen)
            {
                if (_hierarchicalViewEnable)
                {
                    // TODO: hierarchical view
                }
                else
                {
                    // TODO: filter
                    foreach (var dependency in selectedAssetInfo.GetDependencies())
                    {
                        if (GUILayout.Button(Path.GetFileName(dependency), ItemStyle))
                        {
                            Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(dependency);
                        }

                        rect = GUILayoutUtility.GetLastRect();
                    
                        GUI.DrawTexture(new Rect(rect.x - 16, rect.y, rect.height, rect.height), AssetDatabase.GetCachedIcon(dependency));

                        var dependencyInfo = AssetDependenciesViewer.GetAsset(dependency);
                        content = new GUIContent(dependencyInfo.IsIncludedInBuild ? EditorIcons.LinkBlue : EditorIcons.LinkBlack, dependencyInfo.IncludedStatus.ToString());
                    
                        GUI.Label(new Rect(rect.width + rect.x - 20, rect.y + 1, 16, 16), content);
                    }
                }
            }
            
            GUILayout.Space(5);
            
            GUILayout.EndScrollView();

            if (!selectedAssetInfo.IsIncludedInBuild)
            {
                var deleteClicked = HelpBoxWithButton(new GUIContent("This asset is not referenced and never used. Would you like to delete it?", 
                    EditorGUIUtility.IconContent("console.warnicon").image), new GUIContent("Delete Asset"));

                if (deleteClicked)
                {
                    File.Delete(selectedPath);
                    
                    AssetDatabase.Refresh();
                    
                    AssetDependenciesViewer.RemoveAssetFromDatabase(selectedPath);
                }
            }
        }

        void IHasCustomMenu.AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Rebuild Database"), false, AssetDependenciesViewer.RebuildDatabase);
            menu.AddItem(new GUIContent("Clear Database"), false, AssetDependenciesViewer.ClearDatabase);
            menu.AddItem(new GUIContent("To Overlay"), ProjectWindowOverlay.Enabled, () =>
            {
                ProjectWindowOverlay.Enabled = !ProjectWindowOverlay.Enabled;
            });
        }

        private bool HelpBoxWithButton(GUIContent messageContent, GUIContent buttonContent)
        {
            var buttonWidth = buttonContent.text.Length * 8;
            
            const float buttonSpacing = 5.0f;
            const float buttonHeight  = 18.0f;

            var contentRect = GUILayoutUtility.GetRect(messageContent, EditorStyles.helpBox);
            
            GUILayoutUtility.GetRect(1, buttonHeight + buttonSpacing);

            contentRect.height += buttonHeight + buttonSpacing;
            
            GUI.Label(contentRect, messageContent, EditorStyles.helpBox);
            
            var buttonRect = new Rect(contentRect.xMax - buttonWidth - 4.0f, contentRect.yMax - buttonHeight - 4.0f, buttonWidth, buttonHeight);
            
            return GUI.Button(buttonRect, buttonContent);
        }
    }
}