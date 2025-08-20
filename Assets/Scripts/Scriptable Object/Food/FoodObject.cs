using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FoodObject", menuName = "ScriptableObjects/FoodObject")]
public class FoodObject : ScriptableObject
{
    public string foodName;
    public Sprite foodSprite;
    public GameObject prefab;
    public float spawnTime;
    public float orderMaxTime;
    public string level;
    public int price;
    public List<IngredientObject> ingredients;
}
