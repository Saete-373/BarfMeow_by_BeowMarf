using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageObject", menuName = "ScriptableObjects/StageObject")]
public class StageObject : ScriptableObject
{
    public string stageName;
    public int stageNumber;
    public int rent;
    public int playTime;
    public FoodObjectList foodList;
}