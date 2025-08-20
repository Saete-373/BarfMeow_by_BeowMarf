using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RenderInPlate : MonoBehaviour
{
    #region Instance Data
    [Header("UI Template")]
    [SerializeField] private GameObject plateTemplate;
    [Space]

    #endregion


    #region Internal Data
    [Header("Plate Objects")]
    [SerializeField] private List<GameObject> plates;

    [Header("From Inventory")]
    [SerializeField] private List<string> inPlateIngredientNames = new();
    private Dictionary<string, Sprite> inPlateIngredients = new();

    #endregion


    void Start()
    {
        inPlateIngredients = StageManager.Instance.ingredientList.data
            .Where(ingredient => ingredient.ingredientName != "Waste")
            .ToDictionary(ingredient => ingredient.ingredientName, ingredient => ingredient.imageInPlate);

        inPlateIngredientNames = inPlateIngredients.Keys.ToList();
    }


    public void RenderPlateUI(List<string> newIngredients)
    {
        // Debug.Log("RenderPlateUI called with ingredients: " + string.Join(", ", newIngredients));

        bool hasInvalidIngredient = newIngredients.Any(newIngredient => !inPlateIngredientNames.Any(name => name == newIngredient));

        if (!hasInvalidIngredient)
        {
            for (int i = 0; i < plates.Count; i++)
            {
                if (i < newIngredients.Count)
                {
                    plates[i].SetActive(true);
                    plates[i].transform.GetChild(0).GetComponent<Image>().sprite = inPlateIngredients[newIngredients[i]];
                    continue;
                }
                plates[i].SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("Some Ingredient not in List");
            ClearPlate();
        }

    }

    public void ClearPlate()
    {
        foreach (GameObject plate in plates)
        {
            plate.SetActive(false);
        }
    }
}
