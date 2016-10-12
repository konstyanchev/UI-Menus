using UnityEngine;
using System.Collections;
using System;

namespace UIMenus
{
	public class GameSettings : ScriptableObject
	{
		private static GameSettings instance;
		public static GameSettings Instance
		{
			get
			{
				if (GameSettings.instance == null)
				{
					GameSettings gs = Resources.Load<GameSettings>("Settings/GameSettings");
					if (gs != null)
					{
						GameSettings.instance = gs;
						return gs;
					}
				}
				return GameSettings.instance;
			}
		}

		[Serializable]
		public class UISettings
		{
			[Tooltip("Scroll elasticity when going past the limits")]
			public float blockedMoveAmount = 0F;
			[Tooltip("Duration of the animation for elasticity")]
			public float blockedMoveDuration = 0F;
			public float transitionDuration;
			public float animationDuration;
		}

		public UISettings uiSettings = new UISettings();
	}
}

