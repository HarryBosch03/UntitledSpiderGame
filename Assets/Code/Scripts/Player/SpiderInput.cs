using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Crabs.Player
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public class SpiderInput : MonoBehaviour
    {
        [SerializeField] private int gamepadIndex;
        [SerializeField][Range(0.0f, 1.0f)] private float reachThreshold = 0.2f;
        
        public Vector2 MoveDirection { get; private set; }
        public Vector2 ReachDirection { get; private set; } = Vector2.up;
        public bool Reaching { get; private set; }
        public bool PrimaryUse { get; set; }
        public bool SecondaryUse { get; set; }
        public bool Drop { get; set; }

        private static List<Gamepad> usedGamepads = new();
        
        private void Update()
        {
            var gamepad = GetGamepad();
            if (gamepad == null) return;

            MoveDirection = gamepad.leftStick.ReadValue();

            var reachInput = gamepad.rightStick.ReadValue();
            Reaching = reachInput.magnitude > reachThreshold;
            if (Reaching) ReachDirection = reachInput.normalized;

            if (gamepad.rightTrigger.wasPressedThisFrame) PrimaryUse = true;
            if (gamepad.leftTrigger.wasPressedThisFrame) SecondaryUse = true;
            if (gamepad.leftShoulder.wasPressedThisFrame) Drop = true;
        }

        private Gamepad GetGamepad()
        {
            if (gamepadIndex < 0) return null;
            if (gamepadIndex >= Gamepad.all.Count) return null;
                
            return Gamepad.all[gamepadIndex];
        }

        public void ResetTriggers()
        {
            PrimaryUse = false;
            SecondaryUse = false;
            Drop = false;
        }
    }
}