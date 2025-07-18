using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuCardUI : MonoBehaviour
{
    [SerializeField] private Image _foodImage;
    [SerializeField] private List<GameObject> _ingredientList;

    public void SetMenuCardObject(FoodObject food)
    {
        _foodImage.sprite = food.foodSprite;

        for (int i = 0; i < _ingredientList.Count; i++)
        {
            if (i < food.ingredients.Count)
            {
                _ingredientList[i].SetActive(true);
                _ingredientList[i].GetComponent<IngredientMethodUI>().SetIngredientMethodUI(food.ingredients[i]);
                // _ingredientList[i].GetComponent<Image>().sprite = food.ingredients[i].ingredientSprite;
            }
            else
            {
                _ingredientList[i].SetActive(false);
            }
        }
    }
}
