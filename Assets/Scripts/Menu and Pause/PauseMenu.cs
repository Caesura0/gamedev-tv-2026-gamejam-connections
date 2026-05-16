using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI; // Reference to the pause menu UI GameObject
    private bool isPaused = false; 
    public bool speedUp;

    [SerializeField] GameObject gameoverPanel;


    public static EventHandler OnRestart;
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape) /*&& !GameManager.Instance.GameOver*/)
        {
            if (isPaused)
            {
                Resume(); // If the game is already paused, resume it
            }
            else
            {
                Pause(); // If the game is not paused, pause it
            }
        }
    }

    public void Pause()
    {

        Time.timeScale = 0f; // Pause the game by setting time scale to 0
        isPaused = true;
        Debug.Log(Time.timeScale);
        pauseMenuUI.SetActive(true); 
        //AudioManager.Instance.PlayPauseClick();
    }

    public void Resume()
    {
        Time.timeScale = 1f; // Resume the game by setting time scale to 1
        isPaused = false;
        pauseMenuUI.SetActive(false); 
        //AudioManager.Instance.PlayResumeClick();
    }

    public void RestartScene()
    {
        CloseGameOverPanel();
        //GameManager.Instance.ResetGame();
        Time.timeScale = 1f; // Ensure time scale is set to 1
        OnRestart?.Invoke(this, EventArgs.Empty);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Restart the current scene


    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); 
        Time.timeScale = 1f; // Ensure time scale is set to 1
    }


    public void CloseGameOverPanel()
    {
        gameoverPanel.SetActive(false);
    }
}