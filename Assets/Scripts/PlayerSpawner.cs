using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _playerPrefab;
    void Start()
    {
        if (_playerPrefab != null)
        {
            Instantiate(_playerPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Player Prefab is not assigned in the Inspector!");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
