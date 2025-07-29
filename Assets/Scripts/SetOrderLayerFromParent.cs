using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetOrderLayerFromParent : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sprite;

    private int currentOrderLayer;

    void Start()
    {
        currentOrderLayer = sprite.sortingOrder;
    }

    void Update()
    {
        if (transform.parent != null)
        {
            int parentOrderLayer = transform.parent.GetComponent<SpriteRenderer>().sortingOrder;
            if (currentOrderLayer != parentOrderLayer)
            {
                currentOrderLayer = parentOrderLayer;
                sprite.sortingOrder = currentOrderLayer;
            }
        }

    }
}
