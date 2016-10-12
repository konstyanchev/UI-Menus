using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;

public class ObjectCreatorWindow : EditorWindow
{
	private Action<Type> onCreate;
	private Type typeToCreate;
	private Type currentSelection;
	private Type[] typeDerivatives;
	private bool closeOnCreate;

	public static void Start(string title, Type type, Action<Type> OnCreate, bool closeOnCreate)
	{
		//We need a window object to be able to call a memeber method
		ObjectCreatorWindow.GetWindow<ObjectCreatorWindow>(true).Init(title, type, OnCreate, closeOnCreate);
	}

	private void Init(string title, Type type, Action<Type> OnCreate, bool closeOnCreate)
	{
		this.titleContent = new GUIContent(title);
		this.typeToCreate = type;
		this.onCreate = OnCreate;
		this.closeOnCreate = closeOnCreate;
		this.typeDerivatives = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(this.typeToCreate) && t.IsAbstract == false && t != GetType()).ToArray();
	}

	protected void OnGUI()
	{
		if (this.typeDerivatives == null || this.typeDerivatives.Length < 1)
		{
			GUILayout.TextArea("There are no scriptable objects in the project");
			return;
		}

		GUILayout.Label(this.titleContent, EditorStyles.boldLabel);
		{
			foreach (var derivate in this.typeDerivatives)
				this.DrawType(derivate);
		}
		GUILayout.EndHorizontal();
		if (GUILayout.Button("Create") == true)
			this.Create(this.currentSelection);
		GUILayout.BeginHorizontal();

	}

	private void DrawType(Type type)
	{
		if(GUILayout.Button(type.Name) == true)
			this.currentSelection = type;
	}

	private void Create(Type currentSelection)
	{
		this.onCreate(currentSelection);
		if (this != null && this.closeOnCreate == true)
			this.Close();
	}
}

