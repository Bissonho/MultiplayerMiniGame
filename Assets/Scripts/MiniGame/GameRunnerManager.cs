using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Leaderboard;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerData : INetworkSerializable
{
    public string name;
    public ulong id;
    public int score;

    public PlayerData() { } // A default constructor is explicitly required for serialization.

    public PlayerData(string name, ulong id, int score = 0)
    {
        this.name = name;
        this.id = id;
        this.score = score;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref id);
        serializer.SerializeValue(ref score);
    }
}

namespace LobbyRelaySample.ngo
{
    public class GameRunnerManager : NetworkBehaviourSingleton<GameRunnerManager>
    {
        public TMP_Text TimerText = null;

        private NetworkVariable<float> m_TimeRemaining = new NetworkVariable<float>(30.0f);
        private int m_expectedPlayerCount; // Used by the host, but we can't call the RPC until the network connection completes.
        private bool? m_canSpawnCoins;
        private float m_timeout = 5;
        private bool m_hasConnected = false;
        private float counter = 0f;
        private bool gameRunning = false;
        private PlayerData m_localUserData; // This has an ID that's not necessarily the OwnerClientId, since all clients will see all spawned objects regardless of ownership.

        public Action onGameBeginning;
        Action m_onConnectionVerified, m_onGameEnd;

        public void Initialize(Action onConnectionVerified, int expectedPlayerCount, Action onGameBegin, Action onGameEnd, LocalPlayer localUser)
        {
            m_onConnectionVerified = onConnectionVerified;
            m_expectedPlayerCount = expectedPlayerCount;
            onGameBeginning = onGameBegin;
            m_onGameEnd = onGameEnd;
            m_canSpawnCoins = false;
            m_localUserData = new PlayerData(localUser.DisplayName.Value, 0);
        }

        public override void OnNetworkSpawn()
        {
            Score.Instance.AddPlayerServerRpc(m_localUserData.name, NetworkManager.Singleton.LocalClientId, GetId(m_localUserData.name));
            if (IsHost)
            {
                InvokeRepeating(nameof(UpdateGameTimer), 0.0f, 1.0f);
            }
        }

        public override void OnNetworkDespawn()
        {
            m_onGameEnd();
        }

        private void Update()
        {
            if (m_timeout >= 0)
            {
                m_timeout -= Time.deltaTime;
                if (m_timeout < 0)
                    BeginGame();
            }
        }

        void BeginGame()
        {
            m_canSpawnCoins = true;
            GameManager.Instance.BeginGame();
            gameRunning = true;
            onGameBeginning?.Invoke();
        }

        private async Task EndGame()
        {
            Dictionary<ulong, PlayerData> playersData = Score.Instance.GetPlayerDataDictionary();
            var players = new List<LeaderboardRepository.Player>();

            foreach (var player in playersData)
            {
                players.Add(new LeaderboardRepository.Player(player.Value.score, player.Value.name));
                Debug.Log("Player: " + player.Value.name + " Score: " + player.Value.score);
            }

            await LeaderBoardManager.Instance.CreateSession(players);

            if (IsHost)
                StartCoroutine(EndGame_ClientsFirst());
        }

        private IEnumerator EndGame_ClientsFirst()
        {
            EndGame_ClientRpc();
            yield return null;
            SendLocalEndGameSignal();
        }

        private void SendLocalEndGameSignal()
        {
            m_onGameEnd();
        }

        [ClientRpc]
        private void EndGame_ClientRpc()
        {
            if (IsHost)
                return;
            SendLocalEndGameSignal();
        }

        private void UpdateGameTimer()
        {
            if (!gameRunning)
                return;

            if (IsHost)
            {
                m_TimeRemaining.Value -= 1.0f;

                if (m_TimeRemaining.Value <= 0.0f)
                {
                    m_TimeRemaining.Value = 0.0f;
                    EndGame();
                }
            }

            TimerText.SetText("{0}", Mathf.CeilToInt(m_TimeRemaining.Value));

            // Sincronizar o valor do tempo restante para os clientes
            if (IsHost)
            {
                RpcSyncTimeRemainingClientRpc(m_TimeRemaining.Value);
            }
        }

        [ClientRpc]
        private void RpcSyncTimeRemainingClientRpc(float timeRemaining)
        {
            TimerText.SetText("{0}", Mathf.CeilToInt(timeRemaining));
        }

        private ulong GetId(string word)
        {
            string id = string.Empty;

            if (word.Length < 3 || word == null) { return 0; }

            for (int i = 0; i < 3; i++)
            {
                if (i >= word.Length)
                {
                    id += "0";
                }
                else
                {
                    int n = (int)word[i];
                    id += n.ToString();
                }
            }

            return ulong.Parse(id);
        }
    }
}