using UnityEngine;

[CreateAssetMenu(fileName = "IngredientObject", menuName = "ScriptableObjects/IngredientObject")]
public class IngredientObject : ScriptableObject
{
    public string ingredientName;
    public Transform ingredientPart;
    public Sprite ingredientSprite;
    public Sprite cookedSprite;
    // public Transform ingredientPrefab;

}


