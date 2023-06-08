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
   
    public class Player : NetworkBehaviour
    {
        TMPro.TMP_Text m_name = default;
        ulong m_id;

        Action<ulong, Action<PlayerData>> m_retrieveName;
        Action<ulong, Action<PlayerData>> m_retrieveId;

        [SerializeField] private float _speed;
        [SerializeField] private PlayerControl _playerControl;

        void Start()
        {
            _playerControl = new PlayerControl();
            _playerControl.Enable();
            //_characterController = GetComponent<CharacterController>();
        }

        

        public override void OnNetworkSpawn()
        {      
            base.OnNetworkSpawn();
            if (!IsOwner)
            {
                enabled = false;
                //_characterController.enabled = false;
                return;
            }
        }

        //FixedUpdate??
        void Update()
        {
            if (!IsOwner)
                return;

            if (_playerControl.Player.Move.IsInProgress())
            {
                Vector2 move = _playerControl.Player.Move.ReadValue<Vector2>();
                //Vector3 move3D = new Vector3(move.x, move.y, 0);

                GetComponent<Rigidbody2D>().velocity = move * _speed;

                //_characterController.Move(move3D * _speed * Time.deltaTime);
            } else {
                GetComponent<Rigidbody2D>().velocity = new Vector2();
            }
        }
    }
}