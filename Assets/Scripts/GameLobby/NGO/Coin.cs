using Unity.Netcode;
using UnityEngine;

public class Coin : NetworkBehaviour
{
    public GameObject prefab;

    [ServerRpc(RequireOwnership = false)]
    void RemoveCoinServerRpc()
    {
        NetworkObject.Despawn();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            //var playerScore = other.gameObject.GetComponent<PlayerScore>();
            //playerScore.IncreaseScoreServerRpc(10);
        }

        RemoveCoinServerRpc();
    }
}

