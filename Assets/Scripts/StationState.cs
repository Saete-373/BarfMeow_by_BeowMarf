using System.Collections;
using System.Linq;
using UnityEngine;

public class StationState : MonoBehaviour
{
    [SerializeField] private IngredientObjectList ingredientList;
    [SerializeField] private FoodObjectList foodList;
    public GameObject currentIngredient;
    private State currentStationState = State.Empty;
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

    void FixedUpdate()
    {
        if (currentStationState != saveState)
        {
            saveState = currentStationState;

            switch (currentStationState)
            {
                case State.Empty:
                    GetComponentInChildren<Animator>().Play("Empty");
                    currentIngredient = null;
                    break;
                case State.Cooking:
                    FindObjectOfType<AudioManager>().Play("Boiling");
                    GetComponentInChildren<Animator>().Play("Cooking");
                    StartCoroutine(CookingFood());
                    break;
                case State.Finish:
                    FindObjectOfType<AudioManager>().StopSound("Boiling");
                    CheckPotState();
                    break;
            }
        }

    }

    private IEnumerator CookingFood()
    {
        // if (currentStationState == State.Cooking)
        // {
        //     currentIngredient.name += "_Boil";
        // }
        isFinished = false;

        yield return new WaitForSeconds(5f);

        currentStationState = State.Finish;
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

    public bool CheckState(string state)
    {
        return state.ToLower() switch
        {
            "empty" => currentStationState == State.Empty,
            "cooking" => currentStationState == State.Cooking,
            "finish" => currentStationState == State.Finish,
            _ => false,
        };
    }

    private void CheckPotState()
    {
        if (isFinished) return;

        isFinished = true;

        bool hasInIngredient = ingredientList.data.Any(ingredient => ingredient.ingredientName == currentIngredient.name && currentIngredient.name != "Waste");
        bool hasInFood = foodList.data.Any(food => food.foodName == currentIngredient.name);
        // Debug.Log("hasInIngredient: " + hasInIngredient
        //     + ", hasInFood: " + hasInFood
        //     + ", currentIngredient: " + currentIngredient.name);
        bool hasWaste = !hasInIngredient && !hasInFood;

        if (hasWaste)
        {
            currentIngredient.name = "Waste";
            GetComponentInChildren<Animator>().Play("Fail");
            AudioManager.instance.Play("Cooked-Fail");
        }
        else
        {
            GetComponentInChildren<Animator>().Play("Complete");
            AudioManager.instance.Play("Cooked-Success");
        }
    }

}
