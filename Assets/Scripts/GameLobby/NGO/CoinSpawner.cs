using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CoinSpawner : NetworkBehaviour
{
    public GameObject prefab;

    private const int MaxPrefabCount = 100;

    private const int initialSpawnedQuantity = 10;


    private Coroutine spawnCoroutine;


    private void Start()
    {
        NetworkManager.Singleton.OnServerStarted += SpawnCoinStart;
    }


    private void SpawnCoinStart()
    {
        NetworkManager.Singleton.OnServerStarted -= SpawnCoinStart;
        for (int i = 0; i < initialSpawnedQuantity; i++)
        {
            SpawnCoin();
        }

        spawnCoroutine = StartCoroutine(SpawnOverTime());
    }

    public void stopSpawning()
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
        return new Vector3(Random.Range(-100f, 100f), 0.5f, Random.Range(-10f, 10f));
    }

}