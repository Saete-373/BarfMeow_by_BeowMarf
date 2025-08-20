using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;
    public int CurrentStage = 1;
    public int UnlockedStage = 0;
    public bool IsStageComplete = false;
    public int AllMoney = 0;
    public List<StageObject> stageList;
    public MenuSetting menuSetting;
    public FoodObjectList foodList;
    public IngredientObjectList ingredientList;
    public IngredientObject wasteIngredient;
    public IngredientObjectList milkList;
    public List<string> ingredientNames = new();

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

        List<string> baseIngredientNames = ingredientList.data.ConvertAll(ingredient => ingredient.ingredientName);
        List<string> milkIngredientNames = milkList.data.ConvertAll(ingredient => ingredient.ingredientName);

        ingredientNames.AddRange(baseIngredientNames);
        ingredientNames.AddRange(milkIngredientNames);
    }

    private void Start()
    {
        if (menuSetting != null)
        {
            menuSetting.LoadVolume();
        }
        else
        {
            Debug.LogWarning("MenuSetting is not assigned in StageManager.");
        }
        AudioManager.instance.Play("Menu-BGM", 0.5f);
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
        CurrentStage = 1;
        UnlockedStage = 0;
        IsStageComplete = false;

        AllMoney = 0;

        PlayerPrefs.SetInt(CURRENT_STAGE_KEY, CurrentStage);
        PlayerPrefs.SetInt(UNLOCKED_STAGE_KEY, UnlockedStage);
        PlayerPrefs.SetInt(CURRENT_MONEY_KEY, AllMoney);

        PlayerPrefs.Save();
    }

    public bool AddMoney(int amount)
    {
        AllMoney += amount;
        return AllMoney < 0;
    }

    public void Save()
    {
        int oldCurrentStage = PlayerPrefs.GetInt(CURRENT_STAGE_KEY, 1);
        int oldUnlockedStage = PlayerPrefs.GetInt(UNLOCKED_STAGE_KEY, 0);
        bool oldIsStageComplete = oldUnlockedStage == 1;
        int oldAllMoney = PlayerPrefs.GetInt(CURRENT_MONEY_KEY, 0);

        if (CurrentStage > oldCurrentStage)
        {
            SaveProgress();
        }
        else if (CurrentStage == oldCurrentStage && UnlockedStage != oldUnlockedStage)
        {
            SaveProgress();
        }
        else if (CurrentStage == oldCurrentStage && UnlockedStage == oldUnlockedStage && AllMoney != oldAllMoney)
        {
            SaveProgress();
        }
        else
        {
            CurrentStage = oldCurrentStage;
            UnlockedStage = oldUnlockedStage;
            IsStageComplete = oldIsStageComplete;
            AllMoney = oldAllMoney;
        }
    }

    private void SaveProgress()
    {
        PlayerPrefs.SetInt(CURRENT_STAGE_KEY, CurrentStage);
        PlayerPrefs.SetInt(UNLOCKED_STAGE_KEY, UnlockedStage);
        IsStageComplete = UnlockedStage == 1;
        PlayerPrefs.SetInt(CURRENT_MONEY_KEY, AllMoney);

        PlayerPrefs.Save();
    }

    public void Load()
    {
        CurrentStage = PlayerPrefs.GetInt(CURRENT_STAGE_KEY, 1);
        UnlockedStage = PlayerPrefs.GetInt(UNLOCKED_STAGE_KEY, 0);
        IsStageComplete = UnlockedStage == 1;
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
