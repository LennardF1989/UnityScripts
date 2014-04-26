using UnityEditor;
using UnityEngine;
using ShaderForge;
using System.IO;

/**
 * Title: Shader Forge Extensions
 * Version: 1.0
 * 
 * Author: Lennard Fonteijn
 * Website: http://www.lennardf1989.com
 * 
 * License: If you decide to modify this tool, please share it with me through the topic below, 
 * although this is not a requirement. As long as you give credits where credits are due with 
 * a link to the topic below.
 * 
 * Source: http://forum.unity3d.com/threads/242835-Shader-Forge-Extensions?p=1606725
 **/
public class ShaderForgeExtensions : EditorWindow
{
	private Vector2 _scrollPosition;
	private string[] _clipboards;

	private SF_SelectionManager _selection
	{
		get { return SF_Editor.instance.nodeView.selection; }
	}

	[MenuItem("Window/Shader Forge Extensions")]
	public static void Initialize()
	{
		EditorWindow.GetWindow<ShaderForgeExtensions>(false, "SF Extensions");
	}

	public ShaderForgeExtensions()
	{
		minSize = new Vector2(128, 100);

		_scrollPosition = Vector2.zero;

		Refresh();
	}

	public void Update()
	{
		Repaint();
	}

	public void OnGUI()
	{
		if (GUILayout.Button(string.Format("Save ({0} nodes)", _selection.Selection.Count)))
		{
			if(_selection.Selection.Count > 0)
			{
				Save();
				Refresh();
			}
		}

		if (GUILayout.Button("Load"))
		{
			Load();
		}
		else if (GUILayout.Button("Refresh"))
		{
			Refresh();
		}

		GUILayout.Box(string.Empty, GUILayout.ExpandWidth(true), GUILayout.Height(1));

		_scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

		for (int i = 0; i < _clipboards.Length; i++)
		{
			GUILayout.BeginHorizontal();

			if (GUILayout.Button(Path.GetFileNameWithoutExtension(_clipboards[i])))
			{
				InternalLoad(_clipboards[i]);
			}
			else if(GUILayout.Button("X", GUILayout.Width(20)))
			{
				if(EditorUtility.DisplayDialog("Delete clipboard", string.Format("Are you sure you wish to delete {0}?", Path.GetFileNameWithoutExtension(_clipboards[i])), "Yes", "No"))
				{
					File.Delete(_clipboards[i]);

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
		if(!string.IsNullOrEmpty(fileName))
		{
			EditorPrefs.SetString("shaderforge_clipboard", File.ReadAllText(fileName));
			
			_selection.PasteFromClipboard();
			
			EditorPrefs.SetString("shaderforge_clipboard", string.Empty);

			GetWindow<SF_Editor>().Focus();
		}
	}
	
	private void Save()
	{
		string fileContents = string.Join("\n", SF_Editor.instance.nodeView.selection.GetSelectionSerialized());

		string fileName = EditorUtility.SaveFilePanel(
			"Save selection to file",
			Application.dataPath,
			string.Empty,
			"sfc"
			);
		
		if(!string.IsNullOrEmpty(fileName))
		{
			File.WriteAllText(fileName, fileContents);
		}
	}

	private void Refresh()
	{
		_clipboards = Directory.GetFiles(Application.dataPath + "/", "*.sfc", SearchOption.AllDirectories);
	}
}