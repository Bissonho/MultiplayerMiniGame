using LobbyRelaySample.ngo;
using Unity.Netcode;
using UnityEngine;

public class Coin : NetworkBehaviour
{
    public GameObject prefab;

    [ServerRpc(RequireOwnership = false)]
    public void RemoveCoinServerRpc()
    {
        NetworkObject.Despawn();
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player collided with coin client ID" + NetworkManager.Singleton.LocalClientId);
            Score.Instance.UpdateScoreServerRpc(NetworkManager.Singleton.LocalClientId, 10); 
        }

        RemoveCoinServerRpc();
    }
}

