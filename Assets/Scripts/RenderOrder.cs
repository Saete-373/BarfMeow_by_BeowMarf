using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderOrder : MonoBehaviour
{
    public GameplayManager gameManager;
    #region public variables
    public List<GameObject> foodOrderList;

    #endregion

    #region Editor Data
    [Header("Dependencies")]
    [SerializeField] private GameObject orderTemplate;

    [Header("Spawn Settings")]
    [SerializeField] private int _maxOrdersOnScreen = 6;

    #endregion

    #region Internal Data

    private readonly System.Random random = new();
    private bool isInitSpawn = false;
    private bool isSpawnable = true;
    private int foodId = 0;

    #endregion

    void Awake()
    {

    }

    public void RemoveOrder(GameObject orderObject)
    {

        if (orderObject != null)
        {
            foodOrderList.Remove(orderObject);
            Destroy(orderObject);
        }
        else
        {
            Debug.LogWarning("Attempted to remove an order that does not exist in the list.");
        }

    }

    private IEnumerator SpawnOrders()
    {
        if (transform.childCount >= _maxOrdersOnScreen) yield break;

        FindObjectOfType<AudioManager>().Play("New-Order");
        isSpawnable = false;
        // Pick a random order
        int randomIndex = random.Next(0, gameManager.foodOrderList.Count);
        FoodObject order = gameManager.foodOrderList[randomIndex];

        float waitTime = GetWaitTimeBasedOnDifficulty(order.level);

        yield return new WaitForSeconds(waitTime);

        foodId++;

        // Spawn the order
        GameObject spawnedOrder = Instantiate(orderTemplate, transform.position, Quaternion.identity, transform);
        spawnedOrder.GetComponent<OrderEditor>().SetFoodObject(order, foodId);

        foodOrderList.Add(spawnedOrder);

        foodOrderList.RemoveAll(order => order == null);

        isSpawnable = true;

    }

    private IEnumerator InitSpawnOrder()
    {
        if (isInitSpawn) yield break;

        isInitSpawn = true;

        int randomIndex = random.Next(0, gameManager.foodOrderList.Count);
        FoodObject order = gameManager.foodOrderList[randomIndex];


        // Wait a few seconds before starting to spawn orders
        yield return new WaitForSeconds(0.5f);

        foodId++;

        GameObject spawnedOrder = Instantiate(orderTemplate, transform.position, Quaternion.identity, transform);
        spawnedOrder.GetComponent<OrderEditor>().SetFoodObject(order, foodId);

        foodOrderList.Add(spawnedOrder);

    }

    private float GetWaitTimeBasedOnDifficulty(string difficulty)
    {
        return difficulty switch
        {
            "Easy" => 5f,
            "Medium" => 10f,
            "Hard" => 15f,
            _ => 5f // Default case
        };
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!isInitSpawn)
        {
            StartCoroutine(InitSpawnOrder());
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check if game has ended
        if (gameManager.CurrentTime <= 0)
        {
            StopAllCoroutines();
            Debug.Log("Time is up! No more orders will be spawned.");
            isSpawnable = false;
        }
        else if (gameManager.CurrentTime > 0 && isSpawnable)
        {
            StartCoroutine(SpawnOrders());
        }
    }
}
