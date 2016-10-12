using UnityEngine;
using System.Collections;

namespace UIMenus
{
	public class MenuSelector : MonoBehaviour
	{
		private float transitionDuration;

		public void Initialize(RectTransform attachPoint)
		{
			this.gameObject.SetActive(true);
			this.transform.position = attachPoint.position;
			this.transitionDuration = GameSettings.Instance.uiSettings.transitionDuration;
		}


		public IEnumerator MoveTo(RectTransform destination)
		{
			yield return this.StartCoroutine(this.transform.ScrollTransform(this.transform.position, destination.position, this.transitionDuration));
		}
	}
}