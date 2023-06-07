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
        TMPro.TMP_Text m_name = default;
        ulong m_id;

        Action<ulong, Action<PlayerData>> m_retrieveName;
        Action<ulong, Action<PlayerData>> m_retrieveId;

        [SerializeField] private float _speed;
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private PlayerControl _playerControl;

        void Start()
        {
            _playerControl = new PlayerControl();
            _playerControl.Enable();
            _characterController = GetComponent<CharacterController>();
        }

        

        public override void OnNetworkSpawn()
        {      
            base.OnNetworkSpawn();
            m_retrieveName = Score.Instance.GetPlayerData;
            GameRunnerManager.Instance.onGameBeginning += OnGameBegan;

            if (!IsOwner)
            {
                enabled = false;
                _characterController.enabled = false;
                return;
            }
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

        [ClientRpc]
        private void SetName_ClientRpc(PlayerData data)
        {
            Debug.Log("SetName_ClientRpc" + "---" + data.name);
            if (!IsOwner)
                m_name.text = data.name;
        }

        [ClientRpc]
        private void SetId_ClientRpc(ulong id)
        {
            if (!IsOwner)
                m_id = id;
        }

        public void OnGameBegan()
        {
            m_retrieveName.Invoke(OwnerClientId, SetName_ClientRpc);
            GameRunnerManager.Instance.onGameBeginning -= OnGameBegan;
        }
    }
}