using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A singleton instance. Add 'protected T () {}' to the the child class to prevent a non-singleton constructor.
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _instance;

	private static object _lock = new object();
	public static T Instance
	{
		get
		{
			lock(_lock)
			{
				if(_instance == null)
				{
					_instance = (T)FindObjectOfType(typeof(T));

					if(FindObjectsOfType(typeof(T)).Length > 1)
					{
						Debug.LogError("[Singleton] There are multiple instances of '" + typeof(T) + "'. Ensure that there is only one instance of this type.");
					}

					if(_instance == null)
					{
						GameObject singleton = new GameObject(typeof(T).ToString() + " (singleton)");
						_instance = singleton.AddComponent<T>();
						Debug.Log("[Singleton] An instance of " + typeof(T) + "is needed in the scene, so '" + singleton + "' was created.");
					}
				}

				return _instance;
			}
		}
	}
}
