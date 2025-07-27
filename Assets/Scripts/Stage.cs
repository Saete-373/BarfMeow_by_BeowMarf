using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Stage : MonoBehaviour
{
    [SerializeField] private StageUIManager stageUIManager;

    [SerializeField] private int stageNumber;
    [SerializeField] private TMP_Text TimeText;
    [SerializeField] private TMP_Text RentText;
    [SerializeField] private GameObject unlockedStageButton;
    [SerializeField] private GameObject lockedStageButton;

    void Start()
    {
        StageObject stage = StageManager.Instance.stageList[stageNumber - 1];
        int minute = stage.playTime / 60;
        int second = stage.playTime % 60;
        TimeText.text = $"{minute:D2}:{second:D2}";
        RentText.text = $"{stage.rent} $";

    }

    public void PlayGame()
    {
        AudioManager.instance.Play("Click");
        stageUIManager.LoadGame(stageNumber);
    }

    public void SetUnlocked(bool isUnlocked)
    {
        unlockedStageButton.SetActive(isUnlocked); // Open -> True
        lockedStageButton.SetActive(!isUnlocked); // Open -> False

        gameObject.GetComponent<Button>().interactable = isUnlocked;

    }
}
