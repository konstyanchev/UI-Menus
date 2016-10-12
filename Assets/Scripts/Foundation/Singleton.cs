using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	protected static T instance;
	public static T Instance
	{
		get
		{
			if(instance == null)
			{
				instance = (T)GameObject.FindObjectOfType(typeof(T));

				if (instance == null)
					Debug.LogError("An isntance of " + typeof(T) + " needs to be present in the scene");
			}
			return instance;
		}
	}

}
