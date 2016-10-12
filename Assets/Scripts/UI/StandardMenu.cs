using ModularInput;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace UIMenus
{
	public class StandardMenu : MonoBehaviour
	{
		public enum ArrivingDirection
		{
			Up,
			Down,
			Left,
			Right,
		}

		[System.Serializable]
		public class MenuItem
		{
			public RectTransform transform;
			public UnityEvent Enter;
			public UnityEvent Leave;
		}

		public MenuItem[] menuItems;
		public MenuSelector selector;
		public RectTransform mainContainer;
		public RectTransform titleContainer;
		public CanvasGroup backgroundsCanvas;
		public CanvasGroup mainCanvas;

		public ArrivingDirection arrivingDirection;
		[Tooltip("If another menu is opened from the current, the current one can be animated outside of the screen.")]
		public bool animateToBackground;

		public Color highlightedColor;
		public Color normalColor;

		public UnityEvent onItemChanged;
		public UnityEvent onItemSelected;
		public UnityEvent onMenuClosed;

		protected int currentSelection;
		protected bool isAnimating;
		protected bool isFocused;

		protected InputManager manager;
		protected Controller controller;
		protected InputContext context;

		protected Text[] labels;
		protected RectTransform[] attachPoints;
		
		protected Vector3[] origin;
		protected Vector3[] target;

		protected virtual void Awake()
		{
			this.manager = InputManager.Instance;
			this.manager.OnControllerChanged.AddListener(this.HandleControllerChanged);

			this.controller = this.manager.Controller;
			this.context = ScriptableObject.CreateInstance<StandardInputContext>();
			this.context.Init("Base Menu Context");
			this.context.onInput = this.OnInput;

			this.labels = new Text[this.menuItems.Length];
			this.attachPoints = new RectTransform[this.menuItems.Length];
			for (int i = 0; i < this.menuItems.Length; ++i)
				this.labels[i] = this.menuItems[i].transform.GetComponentInChildren<Text>();

			this.StartCoroutine(this.GatherAnimationData());
			this.StartCoroutine(this.FinalizeSetup());
		}

		protected virtual void OnEnable()
		{
			this.manager.AddInputContext(this.context);
			this.StartCoroutine(this.InitializeMenu());
		}

		protected virtual void OnDisable()
		{
			this.manager.OnControllerChanged.RemoveListener(this.HandleControllerChanged);
		}

		protected virtual bool OnInput()
		{
			if (this.isAnimating == false)
			{
				var d = this.controller.GetMenuDirection();
				if (d != CrossDirection.None)
				{
					return this.HandleDirections(d);
				}
				if (this.controller.Validate() == true)
				{
					return this.HandleValidate();
				}
				if (this.controller.Cancel() == true)
				{
					return this.HandleCancel();
				}
				if (this.controller.Pause() == true)
				{
					return this.HandlePause();
				}
				return false;
			}
			// Consume the input if the current menu is animating
			return true;
		}

		protected virtual bool HandleDirections(CrossDirection dir)
		{
			if (dir == CrossDirection.Up)
			{
				if (this.currentSelection > 0)
					this.StartCoroutine(this.ChangeSelection(this.currentSelection - 1));
				else
					this.StartCoroutine(this.ChangeBlocked(this.currentSelection - 1));
				return true;
			}
			else if (dir == CrossDirection.Down)
			{
				if (this.currentSelection < this.menuItems.Length - 1)
					this.StartCoroutine(this.ChangeSelection(this.currentSelection + 1));
				else
					this.StartCoroutine(this.ChangeBlocked(this.currentSelection + 1));
				return true;
			}
			return false;
		}

		protected virtual bool HandleValidate()
		{
			this.onItemSelected.Invoke();
			if (this.menuItems[this.currentSelection].Enter != null && 
				this.menuItems[this.currentSelection].Enter.GetPersistentEventCount() > 0)
			{
				this.OnFocusLost();
				this.menuItems[this.currentSelection].Enter.Invoke();
			}
			return true;
		}

		protected virtual bool HandleCancel()
		{
			this.CloseMenu();
			return true;
		}

		protected virtual bool HandlePause()
		{
			this.StartCoroutine(this.CloseMenu_Internal(false));
			return false;
		}
		
		// This function will be called at the very end of the first frame		
		protected virtual IEnumerator FinalizeSetup() { yield return new WaitForEndOfFrame(); }

		protected virtual IEnumerator GatherAnimationData()
		{
			// Need to wait for the end of frame in order for the menu items to be properly positioned
			yield return new WaitForEndOfFrame();
			float positionOffset;
			if (this.arrivingDirection == ArrivingDirection.Down || this.arrivingDirection == ArrivingDirection.Up)
				positionOffset = Math.Abs(Screen.currentResolution.height - this.transform.position.y);
			else
				positionOffset = Math.Abs(Screen.currentResolution.width - this.transform.position.x);

			for (int i = 0; i < this.menuItems.Length; ++i)
				this.attachPoints[i] = this.menuItems[i].transform.FindChild("AttachPoint").GetComponent<RectTransform>();

			// One extra for the title
			this.origin = new Vector3[this.menuItems.Length + 1];
			this.target = new Vector3[this.menuItems.Length + 1];

			for (int i = 0; i < this.menuItems.Length; i++)
				this.origin[i] = this.menuItems[i].transform.anchoredPosition3D;

			this.origin[this.menuItems.Length] = this.titleContainer.anchoredPosition3D;
			this.target[this.menuItems.Length] = origin[this.menuItems.Length];
			this.target[this.menuItems.Length].y += 300;

			switch (this.arrivingDirection)
			{
				case ArrivingDirection.Up:
					for (int i = 0; i < this.menuItems.Length; i++)
					{
						this.target[i] = this.origin[i];
						this.target[i].y += positionOffset;
					}
					break;
				case ArrivingDirection.Down:
					for (int i = 0; i < this.menuItems.Length; i++)
					{
						this.target[i] = this.origin[i];
						this.target[i].y -= positionOffset;
					}
					break;
				case ArrivingDirection.Left:
					for (int i = 0; i < this.menuItems.Length; i++)
					{
						this.target[i] = this.origin[i];
						this.target[i].x -= positionOffset;
					}
					break;
				case ArrivingDirection.Right:
					for (int i = 0; i < this.menuItems.Length; i++)
					{
						this.target[i] = this.origin[i];
						this.target[i].x += positionOffset;
					}
					break;
			}
		}

		protected virtual IEnumerator InitializeMenu()
		{
			// Block input while the animations run
			this.isAnimating = true;
			if (this.labels.Length > 1)
				this.labels[this.currentSelection].color = this.highlightedColor;
			this.StartCoroutine(this.AnimateTitle(true));
			yield return this.StartCoroutine(this.AnimateMenu(true));
			this.isFocused = true;
			this.isAnimating = false;
		}

		protected virtual IEnumerator AnimateMenu(bool isEntering)
		{
			yield return new WaitForEndOfFrame();
			this.isAnimating = true;

			float animationDuration = GameSettings.Instance.uiSettings.animationDuration;
			float scrollTime = animationDuration / this.menuItems.Length;
			float scrollDelay = scrollTime / 2;

			if (isEntering == true)
			{
				this.backgroundsCanvas.alpha = 0F;
				this.StartCoroutine(this.backgroundsCanvas.FadeCanvasOverTime(1F, animationDuration));
				for (int i = 0; i < this.menuItems.Length; i++)
					this.menuItems[i].transform.anchoredPosition3D = this.target[i];
				if ( this.arrivingDirection != ArrivingDirection.Up)
				{
					for (int i = 0; i < this.menuItems.Length; i++)
					{
						this.StartCoroutine(this.menuItems[i].transform.ScrollRect(target[i], origin[i], scrollTime));
						yield return new WaitForSeconds(scrollDelay);
					}
				}
				else
				{
					for (int i = menuItems.Length - 1; i >= 0; --i)
					{
						this.StartCoroutine(this.menuItems[i].transform.ScrollRect(target[i], origin[i], scrollTime));
						yield return new WaitForSeconds(scrollDelay);
					}
				}
				yield return new WaitForSeconds(scrollTime - scrollDelay);
				this.selector.Initialize(this.attachPoints[this.currentSelection]);
			}
			else
			{
				this.selector.gameObject.SetActive(false);
				this.StartCoroutine(this.backgroundsCanvas.FadeCanvasOverTime(0F, animationDuration));
				if (this.arrivingDirection != ArrivingDirection.Up)
				{
					for (int i = menuItems.Length - 1; i >= 0; --i)
					{
						this.StartCoroutine(this.menuItems[i].transform.ScrollRect(origin[i], target[i], scrollTime));
						yield return new WaitForSeconds(scrollDelay);
					}
				}
				else
				{
					for (int i = 0; i < menuItems.Length; ++i)
					{
						this.StartCoroutine(this.menuItems[i].transform.ScrollRect(origin[i], target[i], scrollTime));
						yield return new WaitForSeconds(scrollDelay);
					}
				}
			}
			this.isAnimating = false;
		}

		protected virtual IEnumerator AnimateTitle(bool isEntering)
		{
			yield return new WaitForEndOfFrame();

			float animDuration = GameSettings.Instance.uiSettings.animationDuration;

			if (isEntering == true)
			{
				this.titleContainer.anchoredPosition3D = this.target[this.menuItems.Length];
				yield return this.StartCoroutine(this.titleContainer.ScrollRect(this.target[this.menuItems.Length], this.origin[this.menuItems.Length], animDuration));
			}
			else
				yield return this.StartCoroutine(this.titleContainer.ScrollRect(this.origin[this.menuItems.Length], this.target[this.menuItems.Length], animDuration));
		}

		protected virtual IEnumerator ChangeSelection(int nextSelection)
		{
			if (nextSelection == this.currentSelection)
				yield break;

			this.isAnimating = true;
			this.onItemChanged.Invoke();
			this.OnMoveStarted(this.currentSelection);

			yield return this.StartCoroutine(this.selector.MoveTo(this.attachPoints[nextSelection]));

			this.currentSelection = nextSelection;
			this.OnMoveFinished(this.currentSelection);
			this.isAnimating = false;
		}

		protected virtual IEnumerator ChangeSelectionToInternal(int nextSelection)
		{
			yield return this.StartCoroutine(this.ChangeSelection(nextSelection));
			this.HandleValidate();
		}

		protected virtual IEnumerator ChangeBlocked(int nextPosition)
		{
			this.isAnimating = true;
			//Shake + Vibration
			//if (this.vibration != null && this.camShake != null)
			//{
			//	this.vibration.Vibrate("InvalidSelection");
			//	this.camShake.ShakeCamera();
			//}
			if (this.mainContainer != null)
			{
				float duration = GameSettings.Instance.uiSettings.blockedMoveDuration / 2;

				Vector3 o = this.mainContainer.anchoredPosition3D;
				Vector3 t = o;
				t.y = nextPosition > this.currentSelection ?
					t.y -= GameSettings.Instance.uiSettings.blockedMoveAmount :
					t.y += GameSettings.Instance.uiSettings.blockedMoveAmount;

				//Come back
				yield return this.StartCoroutine(this.mainContainer.ScrollRect(t, o, duration));
				yield return null;
			}
			this.isAnimating = false;
		}

		protected virtual IEnumerator CloseMenu_Internal(bool bAnimate = true)
		{
			if (bAnimate == true)
			{
				//block input here, remove the context once the animations are done or we would send input to the lower layer
				this.isAnimating = true;
				yield return this.StartCoroutine(this.AnimateMenu(false));
			}
			else
				this.selector.gameObject.SetActive(false);

			this.backgroundsCanvas.alpha = 1F;
			for (int i = 0; i < this.labels.Length; ++i)
				this.labels[i].color = this.normalColor;

			this.manager.RemoveInputContext(this.context);
			this.onMenuClosed.Invoke();
			this.gameObject.SetActive(false);
		}

		protected virtual void OnMoveStarted(int selectedOption)
		{
			this.labels[selectedOption].color = this.normalColor;
		}

		protected virtual void OnMoveFinished(int selectedOption)
		{
			this.labels[selectedOption].color = this.highlightedColor;
		}

		protected virtual void HandleControllerChanged(Controller controller)
		{
			this.controller = controller;
		}

		public virtual void OnFocusLost()
		{
			if (this.animateToBackground == true)
			{
				this.StartCoroutine(this.AnimateTitle(false));
				this.StartCoroutine(this.AnimateMenu(false));
			}
			else
				this.mainCanvas.alpha = 0.1F;
			this.isFocused = false;
		}

		public virtual void OnFocusRegained()
		{
			if (this.animateToBackground == true)
			{
				this.StartCoroutine(this.AnimateTitle(true));
				this.StartCoroutine(this.AnimateMenu(true));
			}
			else
				this.mainCanvas.alpha = 1F;
			this.menuItems[this.currentSelection].Leave.Invoke();
			this.isFocused = true;
		}

		public virtual void CloseMenu(bool animate = true)
		{
			if (this.isAnimating == true || this.isFocused == false)
				return;
			if (this.gameObject.activeInHierarchy == true)
				this.StartCoroutine(this.CloseMenu_Internal(animate));
		}

		public virtual void ChangeSelectionTo(int nextSelection)
		{
			this.StartCoroutine(this.ChangeSelectionToInternal(nextSelection));
		}

	}

}