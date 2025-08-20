using UnityEngine;

[CreateAssetMenu(fileName = "IngredientObject", menuName = "ScriptableObjects/IngredientObject")]
public class IngredientObject : ScriptableObject
{
    public string ingredientName;
    public Transform prefab;
    public Sprite imageUI;
    public Sprite imageInPlate;

}


