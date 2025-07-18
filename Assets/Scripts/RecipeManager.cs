using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeManager : MonoBehaviour
{
    [Header("Recipe UI")]
    [SerializeField] private List<GameObject> _menuCardList; // 4
    [SerializeField] private GameObject nextRecipe;
    [SerializeField] private GameObject prevRecipe;


    private int currentGameStage;


    void Awake()
    {
        if (StageManager.Instance != null)
        {
            // Open From Game Scene
            currentGameStage = StageManager.Instance.CurrentStage;
            nextRecipe.SetActive(false);
            prevRecipe.SetActive(false);
        }
        else
        {
            currentGameStage = 1;
            Debug.LogWarning("StageManager not found.");
        }
    }

    public void OpenRecipePanel()
    {
        gameObject.SetActive(true);
        ApplyRenderCurrentPage();
    }

    public void OpenRecipePanelWithClick()
    {
        gameObject.SetActive(true);
        // nextRecipe.SetActive(true);
        // prevRecipe.SetActive(true);
        ApplyRenderCurrentPage(1);
    }

    public void CloseRecipePanel()
    {
        Debug.Log("Closing Recipe Panel");
        gameObject.SetActive(false);
        nextRecipe.SetActive(false);
        prevRecipe.SetActive(false);
    }

    public void ApplyRenderCurrentPage(int gameStage = 0)
    {
        // //  Render without gameStage parameter
        // if (gameStage < 1)
        // {
        //     Debug.Log("Rendering Recipe Page for Stage: " + currentGameStage);
        //     if (StageManager.Instance != null && currentGameStage <= StageManager.Instance.stageList.Count)
        //     {
        //         RenderCurrentPage(currentGameStage);

        //     }
        //     else
        //     {
        //         Debug.LogWarning("Invalid stage data.");
        //     }
        // }
        // else
        // {
        //     RenderCurrentPage(gameStage);
        // }

        Debug.Log("Rendering Recipe Page for Stage: " + currentGameStage);
        if (StageManager.Instance != null && currentGameStage <= StageManager.Instance.stageList.Count)
        {
            RenderCurrentPage(currentGameStage);

        }
        else
        {
            Debug.LogWarning("Invalid stage data.");
        }


    }

    private void RenderCurrentPage(int gameStage)
    {
        ClearRecipePanel();

        int stageIndex = gameStage;

        FoodObjectList stageData = StageManager.Instance.stageList[stageIndex].foodList;
        int foodCount = stageData.data.Count;
        for (int i = 0; i < foodCount; i++)
        {
            _menuCardList[i].SetActive(true);

            FoodObject food = stageData.data[i];

            _menuCardList[i].GetComponent<MenuCardUI>().SetMenuCardObject(food);

        }
    }

    private void ClearRecipePanel()
    {
        // Logic to clear the recipe panel
        foreach (var card in _menuCardList)
        {
            card.SetActive(false);
        }
    }
}
