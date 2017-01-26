/**
 * Title: Shader Forge Extensions
 * Version: 1.3
 * 
 * Author: Lennard Fonteijn
 * Website: http://www.lennardf1989.com
 * 
 * License: If you decide to modify this tool, please share it with me through the topic below, 
 * although this is not a requirement. As long as you give credits where credits are due with 
 * a link to the topic below.
 * 
 * Source: http://forum.unity3d.com/threads/242835-Shader-Forge-Extensions
 **/

using System.IO;
using Holoville.HOEditorUtils;
using ShaderForge;
using UnityEditor;
using UnityEngine;

namespace LennardF1989.UnityScripts.ShaderForge
{
    public class ShaderForgeExtensions : EditorWindow
    {
        private SF_SelectionManager SelectionManager
        {
            get
            {
                try
                {
                    return SF_Editor.instance.nodeView.selection;
                }
                catch
                {
                    return null;
                }
            }
        }

        private int SelectionCount
        {
            get { return SelectionManager == null ? 0 : SelectionManager.Selection.Count; }
        }

        private Vector2 CameraPosition
        {
            get { return SF_Editor.instance.nodeView.cameraPos; }
        }

        private Vector2 _scrollPosition;

        private string _previousFilter;
        private string _currentFilter;

        private string[] _clips;

        [MenuItem("Window/Shader Forge Extensions")]
        public static void Initialize()
        {
            GetWindow<ShaderForgeExtensions>();
        }

        public void OnEnable()
        {
            HOPanelUtils.SetWindowTitle(this, SF_GUI.Icon, "SF Extensions");

            minSize = new Vector2(275, 100);

            _scrollPosition = Vector2.zero;

            _previousFilter = _currentFilter = string.Empty;

            Refresh();
        }

        public void Update()
        {
            Repaint();
        }

        public void OnGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button(string.Format("Save ({0} nodes)", SelectionCount), EditorStyles.toolbarButton))
            {
                Save();
                Refresh();
            }
            else if (GUILayout.Button("Load", EditorStyles.toolbarButton))
            {
                Load();
            }
            else if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
            {
                Refresh();
            }
            else if(GUILayout.Button("?", EditorStyles.toolbarButton, GUILayout.Width(20)))
            {
                Application.OpenURL("http://forum.unity3d.com/threads/242835-Shader-Forge-Extensions");
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            _previousFilter = _currentFilter;
            _currentFilter = GUILayout.TextField(_currentFilter, GUI.skin.FindStyle("ToolbarSeachTextField"));

            if(!_currentFilter.Equals(_previousFilter))
            {
                Refresh();
            }

            if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
            {
                _previousFilter = _currentFilter = string.Empty;

                GUI.FocusControl(null);

                Refresh();
            }

            GUILayout.EndHorizontal();

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            foreach (string clip in _clips)
            {
                GUILayout.BeginHorizontal();

                if (GUILayout.Button(Path.GetFileNameWithoutExtension(clip)))
                {
                    InternalLoad(clip);
                }
                else if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    if (EditorUtility.DisplayDialog(
                        "Delete clipboard",
                        string.Format("Are you sure you wish to delete {0}?", Path.GetFileNameWithoutExtension(clip)),
                        "Yes", 
                        "No")
                        )
                    {
                        File.Delete(clip);

                        Refresh();
                    }
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }

        private void Load()
        {
            string fileName = EditorUtility.OpenFilePanel(
                "Load file to clipboard",
                Application.dataPath,
                "sfc"
                );

            InternalLoad(fileName);
        }

        private void InternalLoad(string fileName)
        {
            if (SelectionManager == null || string.IsNullOrEmpty(fileName))
            {
                return;
            }

            EditorPrefs.SetString("shaderforge_clipboard", File.ReadAllText(fileName));

            Vector2 targetPosition = (SelectionCount == 0) ? CameraPosition : GetSelectionPosition();

            SelectionManager.PasteFromClipboard();

            MoveSelectionTo(targetPosition + new Vector2(64f, 64f));

            EditorPrefs.SetString("shaderforge_clipboard", string.Empty);

            GetWindow<SF_Editor>().Focus();
        }

        private Vector2 GetSelectionPosition()
        {
            float minX = SelectionManager.Selection[0].rect.x;
            float minY = SelectionManager.Selection[0].rect.y;

            foreach (SF_Node node in SelectionManager.Selection)
            {
                if (node.rect.x < minX)
                {
                    minX = node.rect.x;
                }

                if (node.rect.y < minY)
                {
                    minY = node.rect.y;
                }
            }

            return new Vector2(minX, minY);
        }

        private void MoveSelectionTo(Vector2 targetPosition)
        {
            Vector2 selectionPosition = GetSelectionPosition();

            foreach (SF_Node node in SelectionManager.Selection)
            {
                node.rect.x = (node.rect.x - selectionPosition.x) + targetPosition.x;
                node.rect.y = (node.rect.y - selectionPosition.y) + targetPosition.y;
            }
        }

        private void Save()
        {
            if(SelectionManager == null || SelectionCount == 0)
            {
                return;
            }

            string fileContents = string.Join("\n", SelectionManager.GetSelectionSerialized());

            string fileName = EditorUtility.SaveFilePanel(
                "Save selection to file",
                Application.dataPath,
                string.Empty,
                "sfc"
                );

            if (!string.IsNullOrEmpty(fileName))
            {
                File.WriteAllText(fileName, fileContents);
            }
        }

        private void Refresh()
        {
            string searchFilter = (string.IsNullOrEmpty(_currentFilter)) ? "*.sfc" : string.Format("*{0}*.sfc", _currentFilter);

            try
            {
                _clips = Directory.GetFiles(Application.dataPath, searchFilter, SearchOption.AllDirectories);
            }
            catch
            {
                _clips = new string[0];
            }
        }
    }
}