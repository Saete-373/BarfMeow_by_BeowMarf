using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void NewGame()
    {
        StartCoroutine(DelayNewGame(1f));
    }

    private IEnumerator DelayNewGame(float delay)
    {
        Debug.Log("Starting a new game.");
        AudioManager.instance.Play("Click");

        if (StageManager.Instance != null)
        {
            StageManager.Instance.SetNewGameStage();
        }
        else
        {
            Debug.LogError("StageManager.Instance is null!");
        }

        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("Game");

    }

    public void LoadGame()
    {
        StartCoroutine(DelayLoadGame(1f));
    }

    private IEnumerator DelayLoadGame(float delay)
    {
        Debug.Log("Loading saved game.");
        AudioManager.instance.Play("Click");

        SaveSystem.Load();
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("Stage");
    }

    public void ExitGame()
    {
        StartCoroutine(DelayExitGame(1f));
    }

    private IEnumerator DelayExitGame(float delay)
    {
        Debug.Log("Exiting the game.");
        AudioManager.instance.Play("Click");

        SaveSystem.Save();

        yield return new WaitForSeconds(delay);

        SaveSystem.Save();

#if UNITY_WEBGL && !UNITY_EDITOR
            Application.OpenURL(Application.absoluteURL);
#else

        Application.Quit();
#endif

    }

}
