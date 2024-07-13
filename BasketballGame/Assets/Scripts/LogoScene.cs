using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class LogoScene : MonoBehaviour
{
    [SerializeField]
    private VideoPlayer videoPlayer;

    void Start()
    {
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    private void OnVideoEnd(VideoPlayer video)
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
}
