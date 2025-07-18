using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IngredientObjectList", menuName = "ScriptableObjects/IngredientObjectList")]
public class IngredientObjectList : ScriptableObject
{
    public List<IngredientObject> data;
}