using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] GameObject optionsWindow;
    SceneEnum nextScene = SceneEnum.Level01;
    private Vector2Int playerStartLocation = new Vector2Int(8,2);
    private DirectionEnum playerStartFacing = DirectionEnum.North;

    private void Start()
    {
        AudioManager.Instance.PlayMainMenuMusic();
    }
    public void StartGame()
    {
        StartCoroutine(StartGameRoutine());

    }

    private IEnumerator StartGameRoutine()
    {
        AudioManager.Instance.PlayButtonClick();

        Debug.Log("Start Game Pressed");

        // Small delay to let button click animation/audio finish
        yield return new WaitForSeconds(0.15f);

        SceneChangeData.Instance.playerStartLocation = playerStartLocation;
        SceneChangeData.Instance.playerStartFacing = playerStartFacing;

        Loader.Load(nextScene);
    }


    public void QuitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        //AudioManager.Instance.PlayButtonClick();
        Application.Quit();
    }


    public void OpenOptionsWindow()
    {
        //AudioManager.Instance.PlayButtonClick();
        optionsWindow.SetActive(true);
    }


}
