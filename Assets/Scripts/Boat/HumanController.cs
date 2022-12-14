﻿using UnityEngine;
using UnityEngine.InputSystem;
using Rewired;

namespace BoatAttack
{
    /// <summary>
    /// This sends input controls to the boat engine if 'Human'
    /// </summary>
    public class HumanController : BaseController
    {
        private InputControls _controls;
        public int playerId = 0; // Rewired player id

        private float _throttle;
        private float _steering;

        private bool _paused;

        private Player player; // The Rewired player
        
        private void Awake()
        {
            //_controls = new InputControls();

            player = ReInput.players.GetPlayer(playerId);

            //_controls.BoatControls.Trottle.performed += context => _throttle = context.ReadValue<float>();
            //_controls.BoatControls.Trottle.canceled += context => _throttle = 0f;
            
            //_controls.BoatControls.Steering.performed += context => _steering = context.ReadValue<float>();
            //_controls.BoatControls.Steering.canceled += context => _steering = 0f;

            //_controls.BoatControls.Reset.performed += ResetBoat;
            //_controls.BoatControls.Pause.performed += FreezeBoat;

            //_controls.DebugControls.TimeOfDay.performed += SelectTime;
        }

        //public override void OnEnable()
        //{
        //    base.OnEnable();
        //    _controls.BoatControls.Enable();
        //}

        //private void OnDisable()
        //{
        //    _controls.BoatControls.Disable();
        //}

        private void ResetBoat()
        {
            controller.ResetPosition();
        }

        private void FreezeBoat()
        {
            _paused = !_paused;
            if (_paused)
            {
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = 1f;
            }
        }

        //private void SelectTime(InputAction.CallbackContext context)
        //{
        //    var value = context.ReadValue<float>();
        //    Debug.Log($"changing day time, input:{value}");
        //    DayNightController.SelectPreset(value);
        //}

        void Update()
        {
            _throttle = player.GetButton("Gas") ? 1 : 0;
            _steering = player.GetAxis("Horizontal");
            engine.Accelerate(_throttle);
            engine.Turn(_steering);

            if (player.GetButtonDown("Respawn"))
            {
                ResetBoat();
            }

            if (player.GetButtonDown("Pause"))
            {
                FreezeBoat();
            }
        }
    }
}

