using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace LobbyRelaySample.ngo
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] 
        private float _speed;
        private PlayerControl _playerControl;
        private Vector3 moveDirection = Vector3.zero;
        private CharacterController _characterController;

        void Start()
        {
            _characterController = GetComponent<CharacterController>();
            _playerControl = new PlayerControl();
            _playerControl.Enable();
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
        }


        void Update()
        {
            if (!IsOwner)
                return;

            if (_playerControl.Player.Move.IsInProgress())
            {
                Vector2 moveInput = _playerControl.Player.Move.ReadValue<Vector2>();
                _characterController.Move(moveInput * Time.deltaTime * _speed);
            }
        }
    }
}