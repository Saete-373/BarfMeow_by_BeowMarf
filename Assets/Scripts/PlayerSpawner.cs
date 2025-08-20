using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{

    public GameplayManager gameplayManager;
    void Start()
    {
        gameplayManager.SpawnPlayer();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
