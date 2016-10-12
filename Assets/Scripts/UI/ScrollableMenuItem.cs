using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UIMenus
{
	public class ScrollableMenuItem : MonoBehaviour
	{
		private Text label;
		public Button button;

		protected virtual void Awake()
		{
			this.label = this.GetComponentInChildren<Text>();
			this.button = this.GetComponent<Button>();
		}

		public void Init(string name, int index, StandardMenu menu)
		{
			this.label.text = name + index;
			this.button.onClick.AddListener(delegate { menu.ChangeSelectionTo(index); });
		}
	}
}
