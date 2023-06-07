using LobbyRelaySample.ngo;
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
            GameRunnerManager.Instance.scoreData.UpdateScoreServerRpc(NetworkManager.Singleton.LocalClientId, 10);
        }

        RemoveCoinServerRpc();
    }
}

