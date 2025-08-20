using System.Collections.Generic;
using UnityEngine;

public class IngredientState : MonoBehaviour
{
    // public List<IngredientObject> cookedList;
    public List<string> cookedNames = new();

    private void Start()
    {
        // cookedNames = cookedList.ConvertAll(ingredient => ingredient.ingredientName);
    }
}
