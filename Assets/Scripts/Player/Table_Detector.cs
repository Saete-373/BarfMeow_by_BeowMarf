using System.Collections.Generic;
using UnityEngine;

public class Table_Detector : MonoBehaviour
{
    #region Editor Data
    [SerializeField] private CircleCollider2D _collider;
    public List<GameObject> _ditectedTables;
    public GameObject _currentTable;


    #endregion

    #region Internal Data


    #endregion

    private void Awake()
    {
        if (_collider == null)
        {
            _collider = GetComponent<CircleCollider2D>();
        }
        _ditectedTables.Clear();
    }

    private void OnDisable()
    {
        _ditectedTables.Clear();
    }

    private void UpdateNearestTable()
    {
        if (_ditectedTables.Count == 0)
        {
            return;
        }

        GameObject nearestTable = _ditectedTables[0];
        float nearestDistance = Vector3.Distance(transform.position, nearestTable.transform.position);

        for (int i = 1; i < _ditectedTables.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, _ditectedTables[i].transform.position);
            if (distance < nearestDistance)
            {
                nearestTable = _ditectedTables[i];
                nearestDistance = distance;
            }
        }

        if (nearestTable != _currentTable)
        {
            _currentTable = nearestTable;
        }
    }

    void OnTriggerEnter2D(Collider2D table)
    {
        if (_ditectedTables.Contains(table.gameObject))
        {
            return;
        }
        if (table.gameObject.CompareTag("Blocking_Object"))
        {
            return;
        }
        _ditectedTables.Add(table.gameObject);
        UpdateNearestTable();
    }

    void OnTriggerExit2D(Collider2D table)
    {
        if (table.gameObject.CompareTag("Blocking_Object"))
        {
            return;
        }
        _ditectedTables.Remove(table.gameObject);
        UpdateNearestTable();
    }

}
