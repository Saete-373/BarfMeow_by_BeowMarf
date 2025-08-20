using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private Image tutorialCard;
    [SerializeField] private List<Sprite> tutorialCards;
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject prevButton;
    [SerializeField] private TMP_Text endText;
    [SerializeField] private Animator catAnim;

    private int currentCardIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        currentCardIndex = 0;
        tutorialCard.sprite = tutorialCards[0];

        prevButton.SetActive(false);
        nextButton.SetActive(true);

        endText.text = "SKIP";

        catAnim.Play("NoSpeak");
        StartCoroutine(CatSpeak());
    }

    private IEnumerator CatSpeak()
    {

        yield return new WaitForSeconds(1f);
        catAnim.Play("Speak");
    }

    public void NextCard()
    {
        if (currentCardIndex < tutorialCards.Count - 1)
        {
            currentCardIndex++;
            tutorialCard.sprite = tutorialCards[currentCardIndex];

            prevButton.SetActive(true);
        }

        if (currentCardIndex == tutorialCards.Count - 1)
        {
            nextButton.SetActive(false);
            prevButton.SetActive(true);

            endText.text = "END";
        }
    }

    public void PrevCard()
    {
        if (currentCardIndex > 0)
        {
            currentCardIndex--;
            tutorialCard.sprite = tutorialCards[currentCardIndex];

            nextButton.SetActive(true);
        }

        if (currentCardIndex == 0)
        {
            prevButton.SetActive(false);
            nextButton.SetActive(true);
        }
    }

    public void CloseTutorial()
    {

        StartCoroutine(DelayNewGame(0.5f));
    }

    public void BackToMenu()
    {
        gameObject.SetActive(false);
    }

    private IEnumerator DelayNewGame(float delay)
    {
        // Debug.Log("Starting a new game.");
        AudioManager.instance.Play("Click");

        if (StageManager.Instance != null)
        {
            StageManager.Instance.SetNewGameStage();
        }
        else
        {
            Debug.LogError("StageManager.Instance is null!");
        }

        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
        SceneManager.LoadScene("Game");
    }
}
