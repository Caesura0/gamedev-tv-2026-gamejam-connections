using UnityEngine;

public class PersistantUIItem : MonoBehaviour
{
    private static PersistantUIItem instance;

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