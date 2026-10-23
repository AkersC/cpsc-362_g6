
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndpoint : MonoBehaviour
{
    [SerializeField] private string nextLevelName = "Level2";
    [SerializeField] private GameObject levelCompleteCanvas;

    private bool levelCompleted = false;

    private void Start()
    {
        if (levelCompleteCanvas != null)
            levelCompleteCanvas.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (levelCompleted || !collision.CompareTag("Player"))
            return;

        levelCompleted = true;
        Debug.Log("Level Complete!");

        if (levelCompleteCanvas != null)
        {
            levelCompleteCanvas.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            Debug.LogWarning("Level Complete Canvas not assigned!");
        }
    }

    
public void LoadNextLevel()
{
    if (!Application.CanStreamedLevelBeLoaded(nextLevelName))
    {
        Debug.LogWarning("Next level not found: " + nextLevelName);
        return;
    }

    if (SceneWipeTransition.Instance != null)
    {
        SceneWipeTransition.Instance.TransitionTo(nextLevelName);
    }
    else
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextLevelName);
    }
}


    public void LoadMainMenu()
    {
        if (!Application.CanStreamedLevelBeLoaded("MainMenu"))
        {
            Debug.LogWarning("MainMenu scene not found!");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
