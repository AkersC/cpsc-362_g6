# Code review only - DO NOT MERGE
using UnityEngine; 
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseGame : MonoBehaviour
{
    public static bool PauseTheGame = false;
    public GameObject PauseMenuUI;

    private MenuSlideIn slideIn;

    public void Resume()
    {
        CloseMenu(FinishResume);
    }

    private void FinishResume()
    {
        PauseMenuUI.SetActive(false);

        if (slideIn != null)
        {
            slideIn.Stop();
        }

        Time.timeScale = 1;
        PauseTheGame = false;
    }

    void Pause()
    {
        PauseMenuUI.SetActive(true);
        Time.timeScale = 0;
        PauseTheGame = true;

        if (slideIn == null)
        {
            slideIn = new MenuSlideIn(this, PauseMenuUI.transform);
        }

        slideIn.Play();
    }

    private void CloseMenu(System.Action onDone)
    {
        if (slideIn == null)
        {
            onDone();
            return;
        }

        slideIn.PlayOut(onDone);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (SceneWipeTransition.IsBusy || (slideIn != null && slideIn.IsClosing))
            {
                return;
            }

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
        CloseMenu(FinishRestart);
    }

    private void FinishRestart()
    {
        Time.timeScale = 1f;
        PauseTheGame = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        PauseTheGame = false;
        SceneWipeTransition.Load("MainMenu");
    }
}

# Kevin's Review
1. **Errors:** Escape input is guarded during closing animations, but button actions may still overlap unless `PlayOut()` prevents repeated requests. The animation also needs unscaled time to run while gameplay is paused.

2. **Requirements:** The changes support animated pause, resume, restart, and main-menu navigation. Testing should confirm that returning to the menu restores normal time.

3. **Consistency:** The code follows the project’s input and scene-management conventions. The shared `CloseMenu()` method keeps closing behavior consistent.

4. **Manageability:** Separating animations from completion callbacks makes the code easy to follow. A shared guard for keyboard and button actions would make it more reliable.
