using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

namespace UIMenus
{
	public static class ImageExtensions
	{
	}

	public static class TransformExtensions
	{
		public static IEnumerator ScrollRect(this RectTransform rect, Vector3 from, Vector3 to, float duration, bool bUseAnchoredPosition = true)
		{
			float timePassed = 0F;
			while (timePassed < duration)
			{
				float rate = timePassed / duration;
				if (bUseAnchoredPosition == true)
					rect.anchoredPosition3D = Vector3.Lerp(from, to, rate);
				else
					rect.transform.position = Vector3.Lerp(from, to, rate);
				timePassed += Time.deltaTime;
				yield return null;
			}
			if (bUseAnchoredPosition == true)
				rect.anchoredPosition3D = to;
			else
				rect.transform.position = to;
		}

		public static IEnumerator ScrollTransform(this Transform transform, Vector3 from, Vector3 to, float duration)
		{
			float timePassed = 0F;
			while (timePassed < duration)
			{
				float rate = timePassed / duration;
				transform.position = Vector3.Lerp(from, to, rate);
				timePassed += Time.deltaTime;
				yield return null;
			}
			transform.transform.position = to;
		}
	}

	public static class UIExtensions
	{
		public static IEnumerator FadeCanvasOverTime(this CanvasGroup input, float to, float duration)
		{
			float timePassed = 0F;
			float from = input.alpha;
			while (timePassed < duration)
			{
				float rate = timePassed / duration;
				input.alpha = Mathf.Lerp(from, to, rate);
				timePassed += Time.deltaTime;
				yield return null;
			}
			input.alpha = to;
		}
	}
}
