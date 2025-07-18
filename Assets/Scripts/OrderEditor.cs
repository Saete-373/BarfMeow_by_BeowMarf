using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderEditor : MonoBehaviour
{
    #region Import Data
    [Header("Slider Bar")]
    [SerializeField] OrderTimeBar _orderTimeBar;
    [SerializeField] Image _borderImage;
    [SerializeField] Image _menuImage;
    [SerializeField] TMP_Text _orderNameText;


    #endregion

    #region Internal Data
    private float currentTime;

    private readonly System.Random random = new();

    #endregion

    #region Public Data
    public string OrderName;
    public FoodObject foodObject;
    public int CurrentPrice;
    public List<Sprite> orderFrameVariants;

    #endregion


    void Update()
    {
        // Decrease time continuously
        currentTime -= Time.deltaTime;
        _orderTimeBar.SetOrderTime(currentTime);

        // Check if time is up
        if (currentTime <= 0)
        {
            if (RenderOrder.Instance != null)
            {
                RenderOrder.Instance.RemoveOrder(gameObject);
            }
            else
            {
                Debug.LogError("RenderOrder.Instance is null!");
                Destroy(gameObject);
            }

        }
    }

    public void SetFoodObject(FoodObject food)
    {
        foodObject = food;

        OrderName = foodObject.foodName;
        _orderNameText.text = OrderName;

        CurrentPrice = foodObject.foodPrice;
        currentTime = foodObject.orderMaxTime;

        _orderTimeBar.SetMaxOrderTime(currentTime);
        _orderTimeBar.SetOrderTime(currentTime);

        _menuImage.sprite = foodObject.foodSprite;

        int randomIndex = random.Next(0, orderFrameVariants.Count);
        _borderImage.sprite = orderFrameVariants[randomIndex];
    }
}
