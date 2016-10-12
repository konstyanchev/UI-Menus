using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ScriptableObjectWizard
{
	private const string title = "Create Scriptable Object";

	private static Type[]	types;
	private static string[] names;

	[MenuItem("Assets/Create/Scriptable Object", priority = 90)]
	public static void CreateScriptableObject()
	{
		ObjectCreatorWindow.Start("Scriptable Objects", typeof(ScriptableObject), ScriptableObjectWizard.Create, true);
	}

	private static void Create(Type type)
	{
		var asset = ScriptableObject.CreateInstance(type);

		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		if (path == "")
		{
			path = "Assets";
		}
		else if (Path.GetExtension(path) != "")
		{
			path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
		}

		string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + type.Name + ".asset");

		AssetDatabase.CreateAsset(asset, assetPathAndName);

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh(); 
		EditorUtility.FocusProjectWindow();
		Selection.activeObject = asset;
	}
}
