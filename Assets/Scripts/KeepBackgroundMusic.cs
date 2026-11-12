using UnityEngine;

public class KeepBackgroundMusic : MonoBehaviour
{
    private static KeepBackgroundMusic instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        DontDestroyOnLoad(gameObject);
    }
}
