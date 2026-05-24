using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject gameoverPanel;




    private bool isPaused = false;

    public static EventHandler OnRestart;



    private void Start()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMenuPressed += TogglePause;
        }
    }

    private void OnDestroy()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnMenuPressed -= TogglePause;
        }
    }

    private void TogglePause()
    {
        if (isPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }


    public void Pause()
    {
        Time.timeScale = 0f;
        isPaused = true;

        pauseMenuUI.SetActive(true);
        SceneChangeData.Instance.isPaused = true;

    }

    public void Resume()
    {
        Time.timeScale = 1f;
        isPaused = false;

        pauseMenuUI.SetActive(false);
        SceneChangeData.Instance.isPaused = false;


    }

    public void RestartScene()
    {
        CloseGameOverPanel();

        Time.timeScale = 1f;

        OnRestart?.Invoke(this, EventArgs.Empty);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("MainMenu");
    }

    public void CloseGameOverPanel()
    {
        gameoverPanel.SetActive(false);
    }
}