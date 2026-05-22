using System;
using UnityEngine;

public class SceneChangeData: MonoBehaviour
{
    public static SceneChangeData Instance { get; private set; }

    [SerializeField] public Vector2Int playerStartLocation;
    [SerializeField] public DirectionEnum playerStartFacing;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
