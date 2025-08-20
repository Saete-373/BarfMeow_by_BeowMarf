using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<string> items = new();
    public Transform Dish;
    public Transform IngredientInHand;

    public void AddItem(string item)
    {
        items.Add(item);
        // Debug.Log("Item added: " + item);
    }

    public void RemoveItem(string item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
            // Debug.Log("Item removed: " + item);
        }
        else
        {
            Debug.Log("Item not found in inventory: " + item);
        }
    }

    public void LoadItems()
    {
        if (Dish == null) return;

        var dishRenderer = Dish.GetComponent<RenderInDish>();

        if (dishRenderer == null || dishRenderer.IsSaveItemsNull()) return;

        items = dishRenderer.saveItems;
    }

    public void SetUpDish(Transform dish)
    {
        if (dish == null) return;

        items.Clear();

        Dish = dish;

        if (Dish.TryGetComponent<RenderInDish>(out var dishRenderer))
        {
            if (dishRenderer.saveItems != null && dishRenderer.saveItems.Count > 0)
            {
                items = new List<string>(dishRenderer.saveItems);
            }
        }

    }

    public void SetIngredientInHand(Transform ingredient)
    {
        if (ingredient == null) return;

        IngredientInHand = ingredient;
    }

    public void ClearDish()
    {
        if (Dish == null) return;
        Dish = null;
    }

    public void ClearIngredientInHand()
    {
        if (IngredientInHand == null) return;
        IngredientInHand = null;
    }


    public void ClearInventory()
    {
        items.Clear();
        Debug.Log("Inventory cleared.");
    }

    public void OpenPlateList()
    {
        if (Dish == null) return;

        gameObject.GetComponent<PlayerController>().gameplayManager.renderInPlate.RenderPlateUI(items);
    }

    public void ClosePlateList()
    {
        gameObject.GetComponent<PlayerController>().gameplayManager.renderInPlate.ClearPlate();
    }
}
