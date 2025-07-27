using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class IngredientDetector : MonoBehaviour
{
    #region Scriptable Objects
    public FoodObjectList foodList;
    public IngredientObjectList baseIngredientList;
    public List<IngredientObject> ingredientList;
    public IngredientObjectList whiteLiquidList;
    public IngredientObject wasteIngredient;

    #endregion

    #region Public Variables
    [Header("Display")]
    public int _maxIngredients = 4;

    #endregion

    #region Editor Data
    [Header("Dependencies")]

    #endregion

    #region Internal Data
    List<IngredientObject> currentIngredientList = new();
    private List<List<Vector3>> _ingredientPositions;
    private Vector3 wastePosition = new(0, -0.017f, 0);
    private Vector3 milkPosition = new(0, -0.016f, 0);
    private bool isUpdating = false;

    #endregion



    // Start is called before the first frame update
    void Start()
    {

        _ingredientPositions = new()
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

    }

    // Update is called once per frame
    void Update()
    {
        if (!currentIngredientList.SequenceEqual(ingredientList))
        {
            // Debug.Log("Ingredients have changed, updating...");
            if (isUpdating) return;

            isUpdating = true;
            // Debug.Log("Difference detected");
            currentIngredientList = new List<IngredientObject>(ingredientList);

            // Detect Food First
            bool hasInMenu = foodList.data
                .Any(food => food.ingredients
                    .Where(x => x != null && x.ingredientName != null)
                    .OrderBy(x => x.ingredientName)
                    .SequenceEqual(currentIngredientList
                        .Where(x => x != null && x.ingredientName != null)
                        .OrderBy(x => x.ingredientName)
                    )
                );

            Debug.Log("Has in Menu: " + hasInMenu);
            // for (int i = 0; i < currentIngredientList.Count; i++)
            // {
            //     Debug.Log("Ingredient: " + currentIngredientList[i].ingredientName);
            // }

            if (hasInMenu)
            {
                // Get Menu Name
                FoodObject menuObject = foodList.data
                    .FirstOrDefault(food => food.ingredients
                        .Where(x => x != null && x.ingredientName != null)
                        .Select(x => x.ingredientName)
                        .OrderBy(name => name)
                        .SequenceEqual(currentIngredientList
                            .Where(x => x != null && x.ingredientName != null)
                            .Select(x => x.ingredientName)
                            .OrderBy(name => name)
                        )
                    );

                Debug.Log("Menu detected: " + menuObject.foodName);

                // Instantiate Menu
                Transform instantiatedMenu = Instantiate(menuObject.foodPrefab.transform, transform.position, Quaternion.identity, transform);
                instantiatedMenu.localPosition = new Vector3(0, 0, 0);

                // Clean up existing food parts in the dish before adding the menu item
                foreach (Transform child in transform)
                {
                    if (child != instantiatedMenu && child != transform)
                    {
                        Destroy(child.gameObject);
                    }
                }
                isUpdating = false;

            }
            else
            {
                int numberOfIngredients = currentIngredientList.Count != 0 ? currentIngredientList.Count - 1 : 0;
                List<Vector3> spawnPositions = _ingredientPositions[numberOfIngredients];

                // Filter out Milk Ingredients
                if (whiteLiquidList.data.Any(ingredient => currentIngredientList.Contains(ingredient)))
                {
                    // Instantiate Milk
                    Transform instantiatedMilk = Instantiate(whiteLiquidList.data[0].ingredientPart, transform.position, Quaternion.identity, transform);
                    instantiatedMilk.localPosition = milkPosition;
                    Debug.Log("Milk detected");
                    isUpdating = false;
                }

                bool hasWaste = currentIngredientList.Any(ingredient => !baseIngredientList.data.Contains(ingredient));

                if (hasWaste)
                {
                    // Instantiate waste
                    SpawnWaste();
                    isUpdating = false;
                }
                else
                {
                    // Loop Render Food Parts in Dish
                    for (int i = 0; i < currentIngredientList.Count; i++)
                    {
                        // Instantiate Food Part
                        Transform instantiatedFoodPart = Instantiate(
                            currentIngredientList[i].ingredientPart,
                            transform.position,
                            Quaternion.identity,
                            transform
                        );
                        instantiatedFoodPart.localPosition = spawnPositions[i];
                        Debug.Log("Spawned: " + instantiatedFoodPart.name);
                    }
                    isUpdating = false;
                }


            }
        }
    }

    public void SpawnWaste()
    {
        Transform instantiatedWaste = Instantiate(wasteIngredient.ingredientPart, transform.position, Quaternion.identity, transform);
        instantiatedWaste.localPosition = wastePosition;
        Debug.Log("Waste detected");
    }
}
