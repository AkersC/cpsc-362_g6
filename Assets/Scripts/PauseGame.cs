using UnityEngine;
using UnityEngine.InputSystem;

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

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }

}