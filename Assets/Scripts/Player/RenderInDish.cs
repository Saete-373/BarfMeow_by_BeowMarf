using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RenderInDish : MonoBehaviour
{
    public FoodObject CurrentFood;
    public List<string> saveItems;

    [Header("Dependencies")]
    public PlayerController playerController;

    [Space]

    [Header("Positions")]
    [SerializeField] private Vector3 wastePosition = new(0, -0.017f, 0);
    [SerializeField] private Vector3 milkPosition = new(0, -0.016f, 0);
    [Space]

    private List<List<Vector3>> ingredientPositions = new()
        {
            new List<Vector3>
            {
                new(0, -0.005f, 0)
            },
            new List<Vector3>
            {
                new(-0.087f, -0.005f, 0),
                new(0.075f, -0.012f, 0)
            },
            new List<Vector3>
            {
                new(-0.092f, -0.005f, 0),
                new(0.07f, -0.013f, 0),
                new(0.004f, -0.0489f, 0)
            },
            new List<Vector3>
            {
                new(-0.092f, -0.005f, 0),
                new(0.084f, -0.013f, 0),
                new(-0.05f, 0.048f, 0),
                new(0.055f, 0.018f, 0)
            }
        };

    public bool IsSaveItemsNull() => saveItems == null || saveItems.Count == 0;

    public void RenderDish(PlayerController player)
    {
        playerController = player;
        playerController.renderInDish = this;

        LoadItemFromInventory();
    }

    private void LoadItemFromInventory()
    {

        List<string> itemNames;

        if (playerController?.inventory?.items != null)
        {
            var inventoryItems = playerController.inventory.items.OrderBy(x => x).ToList();
            var currentSaveItems = saveItems?.OrderBy(x => x).ToList() ?? new List<string>();

            if (!inventoryItems.SequenceEqual(currentSaveItems))
            {
                for (int i = transform.childCount - 1; i >= 0; i--)
                {
                    Destroy(transform.GetChild(i).gameObject);
                }

                itemNames = inventoryItems;
                saveItems = new List<string>(itemNames);
            }
            else
            {
                itemNames = saveItems?.OrderBy(x => x).ToList() ?? new List<string>();
            }
        }
        else if (saveItems == null || saveItems.Count == 0)
        {
            itemNames = new List<string>();
            saveItems = new List<string>();
        }
        else
        {
            itemNames = saveItems.OrderBy(x => x).ToList();
        }


        playerController.gameplayManager.renderInPlate.ClearPlate();

        List<string> ingredientNames = StageManager.Instance.ingredientNames
            .OrderBy(name => name)
            .ToList();

        bool hasInList = itemNames.All(item => ingredientNames.Contains(item)); // false -> waste
        if (!hasInList)
        {
            SpawnWaste();
            return;
        }

        FoodObject foodObject = StageManager.Instance.foodList.data
                                .Find(
                                    food => food.ingredients
                                    .Select(x => x.ingredientName)
                                    .OrderBy(name => name)
                                    .SequenceEqual(itemNames)
                                );

        if (foodObject == null)
        {
            SpawnIngredient(itemNames);
            return;
        }

        CurrentFood = foodObject;
        SpawnFood();
    }

    private void SpawnWaste()
    {
        Transform instantiatedWaste = Instantiate(StageManager.Instance.wasteIngredient.prefab, transform.position, Quaternion.identity, transform);
        instantiatedWaste.localPosition = wastePosition;
        Debug.Log("Waste detected");
    }

    private void SpawnIngredient(List<string> ingredientNames)
    {

        List<IngredientObject> currentIngredientList = StageManager.Instance.ingredientList.data
            .Where(ingredient => ingredientNames.Contains(ingredient.ingredientName))
            .ToList();

        int indexOfSet = currentIngredientList.Count != 0 ? currentIngredientList.Count - 1 : 0;

        List<Vector3> positionsOfSet = ingredientPositions[indexOfSet];

        for (int i = 0; i < currentIngredientList.Count; i++)
        {
            Vector3 spawnPosition = positionsOfSet[i];
            if (StageManager.Instance.milkList.data.Contains(currentIngredientList[i]))
            {
                spawnPosition = milkPosition;
            }

            Transform instantiatedFoodPart = Instantiate(
                currentIngredientList[i].prefab,
                transform.position,
                Quaternion.identity,
                transform
            );

            instantiatedFoodPart.localPosition = spawnPosition;
        }
    }

    private void SpawnFood()
    {
        Transform spawnedFood = Instantiate(CurrentFood.prefab.transform, transform.position, Quaternion.identity, transform);
        spawnedFood.localPosition = new Vector3(0, 0, 0);

        gameObject.GetComponent<SpriteRenderer>().enabled = false;

        foreach (Transform child in transform)
        {
            if (child != spawnedFood && child != transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
