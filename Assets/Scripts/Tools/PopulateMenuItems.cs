using UnityEngine;
using System.Collections;
using UnityEditor;
using UIMenus;

public class PopulateMenuItems
{
	[MenuItem("UIMenus/Populate Menu Items")]
	private static void Init()
	{
		var targets = Editor.FindObjectsOfType<StandardMenu>();
		if (targets.Length == 0)
			return;

		for (int i = 0; i < targets.Length; i++)
		{
			Transform t = targets[i].mainContainer.FindChild("Items");
			if (t != null && t.childCount > 0 && (targets[i].menuItems == null || targets[i].menuItems.Length != t.childCount))
			{
				int length = t.childCount;
				targets[i].menuItems = new StandardMenu.MenuItem[length];
				for (int j = 0; j < length; j++)
				{
					targets[i].menuItems[j] = new StandardMenu.MenuItem();
					targets[i].menuItems[j].transform = t.GetChild(j).GetComponent<RectTransform>(); ;
				}
			}
		}
	}
}
