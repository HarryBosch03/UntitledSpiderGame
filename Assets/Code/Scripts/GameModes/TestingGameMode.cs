using System.Collections.Generic;
using Crabs.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Crabs.GameModes
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class TestingGameMode : GameMode
    {
        [SerializeField] private SpiderInput spiderPrefab;
        [SerializeField] private InputAction joinAction = new(binding: "/*/<button>");

        private static readonly List<InputDevice> RegisteredDevices = new();

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
            foreach (var other in RegisteredDevices)
            {
                if (other == device) return;
            }

            switch (device)
            {
                case Keyboard:
                    bind(device, Mouse.current);
                    break;
                case Mouse:
                    bind(Keyboard.current, device);
                    break;
                default:
                    bind(device);
                    break;
            }

            void bind(params InputDevice[] devices)
            {
                spiderPrefab.SpawnWithUser(devices);
                foreach (var e in devices) RegisteredDevices.Add(e);
            }
        }
    }
}