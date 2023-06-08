using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CoinSpawner : NetworkBehaviour
{
    [SerializeField]
    private GameObject prefab;

    [SerializeField]
    private int MaxPrefabCount = 100;

    [SerializeField]
    private int initialSpawnedQuantity = 10;

    [SerializeField]
    private float minX = -25f;

    [SerializeField]
    private float maxX = 25f;

    [SerializeField]
    private float minY = -25f;

    [SerializeField]
    private float maxY = 25f;

    private Coroutine spawnCoroutine;

    private void Start()
    {
        if (prefab == null)
        {
            Debug.LogError("Coin Spawner Prefab is not attached!");
            return;
        }
        NetworkManager.Singleton.OnServerStarted += SpawnCoinStart;
        NetworkManager.Singleton.OnServerStopped += StopCoinSpawning;
    }


    private void SpawnCoinStart()
    {
        for (int i = 0; i < initialSpawnedQuantity; i++)
        {
            SpawnCoin();
        }

        spawnCoroutine = StartCoroutine(SpawnOverTime());
    }

    private void StopCoinSpawning(bool value)
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
    }

    private IEnumerator SpawnOverTime()
    {
        while (NetworkManager.Singleton.ConnectedClients.Count > 0)
        {
            yield return new WaitForSeconds(2f);
            if (NetworkObjectPool.Singleton.GetCurrentlyPooledCount(prefab) < MaxPrefabCount)
            {
                SpawnCoin();
            }

        }
    }

    private void SpawnCoin()
    {
        NetworkObject obj = NetworkObjectPool.Singleton.GetNetworkObject(prefab, GetRandomPositionOnMap(), Quaternion.identity);
        if (obj == null) return;
        obj.GetComponent<Coin>().prefab = prefab;
        if (!obj.IsSpawned) obj.Spawn(true);
    }

    private Vector3 GetRandomPositionOnMap()
    {
        return new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), 0);
    }

}