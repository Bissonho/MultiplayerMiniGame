using UnityEngine;
using Unity.Netcode;

using System.Collections.Generic;
using TMPro;
using System.Text;
using System.Collections;
using System;

namespace LobbyRelaySample.ngo
{
    public class Score : NetworkBehaviourSingleton<Score>
    {
        Dictionary<ulong, PlayerData> playerData = new Dictionary<ulong, PlayerData>();
        Action<PlayerData> m_onGetCurrentCallback;

        public TMP_Text _scoreText;
        ulong m_localId;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            m_localId = NetworkManager.Singleton.LocalClientId;

            Debug.LogWarning("OnNetworkSpawn" + m_localId.ToString());
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddPlayerServerRpc(ulong OwnerClientId,ulong playerId, string name)
        {
            if (!IsHost)
                return;

            if (!playerData.ContainsKey(OwnerClientId))
                playerData.Add(OwnerClientId, new PlayerData(name, playerId, 0));
            else
                playerData[OwnerClientId] = new PlayerData(name, playerId, 0);
            Debug.Log("AddPlayer" + "---" + playerId + "----" + name);
        }


        [ServerRpc]
        public void UpdateScoreServerRpc(ulong id, int delta)
        {
            if (playerData.TryGetValue(id, out var data))
            {
                data.score += delta;
                Debug.Log("UpdateScoreOutput_ClientRpc" + "---" + id + "----" + data.score);
                UpdateScoreClientRpc(id, data.score);
            }
            else
            {
                Debug.LogError("Player not found in dictionary: " + id);
            }
        }

        [ClientRpc]
        void UpdateScoreClientRpc(ulong id, int score)
        {
            if (_scoreText != null && NetworkManager.Singleton.LocalClientId == id)
            {
                Debug.Log("UpdateScoreOutput_ClientRpc" + "---" + id + "----" + score);
                _scoreText.text = score.ToString("00");
            }

        }


        [ServerRpc]
        void printAllPlayersConnectedServerRpc()
        {
            foreach (var playerData in playerData)
            {
                Debug.Log(playerData.Value.id.ToString() + ": " + playerData.Value.score.ToString());
            }
        }
    }
}