using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseGame : MonoBehaviour
{
    public static bool PauseTheGame = false;
    public GameObject PauseMenuUI;

    public void Resume()
    {
        PauseMenuUI.SetActive(false);
        Time.timeScale = 1;
        PauseTheGame = false;
    }

    void Pause()
    {
        PauseMenuUI.SetActive(true);
        Time.timeScale = 0;
        PauseTheGame = true;
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (PauseTheGame)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        PauseTheGame = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        PauseTheGame = false;
        SceneManager.LoadScene("MainMenu");
        Application.Quit();
    }

}