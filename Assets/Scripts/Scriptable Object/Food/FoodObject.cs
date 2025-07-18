using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FoodObject", menuName = "ScriptableObjects/FoodObject")]
public class FoodObject : ScriptableObject
{
    public string foodName;
    public string foodLevel;
    public int foodPrice;
    public Sprite foodSprite;
    public float orderMaxTime;
    public GameObject foodPrefab;
    public List<IngredientObject> ingredients;
}
