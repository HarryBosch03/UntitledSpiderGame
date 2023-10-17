using System;
using System.Collections.Generic;
using Crabs.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace Crabs.GameModes
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class TestingGameMode : GameMode
    {
        [SerializeField] private SpiderInput spiderPrefab;
        [SerializeField] private InputAction joinAction = new(binding: "/*/<button>");

        private static readonly List<InputUser> RegisteredUsers = new();

        protected override void OnEnable()
        {
            base.OnEnable();

            joinAction.Enable();
            joinAction.started += JoinDevice;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            joinAction.started -= JoinDevice;
            joinAction.Disable();
        }

        private void JoinDevice(InputAction.CallbackContext ctx) => JoinDevice(ctx.control.device);

        private void JoinDevice(InputDevice device)
        {
            if (device is Mouse) return;
            
            foreach (var otherUser in RegisteredUsers)
            {
                foreach (var otherDevice in otherUser.pairedDevices)
                {
                    if (otherDevice == device) return;
                }
            }

            var user = InputUser.PerformPairingWithDevice(device);
            RegisteredUsers.Add(user);

            spiderPrefab.SpawnWithUser(user);
        }
    }
}
