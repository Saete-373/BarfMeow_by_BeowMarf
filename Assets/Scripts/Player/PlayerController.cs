using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MoveDirection
{
    Up,
    Down,
    Left,
    Right,
    NONE
}


public class PlayerController : MonoBehaviour
{
    #region Editor Data
    [Header("Movement Attributes")]
    [SerializeField] private float maxMoveSpeed = 100f;
    [SerializeField] private float moveSpeed = 100f;

    [Header("Dependencies")]
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    public TableDetector tableDetector;
    public Inventory inventory;
    public RenderInDish renderInDish;
    [SerializeField] private GameObject _handSlot;
    [SerializeField] private List<GameObject> _items;
    [SerializeField] private GestureRecognizer _gestureRecognizer;
    [SerializeField] private GameObject _orderList;

    public GameplayManager gameplayManager;

    #endregion

    #region Instance Data


    #endregion

    #region Import Data

    private string CurrentGesture = "none";


    #endregion

    public TableData tableData;


    #region Internal Data
    private Vector2 _moveDir = Vector2.zero;
    public MoveDirection faceDir = MoveDirection.NONE;

    private readonly string dishName = "999Dish";
    private readonly List<Vector3> DISH_HANDPOSITION = new List<Vector3> {
                                                            new(0, -0.254f, 0), // up
                                                            new(0, 0.049f, 0), // down
                                                            new(-0.392f, -0.188f, 0), // Left
                                                            new(0.392f, -0.188f, 0) // Right
                                                        };
    private readonly List<Vector3> ITEM_HANDPOSITION = new List<Vector3> {
                                                            new(0, -0.313f, 0), // up
                                                            new(0, 0, 0), // down
                                                            new(-0.349f, -0.313f, 0), // Left
                                                            new(0.349f, -0.313f, 0) // Right
                                                        };
    private readonly Vector3 DISH_TABLEPOSITION = new(0, -0.056f, 0);
    private readonly Vector3 ITEM_TABLEPOSITION = new(0, -0.17f, 0);
    [SerializeField] private bool isCooldownHand = false;
    private Coroutine OnOpenRecipe;
    private Coroutine OnOpenSetting;
    private Coroutine OnOpenTips;
    private Coroutine OnDestroyEndGame;
    private readonly Dictionary<string, int> moveIndex = new()
    {
        { "down", 0 },
        { "up", 1 },
        { "left", 2 },
        { "right", 3 }
    };
    private readonly Dictionary<string, Vector2> moveDirDict = new()
    {
        { "down", new Vector2(0, -1) },
        { "up", new Vector2(0, 1) },
        { "left", new Vector2(-1, 0) },
        { "right", new Vector2(1, 0) }
    };

    private Vector2 targetMoveDir;
    private Vector2 smoothedMoveDir;

    #endregion

    private void Awake()
    {
        SubsGameManager();
    }

    private void Start()
    {
        ResetMoveSpeed();
    }

    private void SubsGameManager()
    {
        gameplayManager = FindObjectOfType<GameplayManager>();

        if (gameplayManager != null)
        {
            gameplayManager.playerController = this;
        }
        else
        {
            Debug.Log("GameplayManager not found in the scene!");
        }
    }



    #region Tick 
    private void Update()
    {
        CallGetTableData();
        // CallGetInDish();
        CheckEndGame();

        if (!gameplayManager.IsPaused())
        {
            RecognizeGesture();
        }
    }

    private void FixedUpdate()
    {
        smoothedMoveDir = Vector2.MoveTowards(smoothedMoveDir, _moveDir, 5f * Time.fixedDeltaTime);
        Vector2 move = moveSpeed * Time.fixedDeltaTime * smoothedMoveDir;
        _rb.MovePosition(_rb.position + move);

        CalculateFacingDetection();
        UpdateAnimation();
    }

    #endregion

    private void CheckEndGame()
    {
        if (gameObject == null) return;

        if (gameplayManager.currentGameState == GameState.EndGame)
        {
            if (OnDestroyEndGame == null)
            {
                OnDestroyEndGame = StartCoroutine(DelayDestroyPlayer(0.5f));
            }

            return;
        }
    }

    #region Time Logic

    private IEnumerator DelayCookingAction(float delay)
    {

        yield return new WaitForSeconds(delay);

    }

    private IEnumerator DelayDestroyPlayer(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnDestroyEndGame = null;
        Destroy(gameObject);
    }

    #endregion

    #region Delay Open UI
    private IEnumerator DelayOpenRecipe(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        gameplayManager.uiManager._recipeManager.OpenRecipePanel();
        // gameplayManager.uiManager.TogglePlayerCanvas(false);
        OnOpenRecipe = null;
    }

    private IEnumerator DelayOpenSetting(float delayTime)
    {

        yield return new WaitForSeconds(delayTime);

        gameplayManager.SetPause(true);
        gameplayManager.uiManager.OpenSettingPanel();
        // gameplayManager.uiManager.TogglePlayerCanvas(false);
    }

    private IEnumerator DelayOpenTips(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        gameplayManager.SetPause(true);
        gameplayManager.uiManager.OpenTipsPanel();

    }

    #endregion


    #region Input Logic

    private void RecognizeGesture()
    {
        if (_gestureRecognizer.currentGesture == CurrentGesture)
            return;

        string[] gestures = { "recipe", "setting", "tips" };

        if (gestures.Contains(CurrentGesture) && gestures.Contains(_gestureRecognizer.currentGesture)
            && CurrentGesture != _gestureRecognizer.currentGesture)
        {
            return;
        }

        CurrentGesture = _gestureRecognizer.currentGesture;

        if (string.IsNullOrEmpty(CurrentGesture) || CurrentGesture == "none")
        {
            SetZeroMoveSpeed();
            return;
        }

        if (CurrentGesture != "recipe")
        {
            if (OnOpenRecipe != null)
            {
                StopCoroutine(OnOpenRecipe);
                OnOpenRecipe = null;
            }

            gameplayManager.uiManager._recipeManager.CloseRecipePanel();
        }

        if (CurrentGesture != "setting")
        {
            if (OnOpenSetting != null)
            {
                StopCoroutine(OnOpenSetting);
                OnOpenSetting = null;
            }
        }

        if (CurrentGesture != "tips")
        {
            if (OnOpenTips != null)
            {
                StopCoroutine(OnOpenTips);
                OnOpenTips = null;
            }
        }

        switch (CurrentGesture)
        {
            // Movement - Horizontal
            case "left":
                HandleMove("left");
                break;
            case "right":
                HandleMove("right");
                break;

            // Movement - Vertical
            case "down":
                HandleMove("down");
                break;
            case "up":
                HandleMove("up");
                break;

            // Interaction with tables
            case "grab":
                HandleGrab();
                break;

            // Cooking interactions
            case "cooking":
                HandleCook();
                break;

            // Delivery actions
            case "yes":
                HandleDelivery();
                break;

            case "no":
                break;

            // Stop action
            case "stop":
                HandleStop();
                break;

            // Open Recipe
            case "recipe":
                OnOpenRecipe = StartCoroutine(DelayOpenRecipe(3f));
                break;

            // Open Setting
            case "setting":
                if (!gameplayManager.IsPaused())
                {
                    OnOpenSetting = StartCoroutine(DelayOpenSetting(3f));
                }
                break;

            // Open Tips
            case "tips":
                if (!gameplayManager.IsPaused())
                {
                    OnOpenTips = StartCoroutine(DelayOpenTips(1f));
                }
                break;
        }




    }

    #region Move Interaction
    private void HandleMove(string gestureMoveName)
    {
        if (isCooldownHand) return;
        _moveDir = Vector2.zero;
        _moveDir = moveDirDict[gestureMoveName];
        ResetMoveSpeed();

    }

    private void HandleStop()
    {
        if (_handSlot.transform.childCount > 0)
        {
            Transform itemInHand = _handSlot.transform.GetChild(0);
            if (itemInHand.name.Contains(dishName))
            {
                itemInHand.localPosition = DISH_HANDPOSITION[GetMoveIndexFormFaceDir()];
            }
            else
            {
                itemInHand.localPosition = ITEM_HANDPOSITION[GetMoveIndexFormFaceDir()];
            }
        }

        SetZeroMoveSpeed();
    }

    public void ResetMoveSpeed()
    {
        moveSpeed = maxMoveSpeed;
        _animator.speed = 1f;
    }

    public void SetZeroMoveSpeed()
    {
        moveSpeed = 0f;
        _animator.speed = 0f;
    }

    #endregion

    private void CallGetTableData()
    {
        tableData = tableDetector.GetTableData();
        if (tableData == null) return;
    }

    #region Grab Interaction 
    // Grab Interaction
    private void HandleGrab()
    {
        if (tableData == null || tableData.CurrentTable == null) return;

        switch (tableData.CurrentTable.tag)
        {
            case "table":
                // ห้ามทำซ้ำ
                if ((inventory.Dish != null && tableData.DishOnTable != null) || (inventory.IngredientInHand != null && tableData.ItemOnTable != null))
                    return;

                // หากมือมีของ -> วาง
                if (tableData.PlaceArea.childCount == 0)
                {
                    if (inventory.IngredientInHand == null && inventory.Dish == null) return;
                    StartCoroutine(PlaceItem());
                    return;
                }

                // หากมือว่าง -> หยิบ
                if (inventory.IngredientInHand == null && inventory.Dish == null)
                {
                    if (tableData.PlaceArea.childCount > 0)
                        StartCoroutine(GetItem());
                    return;
                }

                if (inventory.Dish != null && tableData.ItemOnTable != null && tableData.ItemOnTable.CompareTag("ingredient"))
                {
                    AddIngredientToDish(inventory.Dish, tableData.ItemOnTable, false);
                    // Destroy(tableData.ItemOnTable.gameObject);
                    return;
                }

                if (tableData.DishOnTable != null && inventory.IngredientInHand != null && inventory.IngredientInHand.CompareTag("ingredient"))
                {
                    AddIngredientToDish(tableData.DishOnTable, inventory.IngredientInHand, true);
                    // Destroy(inventory.IngredientInHand.gameObject);
                    return;
                }

                break;
            case "collectable":
                string itemName = tableData.CurrentTable.name.Split(" ")[0].Replace("Box", "");
                if (itemName == "Dish")
                {
                    itemName = dishName;
                }
                BlinkToStation();
                GrabItem(itemName);
                break;
            case "bin":
                AudioManager.instance.Play("Trash-Bin");
                BlinkToStation();
                ClearHand();
                break;
        }


    }
    #endregion

    public void SetCooking(bool state)
    {
        isCooldownHand = state;

        if (isCooldownHand)
        {
            SetZeroMoveSpeed();
        }
        else
        {
            ResetMoveSpeed();
        }
    }

    #region Cooking Interaction
    // Cooking Interaction
    private void HandleCook()
    {
        if (tableData == null || tableData.CurrentTable == null || tableData.CurrentTable.CompareTag("table") || tableData.CurrentTable.CompareTag("collectable") || tableData.CurrentTable.CompareTag("bin") || tableData.CurrentTable.CompareTag("delivery")) return;

        StationState stationState = tableData.CurrentTable.GetComponent<StationState>();
        if (stationState.CheckCookState("cooking")) return;

        if (inventory.IngredientInHand != null && inventory.Dish == null)
        {
            if (isCooldownHand) return;

            switch (tableData.CurrentTable.tag)
            {
                case "cut_station":
                    BlinkToStation();
                    StartCoroutine(CutFood(inventory.IngredientInHand));
                    break;
                case "boil_station":
                    BlinkToStation();
                    BoilFood(inventory.IngredientInHand);
                    break;
                case "refrigerator":
                    BlinkToStation();
                    FreezeFood(inventory.IngredientInHand);
                    break;
                default:
                    Debug.Log("Unknown Cooking Type");
                    break;
            }
        }
        else if (inventory.IngredientInHand == null && inventory.Dish != null && inventory.Dish.childCount == 1 && stationState.CheckCookState("empty"))
        {
            if (isCooldownHand) return;

            switch (tableData.CurrentTable.tag)
            {
                case "cut_station":
                    BlinkToStation();
                    StartCoroutine(ReCutFood(inventory.Dish));
                    break;
                case "boil_station":
                    BlinkToStation();
                    ReBoilFood(inventory.Dish);
                    break;
                case "refrigerator":
                    BlinkToStation();
                    ReFreezeFood(inventory.Dish);
                    break;
                default:
                    Debug.Log("Unknown Cooking Type");
                    break;
            }
        }
        else if (inventory.IngredientInHand == null && inventory.Dish != null && stationState.CheckCookState("finish"))
        {
            BlinkToStation();
            StartCoroutine(GetFood(inventory.Dish));
        }
        else
        {

        }
    }
    #endregion

    #endregion

    #region Movement Logic

    private void BlinkToStation()
    {
        Vector3 tablePos = tableData.CurrentTable.transform.position;
        Vector3 currentPos = transform.position;
        Vector3[] positions = new Vector3[]
        {
            tablePos + new Vector3(0, 0.3779929f),    // straightper
            tablePos + new Vector3(0, -0.45938f),   // Lower
            tablePos + new Vector3(-0.6908719f, -0.1111792f), // Left
            tablePos + new Vector3(0.6908719f, 0.1111792f)   // Right
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
            return;
        }

        transform.position = closestPosition;
    }
    #endregion

    #region Stock Box's function(s)

    // Stock Box's function(s) ----------------------------------------------------------------------------------
    private void GrabItem(string current_item)
    {
        // Find the item to collect
        GameObject collectedItem = _items.Find(item => item.name == current_item);

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
            AudioManager.instance.Play("Plate");
            inventory.SetUpDish(instantiatedItem.transform);
        }
        else
        {
            AudioManager.instance.Play("Close-Fridge");
            inventory.SetIngredientInHand(instantiatedItem.transform);
        }

        // Set position
        Vector3 spawnPosition = current_item == dishName ? DISH_HANDPOSITION[GetMoveIndexFormFaceDir()] : ITEM_HANDPOSITION[GetMoveIndexFormFaceDir()];
        instantiatedItem.transform.localPosition = spawnPosition;

    }

    // ----------------------------------------------------------------------------------------------------------
    #endregion

    #region Table's function(s)
    // Table's function(s) --------------------------------------------------------------------------------------

    private IEnumerator PlaceItem()
    {
        if (isCooldownHand) yield break;

        isCooldownHand = true;

        Transform itemOnHand = _handSlot.transform.GetChild(0);

        itemOnHand.GetComponent<SpriteRenderer>().sortingOrder = 1;

        Vector3 placePosition;
        if (itemOnHand.CompareTag("dish"))
        {
            AudioManager.instance.Play("Plate");
            inventory.ClearDish();
            placePosition = DISH_TABLEPOSITION;
            tableData.DishOnTable = itemOnHand;
        }
        else
        {
            AudioManager.instance.Play("Close-Fridge");
            inventory.ClearIngredientInHand();
            inventory.ClearIngredientInHand();
            placePosition = ITEM_TABLEPOSITION;
            tableData.ItemOnTable = itemOnHand;
        }

        inventory.ClosePlateList();

        itemOnHand.SetParent(tableData.PlaceArea);
        itemOnHand.localPosition = placePosition;



        ClearHand();

        yield return new WaitForSeconds(1f);
        isCooldownHand = false;
    }

    private IEnumerator GetItem()
    {
        if (isCooldownHand) yield break;

        isCooldownHand = true;

        Transform itemOnTable = tableData.PlaceArea.GetChild(0);

        Vector3 placePosition = itemOnTable.CompareTag("dish") ? DISH_HANDPOSITION[GetMoveIndexFormFaceDir()] : ITEM_HANDPOSITION[GetMoveIndexFormFaceDir()];
        itemOnTable.GetComponent<SpriteRenderer>().sortingOrder = 0;

        if (itemOnTable.CompareTag("dish"))
        {
            AudioManager.instance.Play("Plate");
            inventory.SetUpDish(itemOnTable);
            tableData.DishOnTable = null;
            inventory.Dish = itemOnTable;
        }
        else
        {
            AudioManager.instance.Play("Close-Fridge");
            inventory.SetIngredientInHand(itemOnTable);
            tableData.ItemOnTable = null;
            inventory.IngredientInHand = itemOnTable;
        }

        itemOnTable.SetParent(_handSlot.transform);
        itemOnTable.localPosition = placePosition;

        ClearTable(tableData.PlaceArea);

        yield return new WaitForSeconds(1f);
        isCooldownHand = false;
    }

    private void AddIngredientToDish(Transform dish = null, Transform ingredient = null, bool handToTable = true)
    {
        bool isRawIngredient = StageManager.Instance.ingredientNames.Contains(ingredient.name);

        bool isCookedIngredient = ingredient.GetComponent<IngredientState>().cookedNames.Count > 0 && ingredient.GetComponent<IngredientState>().cookedNames.Contains(ingredient.name);

        string newIngredient = "";

        if (isRawIngredient)
        {
            newIngredient = ingredient.name;
        }
        else if (isCookedIngredient)
        {
            newIngredient = ingredient.GetComponent<IngredientState>().cookedNames.FirstOrDefault(name => name == ingredient.name);
        }

        if (!isRawIngredient && !isCookedIngredient)
        {
            newIngredient = "Waste";
        }

        if (newIngredient == null) return;

        inventory.AddItem(newIngredient);

        if (dish == null)
        {
            dish = tableData.PlaceArea.Find(dishName);
        }
        dish.GetComponent<RenderInDish>().RenderDish(this);

        if (handToTable)
        {
            inventory.ClearDish();
            ClearHand();

            inventory.ClosePlateList();
        }
        else
        {
            tableData.ItemOnTable = null;
            ClearTable(tableData.PlaceArea);

            inventory.OpenPlateList();
        }

    }

    // Clear Functions
    private void ClearHand()
    {

        if (_handSlot.transform.childCount == 0)
        {
            return;
        }

        inventory.ClearIngredientInHand();
        inventory.Dish = null;

        while (_handSlot.transform.childCount > 0)
        {
            Transform child = _handSlot.transform.GetChild(0);
            child.SetParent(null);
            Destroy(child.gameObject);
        }

        inventory.ClosePlateList();
    }

    private void ClearTable(Transform place_area)
    {
        if (place_area.childCount == 0)
        {
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

        Transform child = dish.transform.GetChild(0);
        child.SetParent(null);
        Destroy(child.gameObject);
    }

    // ----------------------------------------------------------------------------------------------------------
    #endregion

    #region Cooking Station's function(s)
    // Cooking Station's function(s) ----------------------------------------------------------------------------



    private IEnumerator CutFood(Transform ingredient)
    {
        AudioManager.instance.Play("Cooking");

        SetCooking(true);

        tableData.ItemOnTable = ingredient;
        inventory.ClearIngredientInHand();

        Vector3 placePosition;

        placePosition = ITEM_TABLEPOSITION;
        ingredient.SetParent(tableData.PlaceArea);
        ingredient.localPosition = placePosition;
        inventory.ClosePlateList();

        ingredient.GetComponent<SpriteRenderer>().sortingOrder = 1;

        StationState stationState = tableData.CurrentTable.GetComponent<StationState>();
        stationState.CallCook(ingredient.name, this);


        yield return new WaitForSeconds(stationState.cookingTime);


        stationState.UpdateIngredientName();

        SetCooking(false);

        ClearHand();
    }

    private void BoilFood(Transform ingredient)
    {
        tableData.ItemOnTable = ingredient;
        inventory.ClearIngredientInHand();

        StationState stationState = tableData.CurrentTable.GetComponent<StationState>();
        stationState.CallCook(ingredient.name, this);

        inventory.ClosePlateList();
        ClearHand();
    }



    private void FreezeFood(Transform ingredient)
    {
        AudioManager.instance.Play("Open-Fridge");
        tableData.CurrentTable.GetComponent<StationState>().CallCook(ingredient.name, this);

        inventory.ClearIngredientInHand();
        tableData.ItemOnTable = ingredient;

        inventory.ClosePlateList();
        ClearHand();
    }

    #region Re-Cooking Station's function(s)
    // Re-Cooking Station's function(s) -------------------------------------------------------------------------
    private IEnumerator ReCutFood(Transform dish)
    {
        AudioManager.instance.Play("Cooking");

        SetCooking(true);

        inventory.ClosePlateList();

        StationState stationState = tableData.CurrentTable.GetComponent<StationState>();
        stationState.CallCook(inventory.items[0], this);

        ClearInDish(dish.gameObject);


        yield return new WaitForSeconds(stationState.cookingTime);

        inventory.items[0] = stationState.GetCurrentIngredient();
        dish.GetComponent<RenderInDish>().RenderDish(this);

        inventory.OpenPlateList();

        SetCooking(false);
    }

    private void ReBoilFood(Transform dish)
    {
        inventory.ClosePlateList();

        StationState stationState = tableData.CurrentTable.GetComponent<StationState>();
        stationState.CallCook(inventory.items[0], this);
        inventory.items.RemoveAt(0);

        ClearInDish(dish.gameObject);
    }

    private void ReFreezeFood(Transform dish)
    {
        inventory.ClosePlateList();

        StationState stationState = tableData.CurrentTable.GetComponent<StationState>();
        stationState.CallCook(inventory.items[0], this);
        inventory.items.RemoveAt(0);

        ClearInDish(dish.gameObject);
    }
    #endregion

    #region Get Food's function(s)
    private IEnumerator GetFood(Transform dish)
    {
        if (dish.childCount > 3) yield break;

        if (isCooldownHand) yield break;

        isCooldownHand = true;

        SetZeroMoveSpeed();

        string newIngredientName = tableData.CurrentTable.GetComponent<StationState>().GetCurrentIngredient();

        inventory.AddItem(newIngredientName);

        dish.GetComponent<RenderInDish>().RenderDish(this);

        inventory.OpenPlateList();

        yield return new WaitForSeconds(0.5f);

        isCooldownHand = false;

        ResetMoveSpeed();

        ClearTable(tableData.PlaceArea);


    }
    #endregion

    // ----------------------------------------------------------------------------------------------------------
    #endregion

    #region Delicery Food
    private void HandleDelivery()
    {
        if (tableData.CurrentTable.CompareTag("delivery"))
        {
            if (inventory.Dish == null)
            {
                Debug.LogError("No dish in hand");
                return;
            }

            // Check if dish has content
            if (inventory.Dish.childCount == 0)
            {
                Debug.LogError("Dish is empty");
                return;
            }


            FoodObject foodInDish = inventory.Dish.GetComponent<RenderInDish>().CurrentFood;

            string foodInDishName = foodInDish.foodName;

            if (foodInDish == null)
            {
                Debug.LogError("No food in dish");
                return;
            }


            List<GameObject> foodOrderList = gameplayManager.renderOrder.foodOrderList;

            foreach (GameObject orderObj in foodOrderList)
            {
                OrderEditor order = orderObj.GetComponent<OrderEditor>();

                if (order.OrderName == foodInDishName)
                {
                    AudioManager.instance.Play("Cash-Out");

                    gameplayManager.AddMoney(order.foodObject.price);

                    if (gameplayManager.renderOrder != null)
                    {
                        gameplayManager.renderOrder.RemoveOrder(orderObj);
                    }
                    else
                    {
                        Debug.LogError("RenderOrder.Instance is null!");
                        Destroy(orderObj);
                    }

                    Destroy(inventory.Dish.gameObject);
                    inventory.ClearDish();

                    inventory.ClearInventory();

                    break;
                }
            }
        }
    }

    #endregion


    #region Animation Logic
    private void CalculateFacingDetection()
    {
        faceDir = _moveDir switch
        {
            var dir when dir.x > 0 && dir.y == 0 => MoveDirection.Right,
            var dir when dir.x < 0 && dir.y == 0 => MoveDirection.Left,
            var dir when dir.x == 0 && dir.y < 0 => MoveDirection.Down,
            var dir when dir.x == 0 && dir.y > 0 => MoveDirection.Up,
            _ => MoveDirection.NONE,
        };
    }

    private int GetMoveIndexFormFaceDir()
    {
        return faceDir switch
        {
            MoveDirection.Down => 0,
            MoveDirection.Up => 1,
            MoveDirection.Left => 2,
            MoveDirection.Right => 3,
            _ => 0,
        };
    }

    private void RotateItemInHand(string gestureMoveName)
    {
        if (_handSlot.transform.childCount > 0)
        {
            Transform itemInHand = _handSlot.transform.GetChild(0);
            if (itemInHand.name.Contains(dishName))
            {
                itemInHand.localPosition = DISH_HANDPOSITION[GetMoveIndexFormFaceDir()];
            }
            else
            {
                itemInHand.localPosition = ITEM_HANDPOSITION[GetMoveIndexFormFaceDir()];
            }
        }
    }

    private void UpdateAnimation()
    {

        if (faceDir == MoveDirection.Left)
        {
            _spriteRenderer.flipX = true;
        }
        else
        {
            _spriteRenderer.flipX = false;
        }

        switch (faceDir)
        {
            case MoveDirection.Down:
                _animator.Play("Walk_Down");
                RotateItemInHand("down");
                break;
            case MoveDirection.Up:
                _animator.Play("Walk_Up");
                RotateItemInHand("up");
                break;
            case MoveDirection.Left:
                _animator.Play("Walk_Right");
                RotateItemInHand("left");
                break;
            case MoveDirection.Right:
                _animator.Play("Walk_Right");
                RotateItemInHand("right");
                break;
            default:
                _animator.Play("Idle");
                break;
        }
    }
    #endregion




}
