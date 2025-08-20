using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageUIManager : MonoBehaviour
{

    [SerializeField] private TMP_Text allMoneyText;
    [SerializeField] private List<GameObject> StageList;

    private int stageCount;


    void Start()
    {

        if (StageManager.Instance != null)
        {
            stageCount = StageManager.Instance.stageList.Count;
            allMoneyText.text = $"{StageManager.Instance.AllMoney} $";
        }
        else
        {
            Debug.LogWarning("StageManager not found.");
        }

        RenderStageList();
    }

    void Update()
    {

    }

    public void RenderStageList()
    {
        for (int i = 1; i <= stageCount; i++)
        {
            if (i <= StageManager.Instance.CurrentStage && !StageManager.Instance.IsStageComplete)
            {
                StageList[i - 1].GetComponent<Stage>().SetUnlocked(true);
            }
            else if (i == StageManager.Instance.CurrentStage && StageManager.Instance.IsStageComplete)
            {
                StageList[i - 1].GetComponent<Stage>().SetUnlocked(true);
                if (i < stageCount)
                {
                    StageList[i].GetComponent<Stage>().SetUnlocked(true);
                    i++;
                }
            }
            else if (i > StageManager.Instance.CurrentStage)
            {
                StageList[i - 1].GetComponent<Stage>().SetUnlocked(false);
            }
        }
    }

    public void BackToMenu()
    {
        // Debug.Log("Returning to select stage.");
        AudioManager.instance.Play("Click");

        SceneManager.LoadScene("Menu");
    }

    public void LoadGame(int gameStage)
    {
        // Debug.Log("Starting the game for stage: " + gameStage);

        StageManager.Instance.CurrentStage = gameStage;
        StageManager.Instance.UnlockedStage = 0;
        StageManager.Instance.IsStageComplete = false;
        SceneManager.LoadScene("Game");
    }


}
