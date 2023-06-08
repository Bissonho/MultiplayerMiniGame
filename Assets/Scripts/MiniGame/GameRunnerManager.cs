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
    public PlayerData(string name, ulong id, int score = 0) { this.name = name; this.id = id; this.score = score; }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref id);
        serializer.SerializeValue(ref score);
    }
}


namespace LobbyRelaySample.ngo
{
    /// <summary>
    /// Once the NetworkManager has been spawned, we need something to manage the game state and setup other in-game objects
    /// that is itself a networked object, to track things like network connect events.
    /// </summary>
    public class GameRunnerManager : NetworkBehaviourSingleton<GameRunnerManager>
    {

        public TMP_Text TimerText = null;

        private NetworkVariable<float> m_TimeRemaining = new NetworkVariable<float>(30.0f);



        public Action onGameBeginning;
        Action m_onConnectionVerified, m_onGameEnd;
        private int m_expectedPlayerCount; // Used by the host, but we can't call the RPC until the network connection completes.
        private bool? m_canSpawnCoins;

        private float m_timeout = 5;
        private bool m_hasConnected = false;

        private float counter = 0f;
        private bool gameRunning = false;

        //[SerializeField]
        //private SymbolContainer m_symbolContainerInstance;
        private PlayerData m_localUserData; // This has an ID that's not necessarily the OwnerClientId, since all clients will see all spawned objects regardless of ownership.

        public void Initialize(Action onConnectionVerified, int expectedPlayerCount, Action onGameBegin, Action onGameEnd, LocalPlayer localUser)
        {
            m_onConnectionVerified = onConnectionVerified;
            m_expectedPlayerCount = expectedPlayerCount;
            onGameBeginning = onGameBegin;
            m_onGameEnd = onGameEnd;
            m_canSpawnCoins = false;
            m_localUserData = new PlayerData(localUser.DisplayName.Value, 0);
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
            if (word.Length < 3)
            {
                id = word;
            }
            else
            {
                id = word.Substring(word.Length - 3);
            }

            ulong result = 0;
            if (!ulong.TryParse(id, out result))
            {
                throw new Exception("Não foi possível converter a string em um valor ulong.");
            }

            return result;
        }

        public override void OnNetworkSpawn()
        {
            Score.Instance.AddPlayerServerRpc(m_localUserData.name, NetworkManager.Singleton.LocalClientId, GetId(m_localUserData.name));

            if (IsHost)
            {
                InvokeRepeating(nameof(UpdateGameTimer), 0.0f, 1.0f);
            }
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

        public override void OnNetworkDespawn()
        {
            m_onGameEnd(); // As a backup to ensure in-game objects get cleaned up, if this is disconnected unexpectedly.
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

        [ClientRpc]
        private void EndGame_ClientRpc()
        {
            if (IsHost)
                return;
            SendLocalEndGameSignal();
        }

        private void SendLocalEndGameSignal()
        {
            m_onGameEnd();
        }
    }
}