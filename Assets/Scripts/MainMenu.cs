using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneWipeTransition.Load("Gameplay");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}