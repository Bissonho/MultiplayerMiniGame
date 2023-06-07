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

            Debug.LogWarning("OnNetworkSpawn");
        }

        [ServerRpc(RequireOwnership = false)]
        public void AddPlayerServerRpc(ulong OwnerClientId,ulong playerId, string name)
        {
            if (!IsHost)
                return;

            if (!playerData.ContainsKey(playerId))
                playerData.Add(OwnerClientId, new PlayerData(name, playerId, 0));
            else
                playerData[playerId] = new PlayerData(name, playerId, 0);
            Debug.Log("AddPlayer" + "---" + playerId + "----" + name);
        }


        [ServerRpc]
        public void UpdateScoreServerRpc(ulong id, int delta)
        {
            if (!IsHost)
                return;

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

            printAllPlayersConnectedServerRpc();
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


        // Recupera os dados de um jogador, passando-o para o retorno de chamada onGet
        public void GetPlayerData(ulong targetId, Action<PlayerData> onGet)
        {
            m_onGetCurrentCallback = onGet;
            GetPlayerData_ServerRpc(targetId, m_localId);
        }

        [ServerRpc(RequireOwnership = false)]
        void GetPlayerData_ServerRpc(ulong id, ulong callerId)
        {
            if (playerData.ContainsKey(id))
                GetPlayerData_ClientRpc(callerId, playerData[id]);
            else
                GetPlayerData_ClientRpc(callerId, new PlayerData(null, 0));
        }

        [ClientRpc]
        public void GetPlayerData_ClientRpc(ulong callerId, PlayerData data)
        {
            if (callerId == m_localId)
            {   m_onGetCurrentCallback?.Invoke(data);
                m_onGetCurrentCallback = null;
            }
        }
    }
}