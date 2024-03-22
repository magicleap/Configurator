using UnityEngine;

/// <summary>
/// Class for others to inherit from when singleton behavior is needed
/// Will ensure only one instance of inherited class exists at a time, all others will be destroyed 
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> : MonoBehaviour
{
    public static T Instance;

    public virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = GetComponent<T>();
        }
        else
        {
            Debug.LogError($"Multiple {typeof(T)} found, deleting the second one");
            Destroy(gameObject);
        }
    }
}
