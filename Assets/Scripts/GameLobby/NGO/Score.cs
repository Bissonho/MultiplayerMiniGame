using UnityEngine;
using Unity.Netcode;

using System.Collections.Generic;
using TMPro;
using System.Text;
using System.Collections;

namespace LobbyRelaySample.ngo
{
    public class Score : NetworkBehaviour
    {
        Dictionary<ulong, PlayerData> playerData = new Dictionary<ulong, PlayerData>();

        public TMP_Text _scoreText;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            Debug.LogWarning("OnNetworkSpawn");
        }

        [ServerRpc]
        public void AddPlayerServerRpc(ulong id, string name)
        {
            Debug.Log("AddPlayer" + "---" + id + "----" + name);
            if (!IsHost)
                return;

            if (!playerData.ContainsKey(id))
                playerData.Add(id, new PlayerData(name, id, 0));
            else
                playerData[id] = new PlayerData(name, id, 0);
        }


        public void UpdateScore(ulong id, int delta)
        {
            PrintPlayerData();
            if (IsHost)
                UpdateScoreServerRpc(id, delta);
        }


        [ServerRpc]
        public void UpdateScoreServerRpc(ulong id, int delta)
        {
            if (!IsHost)
                return;

            if (playerData.TryGetValue(id, out var data))
            {
                data.score += delta;
            }
            Debug.Log("UpdateScoreOutput_ClientRpc" + "---" + id + "----" + playerData[id].score);
            UpdateScoreClientRpc(id, playerData[id].score);
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


        void PrintPlayerData()
        {

            StringBuilder data = new StringBuilder();

            foreach (var playerData in playerData)
            {
                string playerId = playerData.Value.id.ToString();
                string playerScore = playerData.Value.score.ToString();
                data.AppendLine(playerId + ": " + playerScore);
            }

            Debug.Log(data.ToString());
        }
    }
}
