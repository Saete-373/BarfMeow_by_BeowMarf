using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngredientMethodUI : MonoBehaviour
{
    [SerializeField] private Image _ingredientImage;
    [SerializeField] private GameObject _methodGO1;
    [SerializeField] private List<GameObject> _methodList1;
    [SerializeField] private GameObject _method2GO;
    [SerializeField] private List<GameObject> _methodList2;

    public void SetIngredientMethodUI(IngredientObject ingredient)
    {

        _ingredientImage.sprite = ingredient.ingredientSprite;

        _ingredientImage.SetNativeSize();

        RectTransform rectTransform = _ingredientImage.GetComponent<RectTransform>();
        Vector2 currentSize = rectTransform.sizeDelta;
        rectTransform.sizeDelta = new Vector2(currentSize.x, currentSize.y);


        string[] ingredientMethods = ingredient.ingredientName.Split('_');

        switch (ingredientMethods.Length)
        {
            case 2:
                _methodGO1.SetActive(true);
                _method2GO.SetActive(false);
                SetMethods(ingredientMethods[1]);
                break;
            case 3:
                _methodGO1.SetActive(true);
                _method2GO.SetActive(true);
                SetMethods(ingredientMethods[1], ingredientMethods[2]);
                break;
            default:
                _methodGO1.SetActive(false);
                _method2GO.SetActive(false);
                break;
        }
    }

    private void SetMethods(string method1 = "", string method2 = "")
    {
        if (method1 != "")
        {
            for (int i = 0; i < _methodList1.Count; i++)
            {
                if (_methodList1[i].name == method1)
                {
                    _methodList1[i].SetActive(true);
                }
                else
                {
                    _methodList1[i].SetActive(false);
                }
            }
        }


        if (method2 != "")
        {
            for (int i = 0; i < _methodList2.Count; i++)
            {
                if (_methodList2[i].name == method2)
                {
                    _methodList2[i].SetActive(true);
                }
                else
                {
                    _methodList2[i].SetActive(false);
                }
            }
        }
    }
}
