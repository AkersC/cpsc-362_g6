using UnityEngine;
using UnityEngine.InputSystem;

public class PauseGame : MonoBehaviour
{
    public static bool PauseTheGame = false;

    public void Resume()
    {
    Time.timeScale = 1;
    PauseTheGame = false;
    }

    void Pause()
    {
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

}