using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingNextStage : MonoBehaviour
{
    void Start()
    {
        Debug.Log("LoadingNextStage Awake called");
        GameplayManager.Instance.isInit = false;
        SceneManager.LoadScene("Game");
    }

}
