using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void GoToGameScene()
    {
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    public void GoToMenuScene()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
}
