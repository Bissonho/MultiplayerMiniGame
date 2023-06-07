using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LobbyRelaySample.ngo
{
    /// <summary>
    /// This cursor object will follow the owning player's mouse cursor and be visible to the other players.
    /// The host will use this object's movement for detecting collision with symbol objects.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Player : NetworkBehaviour
    {
        TMPro.TMP_Text m_nameOutput = default;
        Camera m_mainCamera;
        NetworkVariable<Vector3> m_position = new NetworkVariable<Vector3>(Vector3.zero);
        ulong m_localId;

        [SerializeField] private float _speed;
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private PlayerControl _playerControl;

        // If the local player cursor spawns before this cursor's owner, the owner's data won't be available yet. This is used to retrieve the data later.
        Action<ulong, Action<PlayerData>> m_retrieveName;

        // The host is responsible for determining if a player has successfully selected a symbol object, since collisions should be handled serverside.
        List<SymbolObject> m_currentlyCollidingSymbols;

        void Start()
        {
            _playerControl = new PlayerControl();
            _playerControl.Enable();
            _characterController = GetComponent<CharacterController>();
        }

        public override void OnNetworkSpawn()
        {

            base.OnNetworkSpawn();

            if (!IsOwner)
            {
                enabled = false;
                _characterController.enabled = false; 
                return;
            }

            //Cursor.lockState = CursorLockMode.Locked;
            //_playerScore = GetComponent<PlayerScore>();
            //_playerScore.ScoreVariable.OnValueChanged += _playerScore.OnScoreChanged;
        }

        void Update()
        {
            if (!IsOwner)
                return;

            if (_playerControl.Player.Move.inProgress)
            {
                Vector2 move = _playerControl.Player.Move.ReadValue<Vector2>();
                Vector3 move3D = new Vector3(move.x, 0, move.y);
                _characterController.Move(move3D * _speed * Time.deltaTime);
            }
        }

        /// <summary>
        /// This cursor is spawned in dynamically but needs references to some scene objects. Pushing full object references over RPC calls
        /// is an option if we create custom data for each and ensure they're all spawned on the host correctly, but it's simpler to do
        /// some one-time retrieval here instead.
        /// This also sets up the visuals to make remote player cursors appear distinct from the local player's cursor.
        /// </summary>
        /*public override void OnNetworkSpawn()
        {
            m_retrieveName = NetworkedDataStore.Instance.GetPlayerData;
            m_mainCamera = GameObject.Find("InGameCamera").GetComponent<Camera>();
            InGameRunner.Instance.onGameBeginning += OnGameBegan;
            if (IsHost)
                m_currentlyCollidingSymbols = new List<SymbolObject>();
            m_localId = NetworkManager.Singleton.LocalClientId;

        }

        [ClientRpc]
        private void SetName_ClientRpc(PlayerData data)
        {
            if (!IsOwner)
                m_nameOutput.text = data.name;
        }


        [ServerRpc] // Leave (RequireOwnership = true) for these so that only the player whose cursor this is can make updates.
        private void SetPosition_ServerRpc(Vector3 position)
        {
            m_position.Value = position;
        }


        public void OnGameBegan()
        {
            m_retrieveName.Invoke(OwnerClientId, SetName_ClientRpc);
            InGameRunner.Instance.onGameBeginning -= OnGameBegan;
        }*/
    }
}