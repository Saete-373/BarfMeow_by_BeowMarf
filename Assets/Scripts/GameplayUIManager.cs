using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayUIManager : MonoBehaviour
{
    public static GameplayUIManager Instance;

    [Header("Gameplay UI")]
    [SerializeField] private MoneyBar _moneyBar;
    [SerializeField] private TimeBar _timeBar;
    [SerializeField] private TMP_Text _moneyText;
    [SerializeField] private TMP_Text _timeMinutesText;
    [SerializeField] private TMP_Text _timeSecondsText;
    public RecipeManager _recipeManager;
    [SerializeField] private GameObject _canvas;



    [Header("Setting UI")]
    [SerializeField] private GameObject _settingPanel;

    [Header("End Game UI")]
    [SerializeField] private GameObject _endGamePanel;
    [SerializeField] private TMP_Text _endgameAmountText;
    [SerializeField] private TMP_Text _endgameRentText;
    [SerializeField] private TMP_Text _endgameTotalText;

    private void Awake()
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

    void Start()
    {


    }

    void Update()
    {

    }

    private void HandleSceneSpecificLogic()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName == "Game")
        {
            _canvas.SetActive(true);

        }
        else
        {
            _canvas.SetActive(false);
        }

    }

    public void InitUI(int rent, float maxPlayTime)
    {
        HandleSceneSpecificLogic();

        _moneyBar.SetMaxMoney(rent * 5);
        _moneyBar.SetMoney(0);
        _moneyText.text = "0";

        // Initialize the time bar and timer
        _timeBar.SetMaxTime(maxPlayTime);

        _timeBar.SetTime(maxPlayTime);
    }

    public void UpdateTimeUI(float currentTime, bool isEndGame)
    {
        if (isEndGame)
        {

            _timeMinutesText.text = "00";
            _timeSecondsText.text = "00";
        }

        _timeBar.SetTime(currentTime);

        // Update the time display every frame
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        _timeMinutesText.text = minutes.ToString("00");
        _timeSecondsText.text = seconds.ToString("00");
    }

    public void UpdateMoneyUI(int currentMoney)
    {
        _moneyBar.SetMoney(currentMoney);
        _moneyText.text = currentMoney.ToString();
    }


    // Setting ////////////////////////////////////////////////////////////////////////////////////
    public void OpenSettingPanel()
    {
        _settingPanel.SetActive(true);
        GameplayManager.Instance.SetPause(true);
    }

    public void CloseSettingPanel()
    {
        _settingPanel.SetActive(false);
        GameplayManager.Instance.SetPause(false);
        TogglePlayerCanvas(true);
        AudioManager.instance.Play("Click");
    }

    public void TogglePlayerCanvas(bool isActive)
    {
        if (GameplayManager.Instance.playerController == null) return;
        GameplayManager.Instance.playerController._canvas.SetActive(isActive);
    }


    // End Game ///////////////////////////////////////////////////////////////////////////////////
    public void SetEndGamePanel(int currentMoney, int rent, int total)
    {
        StageManager.Instance.UnlockedStage++;

        _endGamePanel.SetActive(true);
        _endgameAmountText.text = $"{currentMoney}";
        _endgameRentText.text = $"- {rent}";
        _endgameTotalText.text = $"{total}";
    }

    // End Game Button Actions
    public void NextGameStage()
    {
        _endGamePanel.SetActive(false);
        _canvas.SetActive(false);
        AudioManager.instance.Play("Click");
        if (StageManager.Instance.CurrentStage >= StageManager.Instance.stageList.Count)
        {
            SceneManager.LoadScene("Stage");
        }
        else
        {
            SceneManager.LoadScene("Loading");
        }

    }

    public void BackToStage()
    {
        _endGamePanel.SetActive(false);
        _settingPanel.SetActive(false);
        _canvas.SetActive(false);
        AudioManager.instance.Play("Click");

        AudioManager.instance.StopSound("Game-BGM");
        AudioManager.instance.Play("Menu-BGM", 0.5f);

        SceneManager.LoadScene("Stage");

    }

    /// ///////////////////////////////////////////////////////////////////////////////////////////

}
