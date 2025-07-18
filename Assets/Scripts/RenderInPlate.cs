using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RenderInPlate : MonoBehaviour
{
    #region Scriptable Objects
    public IngredientObjectList ingredientList;


    #endregion

    #region Editor Data

    #endregion

    #region Public Data
    public List<string> targetIngredients;

    #endregion

    #region Instance Data
    [Header("UI Prefabs")]
    [SerializeField] private GameObject _platePrefab;

    #endregion

    #region Internal Data
    private Dictionary<string, Sprite> inPlateIngredients = new();
    private List<string> inPlateIngredientNames = new();


    #endregion

    // Start is called before the first frame update
    void Start()
    {
        inPlateIngredients = ingredientList.data
            .Where(ingredient => ingredient.ingredientName != "Waste")
            .ToDictionary(ingredient => ingredient.ingredientName, ingredient => ingredient.cookedSprite);

        inPlateIngredientNames = inPlateIngredients.Keys.ToList();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void RenderPlateUI(List<string> newIngredients)
    {
        ClearPlate();

        Debug.Log("RenderPlateUI called with ingredients: " + string.Join(", ", newIngredients));
        targetIngredients = newIngredients;

        bool hasInvalidIngredient = newIngredients.Any(newIngredient => !inPlateIngredientNames.Any(inPlateName => inPlateName == newIngredient));

        if (!hasInvalidIngredient)
        {
            foreach (string ingredient in newIngredients)
            {
                GameObject plateUI = Instantiate(_platePrefab, new Vector3(0, 0, 0), Quaternion.identity, transform);
                plateUI.transform.GetChild(0).GetComponent<Image>().sprite = inPlateIngredients[ingredient];
                Debug.Log($"Assigned sprite for ingredient: {inPlateIngredients[ingredient].name}");


            }
        }
        else
        {
            Debug.LogWarning("Some Ingredient not in List");

        }

    }

    public void ClearPlate()
    {
        while (transform.childCount > 0)
        {
            Transform child = transform.GetChild(0);
            child.SetParent(null);
            Destroy(child.gameObject);
        }
    }
}
