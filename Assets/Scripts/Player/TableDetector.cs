using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class TableData
{
    public GameObject CurrentTable;
    public Transform PlaceArea;
    public Transform ItemOnTable;
    public Transform DishOnTable;
}

public class TableDetector : MonoBehaviour
{
    [Header("Table Detection")]
    public TableData TableData;
    [Space]


    #region Editor Data
    [Header("Editor Data")]
    [SerializeField] private float radius; // 0.25f
    [SerializeField] private float maxDistance; // 0.4f
    [SerializeField] private LayerMask tableLayerMask;
    [SerializeField] List<Vector2> DetectorPos = new();

    [Space]

    #endregion

    [SerializeField] private PlayerController playerController;

    private readonly string dishName = "999Dish";


    private void FixedUpdate()
    {
        ChangeDetectorPosition();
        DetectTable();
        CheckOutDistance();
    }

    private void ChangeDetectorPosition()
    {
        switch (playerController.faceDir)
        {
            case MoveDirection.Down:
                transform.localPosition = DetectorPos[0];
                break;
            case MoveDirection.Up:
                transform.localPosition = DetectorPos[1];
                break;
            case MoveDirection.Left:
                transform.localPosition = DetectorPos[2];
                break;
            case MoveDirection.Right:
                transform.localPosition = DetectorPos[3];
                break;
        }
    }

    private void DetectTable()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, tableLayerMask);
        if (hits.Length > 0)
        {
            float currentDistance = maxDistance;

            foreach (Collider2D hit in hits)
            {
                float newDistance = Vector2.Distance(transform.position, hit.transform.position);
                if (newDistance < currentDistance)
                {
                    currentDistance = newDistance;

                    if (TableData != null && TableData.CurrentTable == hit.gameObject)
                    {
                        CheckOutDistance();
                        return;
                    }

                    var placeArea = hit.gameObject.transform.Find("PlaceArea");
                    if (placeArea == null)
                    {
                        TableData = new TableData()
                        {
                            CurrentTable = hit.gameObject,
                            PlaceArea = null,
                            ItemOnTable = null,
                            DishOnTable = null
                        };
                        continue;
                    }

                    TableData = new TableData()
                    {
                        CurrentTable = hit.gameObject,
                        PlaceArea = placeArea,
                        ItemOnTable = placeArea.Cast<Transform>().FirstOrDefault(obj => obj.name != dishName),
                        DishOnTable = placeArea.Find(dishName)
                    };


                }
            }
        }
    }

    private void CheckOutDistance()
    {
        if (TableData == null) return;
        if (TableData.CurrentTable == null) return;
        
        float distance = Vector2.Distance(transform.position, TableData.CurrentTable.transform.position);

        if (distance > maxDistance)
        {
            TableData = null;
        }
    }

    public TableData GetTableData()
    {
        return TableData;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

}
