using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    [SerializeField] private GameObject skipButton;

    private bool isVideoFinished = false;

    void Start()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
        }

        StartCoroutine(ShowSkipButtonAfterDelay(5f));
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        isVideoFinished = true;
        Debug.Log("Video finished playing!");
        LoadGameMenu();
    }

    public void SkipVideo()
    {
        if (videoPlayer != null && !isVideoFinished)
        {
            videoPlayer.Stop();
            skipButton.SetActive(false);
            LoadGameMenu();
        }
    }

    private IEnumerator ShowSkipButtonAfterDelay(float delay)
    {
        skipButton.GetComponent<Animator>().Play("none");

        yield return new WaitForSeconds(delay);

        skipButton.GetComponent<Animator>().Play("show");
    }

    private void LoadGameMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
