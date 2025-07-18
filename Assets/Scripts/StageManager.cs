using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;
    public int CurrentStage = 0;
    public int UnlockedStage = 1;
    public int AllMoney = 0;
    public List<StageObject> stageList;

    private const string CURRENT_STAGE_KEY = "CurrentStage";
    private const string UNLOCKED_STAGE_KEY = "UnlockedStage";
    private const string CURRENT_MONEY_KEY = "CurrentMoney";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);




        SaveSystem.Load();
    }

    private void Start()
    {
        AudioManager.instance.Play("Game-BGM", 0.5f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backslash))
        {
            SetNewGameStage();
        }
    }

    public void SetNewGameStage()
    {
        CurrentStage = 0;
        UnlockedStage = 1;
        AllMoney = 0;

        SaveSystem.Save();
    }

    public void AddMoney(int amount)
    {
        AllMoney += amount;
        SaveSystem.Save();
    }

    public void Save()
    {
        PlayerPrefs.SetInt(CURRENT_STAGE_KEY, CurrentStage);
        PlayerPrefs.SetInt(UNLOCKED_STAGE_KEY, UnlockedStage);
        PlayerPrefs.SetInt(CURRENT_MONEY_KEY, AllMoney);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        CurrentStage = PlayerPrefs.GetInt(CURRENT_STAGE_KEY, 0);
        UnlockedStage = PlayerPrefs.GetInt(UNLOCKED_STAGE_KEY, 1);
        AllMoney = PlayerPrefs.GetInt(CURRENT_MONEY_KEY, 0);
    }

    // ลบข้อมูลทั้งหมด
    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(CURRENT_STAGE_KEY);
        PlayerPrefs.DeleteKey(UNLOCKED_STAGE_KEY);
        PlayerPrefs.DeleteKey(CURRENT_MONEY_KEY);
    }

    private void OnApplicationQuit()
    {
        SaveSystem.Save();
    }
}
