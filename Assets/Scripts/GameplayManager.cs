using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager Instance;

    #region Editor Data

    // [Header("Dependencies")]

    #endregion

    #region Import Data
    [Header("Slider Bar")]

    private int currentGameStage;
    private FoodObjectList stageData;

    #endregion

    #region Public Data

    public List<FoodObject> foodOrderList;

    public float CurrentTime;
    public int CurrentMoney;
    public bool isInit = false;
    public Player_Controller playerController;


    #endregion

    #region Internal Data

    public GameState currentGameState = GameState.Playing;
    private int stageRent;
    private float maxPlayTime;
    private bool isPaused = true;

    #endregion

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitGameData()
    {
        Debug.Log("Initializing Game Data...");
        AudioManager.instance.StopSound("Menu-BGM");

        AudioManager.instance.Play("Game-BGM", 0.5f);
        AudioManager.instance.Play("Start-Game");

        StageManager.Instance.CurrentStage++;

        currentGameStage = StageManager.Instance.CurrentStage + 1;
        StageObject stageObj = StageManager.Instance.stageList[currentGameStage - 1];
        stageData = stageObj.foodList;
        stageRent = stageObj.rent;
        maxPlayTime = stageObj.playTime;
        foodOrderList = stageData.data;
        CurrentTime = maxPlayTime;

        currentGameState = GameState.Playing;

        isPaused = false;

        GameplayUIManager.Instance.InitUI(stageRent, maxPlayTime);
    }

    // Start is called before the first frame update
    void Start()
    {
        // InitGameData();
    }

    void Update()
    {
        if (isPaused) return;

        if (currentGameState != GameState.EndGame)
        {
            RunGame();
        }


    }



    private void RunGame()
    {
        if (CurrentTime > 0)
        {
            CurrentTime -= Time.deltaTime;

            GameplayUIManager.Instance.UpdateTimeUI(CurrentTime, false);


            if (CurrentTime <= 60 && currentGameState == GameState.Playing)
            {
                // Trigger hurry-up mode, e.g., change background color, speed up music, etc.
                Debug.Log("Hurry up! Only 1 minute left!");
                currentGameState = GameState.HurryUp;
            }
        }
        else
        {
            isPaused = true;
            currentGameState = GameState.EndGame;

            CurrentTime = 0;
            GameplayUIManager.Instance.UpdateTimeUI(0, true);

            EndGameStateAction();
        }
    }

    #region Public Functions
    public void AddMoney(int amount)
    {
        int addedMoney = CurrentMoney + amount;

        GameplayUIManager.Instance.UpdateMoneyUI(addedMoney);

        CurrentMoney = addedMoney;

    }

    public void SetPause(bool paused)
    {
        isPaused = paused;
    }

    public bool IsPaused() => isPaused;

    #endregion

    private void EndGameStateAction()
    {
        int totalMoney = CurrentMoney - stageRent;

        StageManager.Instance.AddMoney(totalMoney);

        GameplayUIManager.Instance.SetEndGamePanel(
            CurrentMoney,
            stageRent,
            totalMoney
        );

    }


}
