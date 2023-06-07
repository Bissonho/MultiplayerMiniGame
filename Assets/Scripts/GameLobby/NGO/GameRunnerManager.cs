using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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

        public Score scoreData;

        public Action onGameBeginning;
        Action m_onConnectionVerified, m_onGameEnd;
        private int
            m_expectedPlayerCount; // Used by the host, but we can't call the RPC until the network connection completes.
        private bool? m_canSpawnCoins;

        //[SerializeField]
        //private NetworkedDataStore m_dataStore = default;


        private float m_timeout = 5;
        private bool m_hasConnected = false;

        [SerializeField] private float GAME_DURATION = 5f; // Duração do jogo em segundos
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
            scoreData.AddPlayerServerRpc(NetworkManager.Singleton.LocalClientId, GetId(m_localUserData.name), m_localUserData.name);
        }


        private void Update()
        {
            if (m_timeout >= 0)
            {
                m_timeout -= Time.deltaTime;
                if (m_timeout < 0)
                    BeginGame();
            }


            if (gameRunning)
            {
                counter += Time.deltaTime;

                //UIManager.Instance.SetTime((int)counter);
                //UnityEngine.Debug.Log("Tempo: " + (int)counter);

                if (counter >= GAME_DURATION)
                {
                    // Finalizar o jogo
                    EndGame();
                }

            }
        }

        public override void OnNetworkDespawn()
        {
            m_onGameEnd(); // As a backup to ensure in-game objects get cleaned up, if this is disconnected unexpectedly.
        }

        /// <summary>
        /// The game will begin either when all players have connected successfully or after a timeout.
        /// </summary>
        void BeginGame()
        {
            m_canSpawnCoins = true;
            GameManager.Instance.BeginGame();
            gameRunning = true;
            onGameBeginning?.Invoke();
            //m_introOutroRunner.DoIntro(StartMovingSymbols);
        }


        // Essa função é chamada pelo servidor quando o jogo precisa terminar. Uma vez que isso acontece, o servidor precisa informar aos clientes para limparem seus objetos de rede primeiro, 
        /// pois desconectar antes disso impedirá que eles façam isso (já que não podem receber eventos de despawn do servidor desconectado). ///
        [ClientRpc]
        private void WaitForEndingSequence_ClientRpc()
        {
            //m_scorer.OnGameEnd();
        }

        private void EndGame()
        {
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