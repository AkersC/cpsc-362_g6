
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndpoint : MonoBehaviour
{
    [SerializeField] private string nextLevelName = "Level2";

    private bool levelCompleted = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (levelCompleted || !collision.CompareTag("Player"))
            return;

        levelCompleted = true;
        Debug.Log("Level Complete!");

        if (Application.CanStreamedLevelBeLoaded(nextLevelName))
        {
            SceneManager.LoadScene(nextLevelName);
        }
        else
        {
            Debug.LogWarning("Next level not found: " + nextLevelName);
        }
    }
}
