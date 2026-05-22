using UnityEngine;

public class SceneController : MonoBehaviour
{   
    [SerializeField] private SceneEnum nextScene;
    [SerializeField] private Vector2Int playerStartLocation;
    [SerializeField] private DirectionEnum playerStartFacing;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Trigger: Level complete. Loading Scene {nextScene}");
        Debug.Log($"Starting location: {playerStartLocation}");
        Loader.Load(nextScene);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerBehaviour playerScript = player.GetComponent<PlayerBehaviour>();
        if (playerScript != null)
        {
            Debug.Log($"Starting location: {playerStartLocation}");
            playerScript.SetPlayerLocation(playerStartLocation);
            playerScript.SetPlayerFaceDirection(playerStartFacing);
        }
        
    }
}
