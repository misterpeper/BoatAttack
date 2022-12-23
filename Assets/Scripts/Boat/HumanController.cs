using UnityEngine;
using UnityEngine.InputSystem;
using Rewired;

namespace BoatAttack
{
    public class HumanController : BaseController
    {
        public int playerId = 0; // Rewired player id
        private Player player; // The Rewired player

        [HideInInspector] public float _throttle;
        [HideInInspector] public float _steering;

        private bool _paused;

        private void Awake()
        {
            player = ReInput.players.GetPlayer(playerId);
        }
        void FixedUpdate()
        {
            ThrottleInput();
            SteeringInput();
            GameplayInput();
        }
        private void ThrottleInput()
        {
            _throttle = player.GetButton("Gas") ? 1 : 0; // Player throttle input - true or false
            engine.Accelerate(_throttle);
        }

        private void SteeringInput()
        {
            _steering = player.GetAxis("Horizontal");    // Player steering input
            engine.Turn(_steering);
        }

        private void GameplayInput()
        {
            if (player.GetButtonDown("Respawn"))
                ResetBoat();
            if (player.GetButtonDown("Pause"))
                FreezeBoat();
        }
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
    }
}

