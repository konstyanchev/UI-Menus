using ModularInput;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace UIMenus
{
	public class ScrollableMenu : StandardMenu
	{
		[Header("Scrollable Menu")]

		//public GameObject attachPointContainer;
		public GameObject itemPrefab;
		public RectTransform itemsContainer;
		public Scrollbar scrollbar;

		protected int maxSelectorMoves;
		protected int currentSelectorMoves;
		protected float scrollStep;
		protected float scrollAmount;

		protected int firstVisible;
		protected int lastVisible;

		protected override void Awake()
		{
			this.CreateItemList();
			base.Awake();

			this.context.Init("Scrollable Menu Context");
			this.context.onInput = this.OnInput;
		}

		protected override void OnEnable()
		{
			this.InitializeItemList();
			base.OnEnable();
		}

		protected override IEnumerator ChangeSelection(int nextSelection)
		{
			//1. Selector is outside of the viewing area
			if (nextSelection < this.firstVisible || nextSelection > this.lastVisible)
			{
				yield return this.StartCoroutine(this.ScrollListItems(nextSelection, true));
				this.selector.Initialize(this.attachPoints[nextSelection]);
				yield break;
			}
			//2. Selector is in view
			//Going down
			if (nextSelection > this.currentSelection)
			{
				if (this.currentSelectorMoves >= this.maxSelectorMoves)
				{
					yield return this.StartCoroutine(this.ScrollListItems(nextSelection));
					this.selector.Initialize(this.attachPoints[nextSelection]);
				}
				else
				{
					if (currentSelection < this.firstVisible)
						this.currentSelectorMoves = nextSelection - this.firstVisible;
					else
						++this.currentSelectorMoves;
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
			}
			//Going up
			else
			{
				// Have to scroll the view rect up
				if (this.currentSelectorMoves <= 0)
				{
					yield return this.StartCoroutine(this.ScrollListItems(nextSelection));
					this.selector.Initialize(this.attachPoints[nextSelection]);
				}
				else
				{
					if (this.currentSelection > this.lastVisible)
						this.currentSelectorMoves = nextSelection - this.firstVisible;
					else
						--this.currentSelectorMoves;
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
			}
		}

		protected override IEnumerator AnimateMenu(bool isEntering)
		{
			yield return new WaitForEndOfFrame();
			this.isAnimating = true;

			float animationDuration = GameSettings.Instance.uiSettings.animationDuration;
			float scrollTime = animationDuration / this.maxSelectorMoves;
			float scrollDelay = scrollTime / 2;

			this.firstVisible	= Mathf.Max(0, this.firstVisible);
			this.lastVisible	= Mathf.Min(this.menuItems.Length, this.lastVisible);

			if (isEntering == true)
			{
				this.backgroundsCanvas.alpha = 0F;
				this.StartCoroutine(this.backgroundsCanvas.FadeCanvasOverTime(1F, animationDuration));
				for (int i = firstVisible; i <= lastVisible; ++i)
					this.menuItems[i].transform.anchoredPosition3D = this.target[i];
				for (int i = firstVisible; i <= lastVisible; ++i)
				{
					this.StartCoroutine(this.menuItems[i].transform.ScrollRect(this.target[i], this.origin[i], scrollTime));
					yield return new WaitForSeconds(scrollDelay);
				}
				yield return new WaitForSeconds(scrollTime - scrollDelay);
				this.selector.Initialize(this.attachPoints[this.currentSelection]);
			}
			else
			{
				this.selector.gameObject.SetActive(false);
				this.StartCoroutine(this.backgroundsCanvas.FadeCanvasOverTime(0F, animationDuration));
				for (int i = this.lastVisible; i >= this.firstVisible; --i)
				{
					this.StartCoroutine(this.menuItems[i].transform.ScrollRect(this.origin[i], this.target[i], scrollTime));
					yield return new WaitForSeconds(scrollDelay);
				}
			}
			this.isAnimating = false;
		}

		protected virtual IEnumerator ScrollListItems(int nextSelection, bool center = false)
		{
			this.isAnimating = true;
			this.onItemChanged.Invoke();
			this.OnMoveStarted(this.currentSelection);

			Vector3 originalP = this.itemsContainer.anchoredPosition3D;
			Vector3 targetP = originalP;

			if (center == true)
			{
				if (nextSelection < this.firstVisible)
					targetP.y -= this.scrollAmount * (this.firstVisible - nextSelection);
				else
					targetP.y += this.scrollAmount * (nextSelection - this.lastVisible);

				if (nextSelection < this.firstVisible)
				{
					this.currentSelectorMoves = 0;
					this.firstVisible = nextSelection;
					this.lastVisible = this.firstVisible + this.maxSelectorMoves;
				}
				else
				{
					this.currentSelectorMoves = this.maxSelectorMoves;
					this.lastVisible = nextSelection;
					this.firstVisible = this.lastVisible - this.maxSelectorMoves;
				}

			}
			else
			{
				targetP.y = nextSelection > this.currentSelection ? targetP.y += this.scrollAmount : targetP.y - +this.scrollAmount;

				if (nextSelection > this.currentSelection)
				{
					++this.firstVisible;
					++this.lastVisible;
				}
				else
				{
					--this.firstVisible;
					--this.lastVisible;
				}

			}
			float switchDuration = GameSettings.Instance.uiSettings.transitionDuration / 2;
			yield return this.StartCoroutine(this.itemsContainer.ScrollRect(originalP, targetP, switchDuration));

			this.currentSelection = nextSelection;
			this.OnMoveFinished(this.currentSelection);
			this.isAnimating = false;
		}

		protected virtual IEnumerator CenterListItems(int nextSelection)
		{
			this.isAnimating = true;
			this.onItemChanged.Invoke();
			this.OnMoveStarted(this.currentSelection);

			Vector3 originalP = this.itemsContainer.anchoredPosition3D;
			Vector3 targetP = originalP;
			
			float switchDuration = GameSettings.Instance.uiSettings.transitionDuration / 2;
			yield return this.StartCoroutine(this.itemsContainer.ScrollRect(originalP, targetP, switchDuration));

			this.currentSelection = nextSelection;
			this.OnMoveFinished(this.currentSelection);
			this.isAnimating = false;
		}

		protected virtual void CreateItemList() { }

		protected virtual void InitializeItemList() { }
	}
}
