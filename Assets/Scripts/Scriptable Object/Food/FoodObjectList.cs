using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FoodObjectList", menuName = "ScriptableObjects/FoodObjectList")]
public class FoodObjectList : ScriptableObject
{
    public List<FoodObject> data;
}


