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
    private InputManager inputManager;

    private void Awake()
    {
        inputManager = InputManager.Instance;
    }

    private void OnEnable()
    {
        inputManager.OnMenuPressed += TogglePause;
    }

    private void OnDisable()
    {
        if (inputManager != null)
        {
            inputManager.OnMenuPressed -= TogglePause;
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


    }

    public void Resume()
    {
        Time.timeScale = 1f;
        isPaused = false;

        pauseMenuUI.SetActive(false);


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