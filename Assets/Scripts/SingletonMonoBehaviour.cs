using UnityEngine;

public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (T)FindFirstObjectByType(typeof(T));
                if (instance == null)
                {
                    Debug.LogError($"{typeof(T)} Ç™ÉVÅ[ÉìÇ…ë∂ç›ÇµÇ‹ÇπÇÒÅB");
                }
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (CheckInstance())
        {
            OnInitialize();
        }
    }

    protected bool CheckInstance()
    {
        if (instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(this.gameObject);
            return true;
        }
        else if (instance == this)
        {
            return true;
        }
        Destroy(this.gameObject);
        return false;
    }

    protected virtual void OnInitialize() { }
}