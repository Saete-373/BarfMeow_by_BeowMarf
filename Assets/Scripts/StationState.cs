using System.Collections;
using System.Linq;
using UnityEngine;

public class StationState : MonoBehaviour
{
    public string CurrentIngredientName;
    public PlayerController player;
    public float cookingTime;
    [SerializeField] private bool isLockMove;
    [SerializeField] private Animator animator = null;
    [SerializeField] private GameObject _cloudEffect;
    [SerializeField] private State currentStationState = State.Empty;
    private bool isFinished = false;

    public enum State
    {
        Empty,
        Cooking,
        Finish,
        None
    }
    private State saveState = State.None;

    void Start()
    {

    }

    void Update()
    {
        if (currentStationState != saveState)
        {
            saveState = currentStationState;

            switch (currentStationState)
            {
                case State.Empty:
                    PlayAnim("Empty");
                    CurrentIngredientName = null;
                    break;
                case State.Cooking:
                    if (gameObject.CompareTag("boil_station")) AudioManager.instance.Play("Boiling");
                    PlayAnim("Cooking");
                    break;
                case State.Finish:
                    if (gameObject.CompareTag("boil_station")) AudioManager.instance.StopSound("Boiling");
                    CheckResult();
                    break;
            }
        }

    }

    public void SetState(string state)
    {
        switch (state.ToLower())
        {
            case "empty":
                currentStationState = State.Empty;
                break;
            case "cooking":
                currentStationState = State.Cooking;
                break;
            case "finish":
                currentStationState = State.Finish;
                break;
            default:
                Debug.LogWarning($"Unknown state: {state}");
                break;
        }
    }

    public void CallCook(string ingredientName, PlayerController playerController)
    {
        if (currentStationState == State.Cooking || currentStationState == State.Finish) return;

        if (string.IsNullOrEmpty(ingredientName)) return;

        player = playerController;

        currentStationState = State.Cooking;

        CurrentIngredientName = ingredientName + GetCookMethod();

        if (isLockMove)
        {
            _cloudEffect.SetActive(true);
        }

        StartCoroutine(CookingFood());
    }

    private string GetCookMethod()
    {
        switch (gameObject.tag)
        {
            case "cut_station":
                return "_Cut";
            case "boil_station":
                return "_Boil";
            case "refrigerator":
                return "_Freeze";
            default:
                Debug.LogWarning("Unknown station type");
                return null;
        }
    }

    private IEnumerator CookingFood()
    {
        // Debug.Log("Cooking food: " + CurrentIngredientName);
        isFinished = false;


        yield return new WaitForSeconds(cookingTime);

        // Debug.Log("Finished cooking: " + CurrentIngredientName);

        currentStationState = State.Finish;
        if (isLockMove)
        {
            _cloudEffect.SetActive(false);
        }
    }



    public bool CheckCookState(string state)
    {
        return state.ToLower() switch
        {
            "empty" => currentStationState == State.Empty,
            "cooking" => currentStationState == State.Cooking,
            "finish" => currentStationState == State.Finish,
            _ => false,
        };
    }

    private void CheckResult()
    {
        if (isFinished) return;

        isFinished = true;

        bool hasInIngredient = StageManager.Instance.ingredientNames.Contains(CurrentIngredientName);
        bool hasInFood = StageManager.Instance.foodList.data.Any(food => food.foodName == CurrentIngredientName);

        bool hasWaste = !hasInIngredient && !hasInFood;

        if (hasWaste)
        {
            CurrentIngredientName = "Waste";
            PlayAnim("Fail");
            AudioManager.instance.Play("Cooked-Fail");
        }
        else
        {
            PlayAnim("Complete");
            AudioManager.instance.Play("Cooked-Success");
        }

        UpdateIngredientName();
    }

    public void UpdateIngredientName()
    {
        if (CurrentIngredientName == null) return;

        if (player.tableData == null || player.tableData.ItemOnTable == null) return;

        player.tableData.ItemOnTable.name = CurrentIngredientName;
    }

    public string GetCurrentIngredient()
    {
        if (CurrentIngredientName == null) return null;

        currentStationState = State.Empty;

        return CurrentIngredientName;
    }

    private void PlayAnim(string animName)
    {
        if (animator != null)
        {
            animator.Play(animName);
        }
    }


}
