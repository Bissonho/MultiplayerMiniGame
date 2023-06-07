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
            GameRunnerManager.Instance.scoreData.UpdateScoreServerRpc(other.gameObject.GetComponent<NetworkObject>().OwnerClientId, 10);
            //playerScore.IncreaseScoreServerRpc(10);
        }

        RemoveCoinServerRpc();
    }
}

