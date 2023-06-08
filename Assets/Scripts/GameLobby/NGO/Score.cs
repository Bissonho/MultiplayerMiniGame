using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using TMPro;
using System;

namespace LobbyRelaySample.ngo
{
        public class Score : NetworkBehaviourSingleton<Score>
        {
            Dictionary<ulong, PlayerData> playerData = new Dictionary<ulong, PlayerData>();

            public TMP_Text scoreText = null;
            ulong localId;

            public override void OnNetworkSpawn()
            {
                base.OnNetworkSpawn();

                localId = NetworkManager.Singleton.LocalClientId;

                Debug.LogWarning($"OnNetworkSpawn {localId}");
            }

            [ServerRpc(RequireOwnership = false)]
            public void AddPlayerServerRpc(string name, ulong localClientId, ulong playerId)
            {
                if (!playerData.ContainsKey(localClientId))
                    playerData.Add(localClientId, new PlayerData(name, playerId, 0));
                else
                    playerData[localClientId] = new PlayerData(name, playerId, 0);

                Debug.Log($"AddPlayer {localClientId}--- {playerId} --- {name}");
            }

            [ServerRpc(RequireOwnership = false)]
            public void UpdateScoreServerRpc(ulong id, int delta)
            {
                if (playerData.TryGetValue(id, out var data))
                {
                    data.score += delta;

                    Debug.Log($"Serve RPC UpdateScoreOutput_ClientRpc --- {id} --- {data.score}");
                    UpdateScoreClientRpc(id, data.score);
                }
                else
                {
                    Debug.LogError($"Player not found in dictionary: {id}");
                }
            }

            [ClientRpc]
            void UpdateScoreClientRpc(ulong id, int score)
            {
                if (NetworkManager.Singleton.LocalClientId == id)
                {
                    Debug.Log($"Client RPC UpdateScoreOutput_ClientRpc --- {id} --- {score}");
                    scoreText.text = score.ToString("00");
                }
            }

            [ServerRpc(RequireOwnership = false)]
            public void PrintPlayerDataServerRpc()
            {
                foreach (var playerData in playerData)
                {
                    Debug.Log($" Player Store Current {playerData.Value.id}: {playerData.Value.score}");
                }
            }
        }
}
