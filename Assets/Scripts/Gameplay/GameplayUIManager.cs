using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayUIManager : MonoBehaviour
{
    public GameplayManager gameplayManager;

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

    [Header("Tips UI")]
    [SerializeField] private GameObject _tipsPanel;

    [Header("End Game UI")]
    [SerializeField] private GameObject _endGamePanel;
    [SerializeField] private TMP_Text _endgameHeaderText;
    [SerializeField] private TMP_Text _endgameAmountText;
    [SerializeField] private TMP_Text _endgameRentText;
    [SerializeField] private TMP_Text _endgameTotalText;
    [SerializeField] private GameObject _passedButtons;
    [SerializeField] private GameObject _failedButtons;
    [SerializeField] private GameObject _gameOverButtons;

    string result = "";

    private void Awake()
    {

    }

    void Start()
    {

    }

    void Update()
    {

    }

    public void InitUI(int rent, float maxPlayTime)
    {
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
        AudioManager.instance.Play("Click");
        _settingPanel.SetActive(true);
    }

    public void CloseSettingPanel()
    {
        AudioManager.instance.Play("Click");
        _settingPanel.SetActive(false);
        gameplayManager.SetPause(false);
    }


    // Tips ///////////////////////////////////////////////////////////////////////////////////////
    public void OpenTipsPanel()
    {
        AudioManager.instance.Play("Click");
        _tipsPanel.SetActive(true);
    }

    public void CloseTipsPanel()
    {
        AudioManager.instance.Play("Click");
        _tipsPanel.SetActive(false);
        gameplayManager.SetPause(false);
    }


    // End Game ///////////////////////////////////////////////////////////////////////////////////
    public void SetEndGamePanel(int currentMoney, int rent, int total, string endGameText)
    {
        result = endGameText;
        Debug.Log("End Game: " + result);
        _endGamePanel.SetActive(true);
        _endgameHeaderText.text = endGameText;
        _endgameAmountText.text = $"{currentMoney}";
        _endgameRentText.text = $"- {rent}";
        _endgameTotalText.text = $"{total}";
    }

    // End Game Button Actions
    public void NextGameStage()
    {
        _endGamePanel.SetActive(false);
        _settingPanel.SetActive(false);
        _canvas.SetActive(false);
        AudioManager.instance.Play("Click");

        if (StageManager.Instance.CurrentStage >= StageManager.Instance.stageList.Count)
        {
            AudioManager.instance.StopSound("Game-BGM");
            AudioManager.instance.Play("Menu-BGM", 0.5f);

            SaveSystem.Save();

            SceneManager.LoadScene("Stage");
        }
        else
        {
            StageManager.Instance.CurrentStage++;
            StageManager.Instance.UnlockedStage = 0;
            StageManager.Instance.IsStageComplete = false;

            SceneManager.LoadScene("Game");
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

        SaveSystem.Save();

        SceneManager.LoadScene("Stage");

    }

    public void RetryGame()
    {
        _endGamePanel.SetActive(false);
        _settingPanel.SetActive(false);
        _canvas.SetActive(false);

        AudioManager.instance.Play("Click");

        StageManager.Instance.UnlockedStage = 0;
        StageManager.Instance.IsStageComplete = false;

        SaveSystem.Save();

        SceneManager.LoadScene("Game");
    }

    public void ResetSave()
    {
        _endGamePanel.SetActive(false);
        _canvas.SetActive(false);

        AudioManager.instance.Play("Click");

        AudioManager.instance.StopSound("Game-BGM");
        AudioManager.instance.Play("Menu-BGM", 0.5f);

        StageManager.Instance.UnlockedStage = 0;
        StageManager.Instance.IsStageComplete = false;
        StageManager.Instance.AllMoney = 0;

        SaveSystem.Save();

        SceneManager.LoadScene("Stage");
    }

    public void OpenPassedButtons()
    {
        _passedButtons.SetActive(true);
        _gameOverButtons.SetActive(false);
    }

    public void OpenFailedButtons()
    {
        _passedButtons.SetActive(true);
        _gameOverButtons.SetActive(false);
    }

    public void OpenGameOverButtons()
    {
        _passedButtons.SetActive(false);
        _gameOverButtons.SetActive(true);
    }

    /// ///////////////////////////////////////////////////////////////////////////////////////////

}
