
[System.Serializable]
public struct SaveData
{
    public int currentStage;
    public int unlockedStage; // unlockedStage <= currentStage -> 1
    public int currentMoney;
    public string SaveFilePath;
}