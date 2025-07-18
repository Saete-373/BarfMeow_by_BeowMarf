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
    // 0 1 2 3 4 
    //         5
    public void RenderStageList()
    {
        for (int i = 0; i < stageCount; i++)
        {
            int stageNumber = i + 1;

            // ปลดล็อกด่านที่น้อยกว่าหรือเท่ากับ UnlockedStage
            if (stageNumber <= StageManager.Instance.UnlockedStage)
            {
                StageList[i].GetComponent<Stage>().SetUnlocked(true);
            }
            else if (stageNumber == StageManager.Instance.UnlockedStage + 1 &&
                     StageManager.Instance.CurrentStage == StageManager.Instance.UnlockedStage)
            {
                StageList[i].GetComponent<Stage>().SetUnlocked(true);
            }
            else
            {
                StageList[i].GetComponent<Stage>().SetUnlocked(false);
            }
        }
    }

    public void BackToMenu()
    {
        Debug.Log("Returning to select stage.");

        SceneManager.LoadScene("Menu");
    }

    public void LoadGame(int gameStage)
    {
        Debug.Log("Starting the game for stage: " + gameStage);
        StageManager.Instance.CurrentStage = gameStage - 1;
        SceneManager.LoadScene("Game");
    }


}
