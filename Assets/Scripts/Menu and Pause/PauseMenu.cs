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

    private void OnEnable()
    {
        InputManager.Instance.Actions.Player.Menu.performed += OnPausePressed;
    }

    private void OnDisable()
    {
        // Prevent event leaks
        if (InputManager.Instance != null)
        {
            InputManager.Instance.Actions.Player.Menu.performed -= OnPausePressed;
        }
    }

    private void OnPausePressed(InputAction.CallbackContext context)
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