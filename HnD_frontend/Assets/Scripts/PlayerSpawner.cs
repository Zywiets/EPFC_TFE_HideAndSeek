using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public List<SpawnPoint> playerSpawnPoints; 
    public GameObject player;

    private void Start()
    {
        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        Instantiate(player);
    }
}
