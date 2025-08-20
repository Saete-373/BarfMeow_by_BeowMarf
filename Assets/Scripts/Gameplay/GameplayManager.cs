using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameplayManager : MonoBehaviour
{
    #region Import Data
    [Header("Slider Bar")]

    private int currentGameStage;
    private FoodObjectList stageData;
    [SerializeField] private GameObject _playerPrefab;

    #endregion

    #region Public Data
    public GameplayUIManager uiManager;
    public PlayerController playerController;
    public RenderInPlate renderInPlate;
    public RenderOrder renderOrder;
    public List<FoodObject> foodOrderList;


    public float CurrentTime;
    public int CurrentMoney;
    public bool isInit = false;


    public RawImage cameraDisplay;
    public TMP_Text gestureText;
    public TMP_Text fpsText;



    #endregion

    #region Internal Data

    public GameState currentGameState = GameState.Playing;
    private int stageRent;
    private float maxPlayTime;
    private bool isPaused = true;
    private bool isTriggerHurryUp = false;

    #endregion

    void Awake()
    {
        InitGameData();
    }

    public void InitGameData()
    {
        AudioManager.instance.StopSound("Menu-BGM");

        AudioManager.instance.Play("Game-BGM", 0.5f);
        AudioManager.instance.Play("Start-Game");

        currentGameStage = StageManager.Instance.CurrentStage;

        StageObject stageObj = StageManager.Instance.stageList[currentGameStage - 1];
        stageData = stageObj.foodList;
        stageRent = stageObj.rent;
        maxPlayTime = stageObj.playTime;

        foodOrderList = stageData.data;
        CurrentTime = maxPlayTime;

        currentGameState = GameState.Playing;

        isPaused = false;

        uiManager.InitUI(stageRent, maxPlayTime);
    }

    public void SpawnPlayer()
    {
        if (_playerPrefab != null)
        {
            GameObject playerObj = Instantiate(_playerPrefab, transform.position, Quaternion.identity);
            PlayerController player = playerObj.GetComponent<PlayerController>();
            player.gameplayManager = this;
        }
        else
        {
            Debug.LogError("Player Prefab is not assigned in the Inspector!");
        }
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

            uiManager.UpdateTimeUI(CurrentTime, false);


            if (CurrentTime <= 60 && currentGameState == GameState.Playing)
            {
                Debug.Log("Hurry up! Only 1 minute left!");
                if (!isTriggerHurryUp)
                {
                    isTriggerHurryUp = true;
                    AudioManager.instance.Play("Hurry-Up");
                }
                currentGameState = GameState.HurryUp;
            }
        }
        else
        {
            isPaused = true;
            currentGameState = GameState.EndGame;

            AudioManager.instance.StopSound("Hurry-Up");

            CurrentTime = 0;
            uiManager.UpdateTimeUI(0, true);

            EndGameStateAction();
        }
    }

    #region Public Functions
    public void AddMoney(int amount)
    {
        int addedMoney = CurrentMoney + amount;

        uiManager.UpdateMoneyUI(addedMoney);

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

        bool isGameOver = StageManager.Instance.AddMoney(totalMoney);

        string endGameText = CheckEndState(totalMoney, isGameOver);

        uiManager.SetEndGamePanel(
            CurrentMoney,
            stageRent,
            totalMoney,
            endGameText
        );


    }

    private string CheckEndState(int totalMoney, bool isGameOver)
    {
        if (isGameOver)
        {
            uiManager.OpenGameOverButtons();
            return "GAME OVER";
        }
        else if (totalMoney < 0)
        {
            StageManager.Instance.UnlockedStage = 0;
            StageManager.Instance.IsStageComplete = false;
            uiManager.OpenFailedButtons();
            return "YOU FAILED";
        }
        else
        {
            StageManager.Instance.UnlockedStage = 1;
            StageManager.Instance.IsStageComplete = true;
            uiManager.OpenPassedButtons();
            return "YOU PASSED";
        }
    }


}
