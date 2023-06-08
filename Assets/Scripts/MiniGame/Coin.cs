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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player collided with coin client ID" + NetworkManager.Singleton.LocalClientId);
            Score.Instance.UpdateScoreServerRpc(NetworkManager.Singleton.LocalClientId, 10); // Fixed code
            Score.Instance.PrintPlayerDataServerRpc(); // Fixed code
        }

        RemoveCoinServerRpc();
    }
}

