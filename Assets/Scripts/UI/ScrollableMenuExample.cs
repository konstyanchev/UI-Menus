using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UIMenus
{
	public class ScrollableMenuExample : ScrollableMenu
	{
		public int numberOfItems;

		private int lastStep;

		protected override void CreateItemList()
		{
			if (this.numberOfItems < 1)
			{
				Debug.LogError("You need to create at least 1 item in order for this script to function");
				this.gameObject.SetActive(false);
				return;
			}

			this.menuItems = new MenuItem[this.numberOfItems];
			for (int index = 0; index < this.numberOfItems; ++index)
			{
				GameObject newItem = Instantiate(this.itemPrefab) as GameObject;
				newItem.name = (index).ToString();
				newItem.transform.SetParent(this.itemsContainer, false);
				newItem.GetComponentInChildren<ScrollableMenuItem>().Init("Item ", index, this);
				MenuItem newMenuItem = new MenuItem();
				newMenuItem.transform = newItem.transform.GetComponent<RectTransform>();
				this.menuItems[index] = newMenuItem;
			}

			this.labels = new Text[this.menuItems.Length];
			for (int i = 0; i < this.menuItems.Length; ++i)
				this.labels[i] = this.menuItems[i].transform.GetComponentInChildren<Text>();

			this.scrollAmount = this.itemPrefab.GetComponent<LayoutElement>().minHeight;
			this.scrollAmount += this.itemsContainer.GetComponent<VerticalLayoutGroup>().spacing;
			this.maxSelectorMoves = Mathf.RoundToInt(this.mainContainer.rect.height / this.scrollAmount) - 1;
			this.maxSelectorMoves = Mathf.Min(this.numberOfItems - 1, this.maxSelectorMoves); //in case the items don't fill the view

			if (this.scrollbar != null)
				this.SetupScrollbar();
		}

		protected override IEnumerator FinalizeSetup()
		{
			yield return base.FinalizeSetup();
			if (this.scrollbar !=null && this.scrollbar.isActiveAndEnabled == true)
				this.scrollbar.onValueChanged.AddListener(this.ClampScrollbar);
		}

		private void SetupScrollbar()
		{
			this.scrollAmount = this.itemPrefab.GetComponent<LayoutElement>().minHeight;
			this.scrollAmount += this.itemsContainer.GetComponent<VerticalLayoutGroup>().spacing;

			float itemsTotalHeight = this.scrollAmount * this.numberOfItems - this.itemsContainer.GetComponent<VerticalLayoutGroup>().spacing;
			if (itemsTotalHeight > this.itemsContainer.rect.height)
				this.itemsContainer.sizeDelta = new Vector2(this.itemsContainer.rect.width, itemsTotalHeight);
			else
			{
				this.scrollbar.interactable = false;
				this.scrollbar.gameObject.SetActive(false);
			}
			this.maxSelectorMoves = Mathf.RoundToInt(this.mainContainer.rect.height / this.scrollAmount) - 1;
			this.scrollStep = 1F / (this.numberOfItems - 1 - this.maxSelectorMoves);
		} 

		private void ClampScrollbar(float value)
		{
			if (this.isAnimating == true)
				return;
			int step = Mathf.RoundToInt(value / this.scrollStep);
			if (this.lastStep != step)
			{
				this.lastVisible = this.numberOfItems - step - 1;
				this.firstVisible = this.lastVisible - this.maxSelectorMoves;
			}
			this.lastStep = step;
			this.scrollbar.value = Mathf.Min(step * scrollStep, (step - 1 * scrollStep));
			//The selector dissapears if the current selection moves out of view
			this.selector.Initialize(this.attachPoints[this.currentSelection]);
		}
			
		protected override void InitializeItemList()
		{
			this.currentSelection = 0;
			this.currentSelectorMoves = 0;
			this.firstVisible = 0;
			this.lastVisible = this.maxSelectorMoves;
			this.itemsContainer.anchoredPosition3D = new Vector3(0F, 0F, 0F);
			this.lastStep = Mathf.RoundToInt(1F / this.scrollStep);
		}
	}
}
