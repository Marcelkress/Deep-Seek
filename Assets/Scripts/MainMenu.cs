using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void LoadNextScene()
    {
        SceneManager.LoadScene("Level");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
