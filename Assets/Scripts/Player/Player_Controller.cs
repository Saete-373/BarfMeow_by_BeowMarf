using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player_Controller : MonoBehaviour
{
    #region Enums
    private enum Directions { Up, Down, Left, Right, Up_Left, Up_Right, Down_Left, Down_Right, NONE };

    #endregion

    #region Scriptable Objects
    [SerializeField] private IngredientObjectList baseIngredientList;

    #endregion


    #region Editor Data
    [Header("Movement Attributes")]
    [SerializeField] private float _moveSpeed = 200f;

    [Header("Dependencies")]
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Table_Detector _tableDetector;
    [SerializeField] private GameObject _handSlot;
    [SerializeField] private GameObject _ingredientSlot;
    [SerializeField] private List<GameObject> _items;
    [SerializeField] private GestureRecognizer _gestureRecognizer;
    [SerializeField] private GameObject _plateList;
    [SerializeField] private GameObject _orderList;
    // [SerializeField] private GameplayUIManager _gameUIManager;
    [SerializeField] private GameObject _cloudEffect;
    [SerializeField] private GameObject _canvas;

    #endregion

    #region Instance Data


    #endregion

    #region Import Data
    private GameObject currentTable = null;
    private string currentGesture = "none";


    #endregion


    #region Internal Data
    private bool isMovable = true;
    private Vector2 _moveDir = Vector2.zero;
    private Directions _facingDirection = Directions.NONE;

    private readonly string dishName = "999Dish";
    private readonly List<Vector3> DISH_HANDPOSITION = new List<Vector3> {
                                                            new(0, -0.254f, 0), // Down
                                                            new(0, 0.049f, 0), // Up
                                                            new(-0.392f, -0.188f, 0), // Left
                                                            new(0.392f, -0.188f, 0) // Right
                                                        };
    private readonly List<Vector3> ITEM_HANDPOSITION = new List<Vector3> {
                                                            new(0, -0.313f, 0), // Down
                                                            new(0, 0, 0), // Up
                                                            new(-0.349f, -0.313f, 0), // Left
                                                            new(0.349f, -0.313f, 0) // Right
                                                        };
    private readonly Vector3 DISH_TABLEPOSITION = new(0, -0.056f, 0);
    private readonly Vector3 ITEM_TABLEPOSITION = new(0, -0.17f, 0);
    private bool isCooldownHand = false;
    private Coroutine OnOpenRecipe;
    private Coroutine OnOpenSetting;

    #endregion

    private void Start()
    {
        HandleSceneSpecificLogic();
    }

    private void HandleSceneSpecificLogic()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        Debug.Log("Current Scene Player: " + currentSceneName);
        if (currentSceneName != "Game")
        {
            _canvas.SetActive(false);
        }
        else
        {
            _canvas.SetActive(true);
            GameplayManager.Instance.InitGameData();
        }

    }


    #region Tick 
    private void Update()
    {

        RecognizeGesture();
        // GatherInput();

    }

    private void FixedUpdate()
    {
        MovementUpdate();
    }

    #endregion

    #region Time Logic

    private IEnumerator DelayAction(float delay)
    {
        yield return new WaitForSeconds(delay);
        // Perform the action here
    }

    private IEnumerator DelayCookingAction(float delay)
    {
        _cloudEffect.SetActive(true);
        yield return new WaitForSeconds(delay);
        _cloudEffect.SetActive(false);
    }

    #endregion


    #region Input Logic

    private void RecognizeGesture()
    {
        if (_gestureRecognizer.currentGesture == currentGesture)
            return;

        currentGesture = _gestureRecognizer.currentGesture;

        if (string.IsNullOrEmpty(currentGesture) || currentGesture == "none")
        {
            _moveDir = Vector2.zero;
            return;
        }

        if (currentGesture != "recipe")
        {
            if (OnOpenRecipe != null)
            {
                StopCoroutine(OnOpenRecipe);
                OnOpenRecipe = null;
            }
            GameplayUIManager.Instance._recipeManager.CloseRecipePanel();
        }

        if (currentGesture != "setting")
        {
            if (OnOpenSetting != null)
            {
                StopCoroutine(OnOpenSetting);
                OnOpenSetting = null;
            }
        }


        // Debug.Log(currentGesture);
        switch (currentGesture)
        {
            // Movement - Horizontal
            case "left":
                // _moveDir.x = -1f;
                if (isMovable)
                {
                    if (_handSlot.transform.childCount > 0)
                    {
                        Transform itemInHand = _handSlot.transform.GetChild(0);
                        if (itemInHand.name.Contains(dishName))
                        {
                            itemInHand.localPosition = DISH_HANDPOSITION[2];
                        }
                        else
                        {
                            itemInHand.localPosition = ITEM_HANDPOSITION[2];
                        }
                    }

                    _moveDir = new Vector2(-1, 0);
                }
                break;
            case "right":
                if (isMovable)
                {
                    if (_handSlot.transform.childCount > 0)
                    {
                        Transform itemInHand = _handSlot.transform.GetChild(0);
                        if (itemInHand.name.Contains(dishName))
                        {
                            itemInHand.localPosition = DISH_HANDPOSITION[3];
                        }
                        else
                        {
                            itemInHand.localPosition = ITEM_HANDPOSITION[3];
                        }
                    }
                    _moveDir = new Vector2(1, 0);
                }
                break;

            // Movement - Vertical
            case "straight":
                if (isMovable)
                {
                    if (_handSlot.transform.childCount > 0)
                    {
                        Transform itemInHand = _handSlot.transform.GetChild(0);
                        if (itemInHand.name.Contains(dishName))
                        {
                            itemInHand.localPosition = DISH_HANDPOSITION[0];
                        }
                        else
                        {
                            itemInHand.localPosition = ITEM_HANDPOSITION[0];
                        }
                    }

                    _moveDir = new Vector2(0, -1);
                }
                break;
            case "back":
                if (isMovable)
                {
                    if (_handSlot.transform.childCount > 0)
                    {
                        Transform itemInHand = _handSlot.transform.GetChild(0);
                        if (itemInHand.name.Contains(dishName))
                        {
                            itemInHand.localPosition = DISH_HANDPOSITION[1];
                        }
                        else
                        {
                            itemInHand.localPosition = ITEM_HANDPOSITION[1];
                        }
                    }

                    _moveDir = new Vector2(0, 1);
                }
                break;

            // Interaction with tables
            case "grab":
                HandleGrabInteraction();
                break;

            // Cooking interactions
            case "cooking":
                HandleCookingInteraction();
                break;

            // Delivery actions
            case "yes":
                DeliveryFood();
                break;

            case "no":
                break;

            // Stop action
            case "stop":
                _moveDir = Vector2.zero;
                break;

            // Open Recipe
            case "recipe":
                OnOpenRecipe = StartCoroutine(DelayOpenRecipe(1f));

                break;

            // Open Setting
            case "setting":
                OnOpenSetting = StartCoroutine(DelayOpenSetting(1f));

                break;
        }

        _rb.velocity = _moveDir.normalized * _moveSpeed * Time.fixedDeltaTime;

        CalculateFacingDetection();
        UpdateAnimation();
    }

    #region Grab Interaction 
    // Grab Interaction
    private void HandleGrabInteraction()
    {
        if (_tableDetector._currentTable == null)
        {
            return;
        }

        currentTable = _tableDetector._currentTable;
        // Debug.Log(currentTable.name);

        switch (currentTable.tag)
        {
            case "table":
                // Debug.Log("Place Something on the table");

                Transform placeArea = currentTable.transform.Find("PlaceArea");
                Transform dishOnHand = _handSlot.transform.Find(dishName);
                Transform dishOnTable = placeArea.transform.Find(dishName);

                Transform ingredientOnHand = _handSlot.transform.Cast<Transform>().FirstOrDefault(obj => obj.name != dishName);
                Transform ingredientOnTable = placeArea.transform.Cast<Transform>().FirstOrDefault(obj => obj.name != dishName);

                // ห้ามทำซ้ำ
                if ((dishOnHand != null && dishOnTable != null) || (ingredientOnHand != null && ingredientOnTable != null))
                    return;

                // หากมือมีของ -> วาง
                if (_handSlot.transform.childCount > 0 && placeArea.transform.childCount == 0)
                {
                    StartCoroutine(PlaceItem(placeArea));
                    return;
                }

                // หากมือว่าง -> หยิบ
                if (_handSlot.transform.childCount == 0 && placeArea.transform.childCount > 0)
                {
                    StartCoroutine(GetItem(placeArea));
                    return;
                }

                // จานในมือ + วัตถุดิบบนโต๊ะ
                if (dishOnHand != null && ingredientOnTable != null && ingredientOnTable.CompareTag("ingredient"))
                {
                    AddIngredientToDish(dishOnHand, ingredientOnTable);
                    Destroy(ingredientOnTable.gameObject);
                    return;
                }

                // จานบนโต๊ะ + วัตถุดิบในมือ
                if (dishOnTable != null && ingredientOnHand != null && ingredientOnHand.CompareTag("ingredient"))
                {
                    AddIngredientToDish(dishOnTable, ingredientOnHand);
                    Destroy(ingredientOnHand.gameObject);
                    return;
                }

                break;
            case "collectable":
                string itemName = currentTable.name.Split(" ")[0].Replace("Box", "");
                if (itemName == "Dish")
                {
                    itemName = dishName;
                }
                // Debug.Log($"Grab {itemName}");
                BlinkToStation(currentTable.transform.position);
                GrabItem(itemName);
                break;
            case "bin":
                // Debug.Log("Drop something");
                FindObjectOfType<AudioManager>().Play("Trash-Bin");
                BlinkToStation(currentTable.transform.position);
                ClearHand();
                break;
                // default:
                // Debug.Log("Unknown Object Detected");
                // break;
        }


    }
    #endregion

    #region Cooking Interaction
    // Cooking Interaction
    private void HandleCookingInteraction()
    {
        if (_tableDetector._currentTable == null)
        {
            Debug.Log("No table detected");
            return;
        }

        currentTable = _tableDetector._currentTable;

        // if (currentTable.CompareTag("boil_station"))
        // {
        //     // Debug.Log("Cooking Station Detected");
        // }

        // StationState stationState = currentTable.GetComponent<StationState>();

        if (currentTable.CompareTag("table") || currentTable.CompareTag("collectable") || currentTable.CompareTag("bin") || currentTable.CompareTag("delivery")) return;

        // Add null checks for PlaceArea
        Transform placeArea = currentTable.transform.Find("PlaceArea");
        GameObject itemOnTable = null;

        if (placeArea != null && placeArea.childCount > 0)
        {
            itemOnTable = placeArea.GetChild(0).gameObject;
        }

        GameObject ingredientInHand = _handSlot.transform.Cast<Transform>().Select(t => t.gameObject).FirstOrDefault(obj => obj.name != dishName);
        GameObject dishInHand = _handSlot.transform.Cast<Transform>().Select(t => t.gameObject).FirstOrDefault(obj => obj.name == dishName);

        if (itemOnTable == null && ingredientInHand != null && dishInHand == null)
        {
            // if (stationState.CheckState("cooking")) return;

            // stationState.SetState("cooking");

            switch (currentTable.tag)
            {
                case "cut_station":
                    BlinkToStation(currentTable.transform.position);
                    // Debug.Log("Start Cutting Food");
                    StartCoroutine(CutFood(ingredientInHand));
                    // Debug.Log("Cutting Food Completed");

                    break;
                case "boil_station":
                    BlinkToStation(currentTable.transform.position);
                    BoilFood(ingredientInHand);
                    break;
                case "refrigerator":
                    BlinkToStation(currentTable.transform.position);
                    StartCoroutine(FreezeFood(ingredientInHand));
                    break;
                default:
                    Debug.Log("Unknown Cooking Type");
                    StartCoroutine(DelayCookingAction(1f));
                    break;
            }

            StartCoroutine(DelayAction(1f));
            // Debug.Log("Cook Completed");
        }
        else if (itemOnTable != null && ingredientInHand == null && dishInHand != null)
        {
            BlinkToStation(currentTable.transform.position);
            StartCoroutine(GetFood(itemOnTable, dishInHand));
            StartCoroutine(DelayAction(1f));
            // Debug.Log("Get Food Completed");
        }
        else if (itemOnTable == null && ingredientInHand == null && dishInHand != null)
        {
            if (dishInHand.transform.childCount != 1)
            {
                Debug.Log("Can repeatly cook with only 1 ingredient in dish");
                return;
            }

            // if (stationState.CheckState("cooking")) return;

            // stationState.SetState("cooking");

            string ingredientName = dishInHand.transform.GetComponent<IngredientDetector>().ingredientList
            .Select(ingredientObj => ingredientObj.ingredientName)
            .ToList()[0];

            switch (currentTable.tag)
            {
                case "cut_station":
                    BlinkToStation(currentTable.transform.position);
                    StartCoroutine(ReCutFood(dishInHand));

                    break;
                case "boil_station":
                    BlinkToStation(currentTable.transform.position);
                    ReBoilFood(dishInHand);

                    break;
                case "refrigerator":
                    BlinkToStation(currentTable.transform.position);
                    StartCoroutine(ReFreezeFood(dishInHand));

                    break;
                default:
                    Debug.Log("Unknown Cooking Type");

                    break;
            }

            StartCoroutine(DelayAction(0.5f));
            // Debug.Log("Cook Completed");
        }
        else
        {

            Debug.Log("Do Nothing!");
            Debug.Log("itemOnTable: " + (itemOnTable != null));
            Debug.Log("ingredientInHand: " + (ingredientInHand != null));
            Debug.Log("dishInHand: " + (dishInHand != null));
        }



    }
    #endregion

    #endregion

    #region Movement Logic
    private void MovementUpdate()
    {
        _rb.velocity = _moveDir.normalized * _moveSpeed * Time.fixedDeltaTime;
    }

    private void BlinkToStation(Vector3 stationPosition)
    {
        Vector3 currentPos = transform.position;
        Vector3[] positions = new Vector3[]
        {
            stationPosition + new Vector3(0, 0.3779929f),    // Upper
            stationPosition + new Vector3(0, -0.45938f),   // Lower
            stationPosition + new Vector3(-0.6908719f, -0.1111792f), // Left
            stationPosition + new Vector3(0.6908719f, 0.1111792f)   // Right
        };

        Vector3 closestPosition = positions[0];
        float minDistance = Vector3.SqrMagnitude(positions[0] - currentPos);

        for (int i = 1; i < positions.Length; i++)
        {
            float distance = Vector3.SqrMagnitude(positions[i] - currentPos);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPosition = positions[i];
            }
        }

        if (minDistance >= 1f)
        {
            Debug.Log("Too far from the station");
            return;
        }

        // Debug.Log(minDistance);

        transform.position = closestPosition;
    }
    #endregion

    #region Stock Box's function(s)

    // Stock Box's function(s) ----------------------------------------------------------------------------------
    private void GrabItem(string current_item)
    {
        // Find the item to collect
        GameObject collectedItem = _items.Find(item => item.name == current_item);
        GameObject dishOnHand = _handSlot.transform.Find(dishName)?.gameObject;
        GameObject ingredientOnHand = _handSlot.transform.Cast<Transform>().Select(t => t.gameObject).FirstOrDefault(obj => obj.name != dishName);
        int ingredientCount = _handSlot.transform.Cast<Transform>().Count(child => child.name != dishName);



        // Item Not in the list -> Can't collect
        if (collectedItem == null)
        {
            Debug.LogError($"Item with name {current_item} not found in the list.");
            return;
        }

        // Hand Not Empty -> Can't collect
        if (_handSlot.transform.childCount > 0)
        {
            Debug.Log("Hands are full!");
            return;
        }

        // Instantiate the item
        GameObject instantiatedItem = Instantiate(collectedItem, _handSlot.transform.position, Quaternion.identity, _handSlot.transform);
        instantiatedItem.name = collectedItem.name;

        if (instantiatedItem.CompareTag("dish"))
        {
            FindObjectOfType<AudioManager>().Play("Plate");
        }
        else
        {
            FindObjectOfType<AudioManager>().Play("Close-Fridge");
        }

        // Set position
        Vector3 spawnPosition = current_item == dishName ? DISH_HANDPOSITION[0] : ITEM_HANDPOSITION[0];
        instantiatedItem.transform.localPosition = spawnPosition;

        StartCoroutine(DelayAction(1f));
    }

    // ----------------------------------------------------------------------------------------------------------
    #endregion

    #region Table's function(s)
    // Table's function(s) --------------------------------------------------------------------------------------

    private IEnumerator PlaceItem(Transform placeArea)
    {
        if (isCooldownHand) yield break;

        isCooldownHand = true;

        Transform itemOnHand = _handSlot.transform.GetChild(0);

        Vector3 placePosition = itemOnHand.CompareTag("dish") ? DISH_TABLEPOSITION : ITEM_TABLEPOSITION;
        itemOnHand.GetComponent<SpriteRenderer>().sortingOrder = 1;

        if (itemOnHand.CompareTag("dish"))
        {
            FindObjectOfType<AudioManager>().Play("Plate");
        }
        else
        {
            FindObjectOfType<AudioManager>().Play("Close-Fridge");
        }

        itemOnHand.SetParent(placeArea.transform);
        itemOnHand.localPosition = placePosition;
        _plateList.GetComponent<RenderInPlate>().ClearPlate();
        ClearHand();

        yield return new WaitForSeconds(1f);
        isCooldownHand = false;
    }

    private IEnumerator GetItem(Transform placeArea)
    {
        if (isCooldownHand) yield break;

        isCooldownHand = true;

        Transform itemOnTable = placeArea.transform.GetChild(0);

        Vector3 placePosition = itemOnTable.CompareTag("dish") ? DISH_HANDPOSITION[0] : ITEM_HANDPOSITION[0];
        itemOnTable.GetComponent<SpriteRenderer>().sortingOrder = 0;

        if (itemOnTable.CompareTag("dish"))
        {
            FindObjectOfType<AudioManager>().Play("Plate");
            List<string> detector = itemOnTable.GetComponent<IngredientDetector>().ingredientList
            .Select(ingredientObj => ingredientObj.ingredientName).ToList();

            // detector.Add(ingredient.name);

            _plateList.GetComponent<RenderInPlate>().RenderPlateUI(detector);
        }
        else
        {
            FindObjectOfType<AudioManager>().Play("Close-Fridge");
        }

        itemOnTable.SetParent(_handSlot.transform);
        itemOnTable.localPosition = placePosition;



        ClearTable(placeArea);

        yield return new WaitForSeconds(1f);
        isCooldownHand = false;
    }

    private void AddIngredientToDish(Transform dish, Transform ingredient)
    {
        IngredientObject ingredientObj = ingredient.GetComponent<IngredientState>().ingredientObjList
            .FirstOrDefault(obj => obj.ingredientName == ingredient.name);

        dish.GetComponent<IngredientDetector>().ingredientList.Add(ingredientObj);

        List<string> detector = dish.GetComponent<IngredientDetector>().ingredientList
            .Select(ingredientObj => ingredientObj.ingredientName).ToList();

        // detector.Add(ingredient.name);

        _plateList.GetComponent<RenderInPlate>().RenderPlateUI(detector);

        StartCoroutine(DelayAction(0.5f));
    }

    // Clear Functions
    private void ClearHand()
    {
        if (_handSlot.transform.childCount == 0)
        {
            // Debug.Log("No items to clear!");
            return;
        }

        while (_handSlot.transform.childCount > 0)
        {
            Transform child = _handSlot.transform.GetChild(0);
            child.SetParent(null);
            Destroy(child.gameObject);
        }
    }

    private void ClearTable(Transform place_area)
    {
        if (place_area.childCount == 0)
        {
            // Debug.Log("No items to clear!");
            return;
        }

        while (place_area.childCount > 0)
        {
            Transform child = place_area.GetChild(0);
            child.SetParent(null);
            Destroy(child.gameObject);
        }
    }

    private void ClearInDish(GameObject dish)
    {
        if (dish.transform.childCount == 0)
        {
            return;
        }

        dish.GetComponent<IngredientDetector>().ingredientList.Clear();

        Transform child = dish.transform.GetChild(0);
        child.SetParent(null);
        Destroy(child.gameObject);
    }

    // ----------------------------------------------------------------------------------------------------------
    #endregion

    #region Cooking Station's function(s)
    // Cooking Station's function(s) ----------------------------------------------------------------------------

    private IEnumerator CutFood(GameObject ingredient)
    {
        if (isCooldownHand) yield break;

        isCooldownHand = true;

        GameObject saveTable = currentTable;
        FindObjectOfType<AudioManager>().Play("Cooking");
        // Debug.Log("Cutting Food");
        _cloudEffect.SetActive(true);
        _moveDir = Vector2.zero;
        isMovable = false;
        // _rb.velocity = Vector2.zero;
        ingredient.GetComponent<SpriteRenderer>().enabled = false;


        yield return new WaitForSeconds(3f);

        // _rb.velocity = _moveDir.normalized * _moveSpeed * Time.fixedDeltaTime;
        _cloudEffect.SetActive(false);
        isMovable = true;
        isCooldownHand = false;

        ingredient.name += "_Cut";
        Debug.Log("Cutting Food Completed : " + ingredient.name);
        ingredient.GetComponent<SpriteRenderer>().enabled = true;

        Transform placeArea = saveTable.transform.Find("PlaceArea");
        Vector3 placePosition = ingredient.CompareTag("dish") ? DISH_TABLEPOSITION : ITEM_TABLEPOSITION;

        ingredient.transform.SetParent(placeArea);
        ingredient.transform.localPosition = placePosition;

        _plateList.GetComponent<RenderInPlate>().ClearPlate();
        ClearHand();
    }

    private void BoilFood(GameObject ingredient)
    {
        StationState stationState = currentTable.GetComponent<StationState>();
        if (stationState.CheckState("cooking")) return;

        stationState.SetState("cooking");

        ingredient.GetComponent<SpriteRenderer>().enabled = false;

        ingredient.name += "_Boil";

        stationState.currentIngredient = ingredient;

        Transform placeArea = currentTable.transform.Find("PlaceArea");
        Vector3 placePosition = ingredient.CompareTag("dish") ? DISH_TABLEPOSITION : ITEM_TABLEPOSITION;

        ingredient.transform.SetParent(placeArea);
        ingredient.transform.localPosition = placePosition;

        _plateList.GetComponent<RenderInPlate>().ClearPlate();
        ClearHand();
    }



    private IEnumerator FreezeFood(GameObject ingredient)
    {
        FindObjectOfType<AudioManager>().Play("Open-Fridge");
        ingredient.GetComponent<SpriteRenderer>().enabled = false;
        yield return new WaitForSeconds(1f);


        ingredient.name += "_Freeze";

        Transform placeArea = currentTable.transform.Find("PlaceArea");
        Vector3 placePosition = ingredient.CompareTag("dish") ? DISH_TABLEPOSITION : ITEM_TABLEPOSITION;

        ingredient.transform.SetParent(placeArea);
        ingredient.transform.localPosition = placePosition;
        ingredient.GetComponent<SpriteRenderer>().enabled = false;

        _plateList.GetComponent<RenderInPlate>().ClearPlate();
        ClearHand();
    }

    #region Re-Cooking Station's function(s)
    // Re-Cooking Station's function(s) -------------------------------------------------------------------------
    private IEnumerator ReCutFood(GameObject dish)
    {
        if (isCooldownHand) yield break;

        isCooldownHand = true;

        FindObjectOfType<AudioManager>().Play("Cooking");
        // Debug.Log("Recutting Food");
        _cloudEffect.SetActive(true);
        isMovable = false;

        // _rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(3f);

        isCooldownHand = false;

        _cloudEffect.SetActive(false);
        isMovable = true;

        List<string> detector = dish.GetComponent<IngredientDetector>().ingredientList
            .Select(ingredientObj => ingredientObj.ingredientName).ToList();

        string oldIngredientName = detector[0];
        string newIngredientName = oldIngredientName + "_Cut";

        detector.Remove(oldIngredientName);
        detector.Add(newIngredientName);

        // Debug.Log("Detector After Change: " + string.Join(", ", detector));

        _plateList.GetComponent<RenderInPlate>().RenderPlateUI(detector);

        List<IngredientObject> dishIngredients = dish.GetComponent<IngredientDetector>().ingredientList;
        IngredientObject ingredientObj = dishIngredients.FirstOrDefault(obj => obj.ingredientName.Contains(oldIngredientName));

        ingredientObj.ingredientName = newIngredientName;

        dish.GetComponent<IngredientDetector>().ingredientList.RemoveAll(obj => obj.ingredientName == oldIngredientName);
        dish.GetComponent<IngredientDetector>().ingredientList.Add(ingredientObj);


        // Transform placeArea = currentTable.transform.Find("PlaceArea");
        // Vector3 placePosition = ITEM_TABLEPOSITION;

        // GameObject collectedIngredient = _items.Find(item => item.name == newIngredientName);
        // GameObject ingredient = Instantiate(collectedIngredient, _handSlot.transform.position, Quaternion.identity, placeArea.transform);
        // ingredient.transform.localPosition = placePosition;

        // ClearTable(placeArea);

    }

    private void ReBoilFood(GameObject dish)
    {
        StationState stationState = currentTable.GetComponent<StationState>();
        if (stationState.CheckState("cooking")) return;

        stationState.SetState("cooking");

        string oldIngredientName = dish.GetComponent<IngredientDetector>().ingredientList.ToList()[0].ingredientName;
        string newIngredientName = oldIngredientName + "_Boil";

        string originalIngredientName = oldIngredientName.Replace("_Cut", "").Replace("_Boil", "").Replace("_Freeze", "");

        GameObject originalIngredient = _items.Find(item => item.name == originalIngredientName);

        Transform placeArea = currentTable.transform.Find("PlaceArea");

        GameObject instantiatedItem = Instantiate(originalIngredient, _handSlot.transform.position, Quaternion.identity, placeArea.transform);
        instantiatedItem.GetComponent<SpriteRenderer>().enabled = false;

        Vector3 placePosition = instantiatedItem.CompareTag("dish") ? DISH_TABLEPOSITION : ITEM_TABLEPOSITION;
        instantiatedItem.transform.localPosition = placePosition;

        instantiatedItem.name = newIngredientName;

        stationState.currentIngredient = instantiatedItem;

        ClearInDish(dish);
    }

    private IEnumerator ReFreezeFood(GameObject dish)
    {
        // Debug.Log("Refreezing Food");
        isMovable = false;

        yield return new WaitForSeconds(1f);

        isMovable = true;

        List<string> detector = dish.GetComponent<IngredientDetector>().ingredientList
            .Select(ingredientObj => ingredientObj.ingredientName)
            .ToList();

        string oldIngredientName = detector[0];
        string newIngredientName = oldIngredientName + "_Freeze";

        detector.Remove(oldIngredientName);
        detector.Add(newIngredientName);

        _plateList.GetComponent<RenderInPlate>().RenderPlateUI(detector);

        List<IngredientObject> dishIngredients = dish.GetComponent<IngredientDetector>().ingredientList;
        IngredientObject ingredientObj = dishIngredients.FirstOrDefault(obj => obj.ingredientName.Contains(detector[0]));

        dishIngredients.RemoveAll(obj => obj.ingredientName == oldIngredientName);
        dishIngredients.Add(ingredientObj);

        Transform placeArea = currentTable.transform.Find("PlaceArea");
        Vector3 placePosition = ITEM_TABLEPOSITION;

        GameObject collectedIngredient = _items.Find(item => item.name == newIngredientName);
        GameObject ingredient = Instantiate(collectedIngredient, _handSlot.transform.position, Quaternion.identity, placeArea.transform);
        ingredient.transform.localPosition = placePosition;

        ClearTable(placeArea);

    }
    #endregion

    #region Get Food's function(s)
    private IEnumerator GetFood(GameObject ingredient, GameObject dish)
    {
        if (isCooldownHand) yield break;

        isCooldownHand = true;

        isMovable = false;

        Debug.Log("Ingredient: " + ingredient.name);

        if (currentTable.CompareTag("boil_station"))
        {
            Debug.Log("Boil Station Detected");
            StationState boilStationState = currentTable.GetComponent<StationState>();
            if (!boilStationState.CheckState("finish"))
            {
                isMovable = true;
                yield break;
            }
            else
            {
                boilStationState.SetState("empty");
            }
        }

        yield return new WaitForSeconds(0.5f);

        isCooldownHand = false;

        isMovable = true;

        if (ingredient.name.Contains("Waste"))
        {
            dish.GetComponent<IngredientDetector>().SpawnWaste();
        }
        else
        {
            IngredientObject ingredientObj = ingredient.GetComponent<IngredientState>().ingredientObjList
            .FirstOrDefault(obj => obj.ingredientName == ingredient.name);

            dish.GetComponent<IngredientDetector>().ingredientList.Add(ingredientObj);

            List<string> detector = dish.GetComponent<IngredientDetector>().ingredientList
                .Select(ingredientObj => ingredientObj.ingredientName).ToList();

            _plateList.GetComponent<RenderInPlate>().RenderPlateUI(detector);
        }

        Transform placeArea = currentTable.transform.Find("PlaceArea");
        ClearTable(placeArea);


    }
    #endregion

    // ----------------------------------------------------------------------------------------------------------
    #endregion

    #region Delicery Food
    private void DeliveryFood()
    {
        // Debug.Log("Yes Detected");
        if (_tableDetector._currentTable == null)
            return;

        currentTable = _tableDetector._currentTable;
        // Debug.Log(currentTable.name);

        if (currentTable == null)
            return;

        if (currentTable.CompareTag("delivery"))
        {
            // Debug.Log("Yes Detected 2");

            Transform dishTransform = _handSlot.transform.Find(dishName);

            if (dishTransform == null)
            {
                Debug.LogError("No dish in hand");
                return;
            }

            // Check if dish has content
            if (dishTransform.childCount == 0)
            {
                Debug.LogError("Dish is empty");
                return;
            }

            string foodInDishName = dishTransform.GetChild(0).name.Replace("(Clone)", "").Trim();

            Debug.Log("Food in dish: " + foodInDishName);

            RenderOrder renderOrder = _orderList.GetComponent<RenderOrder>();

            List<GameObject> foodOrderList = _orderList.GetComponent<RenderOrder>().foodOrderList;



            bool isOrderFound = foodOrderList
                .Any(order => order.transform.GetComponent<OrderEditor>().OrderName == foodInDishName);

            if (!isOrderFound)
            {
                Debug.LogError("No matching order found for the dish");
                return;
            }
            else
            {
                GameObject orderObj = foodOrderList.FirstOrDefault(order => order.transform.GetComponent<OrderEditor>().OrderName == foodInDishName);

                FoodObject order = orderObj.GetComponent<OrderEditor>().foodObject;

                FindObjectOfType<AudioManager>().Play("Cash-Out");

                GameplayManager.Instance.AddMoney(order.foodPrice);

                if (RenderOrder.Instance != null)
                {
                    RenderOrder.Instance.RemoveOrder(orderObj);
                }
                else
                {
                    Debug.LogError("RenderOrder.Instance is null!");
                    Destroy(orderObj);
                }

                Destroy(dishTransform.gameObject);
            }

        }
    }

    #endregion


    #region Animation Logic
    private void CalculateFacingDetection()
    {
        _facingDirection = _moveDir switch
        {
            var dir when dir.x > 0 && dir.y == 0 => Directions.Right,
            var dir when dir.x < 0 && dir.y == 0 => Directions.Left,
            var dir when dir.x == 0 && dir.y > 0 => Directions.Up,
            var dir when dir.x == 0 && dir.y < 0 => Directions.Down,
            var dir when dir.x > 0 && dir.y > 0 => Directions.Up_Right,
            var dir when dir.x < 0 && dir.y > 0 => Directions.Up_Left,
            var dir when dir.x > 0 && dir.y < 0 => Directions.Down_Right,
            var dir when dir.x < 0 && dir.y < 0 => Directions.Down_Left,
            _ => Directions.NONE,
        };

    }

    private void UpdateAnimation()
    {

        if (_facingDirection == Directions.Left || _facingDirection == Directions.Up_Left || _facingDirection == Directions.Down_Left)
        {
            _spriteRenderer.flipX = true;
        }
        else
        {
            _spriteRenderer.flipX = false;
        }

        switch (_facingDirection)
        {
            case Directions.Up:
                _animator.Play("Walk_Up");
                break;
            case Directions.Down:
                _animator.Play("Walk_Down");
                break;
            case Directions.Left:
                _animator.Play("Walk_Right");
                break;
            case Directions.Right:
                _animator.Play("Walk_Right");
                break;
            case Directions.Up_Left:
                _animator.Play("Walk_Up_Right");
                break;
            case Directions.Up_Right:
                _animator.Play("Walk_Up_Right");
                break;
            case Directions.Down_Left:
                _animator.Play("Walk_Down_Right");
                break;
            case Directions.Down_Right:
                _animator.Play("Walk_Down_Right");
                break;
            default:
                _animator.Play("Idle");
                break;
        }
    }
    #endregion


    #region Delay Open UI
    private IEnumerator DelayOpenRecipe(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        GameplayUIManager.Instance._recipeManager.OpenRecipePanel();
        OnOpenRecipe = null;
    }

    private IEnumerator DelayOpenSetting(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        GameplayUIManager.Instance.OpenSettingPanel();
        OnOpenSetting = null;
    }

    #endregion

}
