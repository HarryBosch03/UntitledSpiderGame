using System;
using System.Collections.Generic;
using System.Text;
using Crabs.Utility;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.Utilities;

namespace Crabs.Player
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public class SpiderInput : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputAsset;
        [SerializeField] [Range(0.0f, 1.0f)] private float reachThreshold = 0.2f;

        private bool useMouse;
        private Camera mainCamera;

        private InputActionReference moveAction;
        private InputActionReference jumpAction;
        private InputActionReference reachAction;
        private InputActionReference primaryUseAction;
        private InputActionReference secondaryUseAction;
        private InputActionReference dropAction;
        private InputActionReference danceAction;
        
        public int Index { get; private set; }
        public Vector2 MoveDirection { get; private set; }
        public Vector2 ReachVector { get; private set; } = Vector2.up;
        public bool Reaching { get; private set; }
        public bool PrimaryUse { get; set; }
        public bool SecondaryUse { get; set; }
        public bool Drop { get; set; }
        public bool Dance { get; set; }
        public bool Jump { get; set; }

        public static readonly List<SpiderInput> All = new();
        
        private void Awake()
        {
            mainCamera = Camera.main;
            inputAsset = Instantiate(inputAsset);

            moveAction = bind("Move");
            jumpAction = bind("Jump");
            reachAction = bind("Reach");
            primaryUseAction = bind("Use0");
            secondaryUseAction = bind("Use1");
            dropAction = bind("Drop");
            danceAction = bind("Dance");
            
            InputActionReference bind(string path) => InputActionReference.Create(inputAsset.FindAction(path));
        }

        private void OnDestroy()
        {
            Destroy(inputAsset);
        }

        private void OnEnable()
        {
            inputAsset.Enable();
            Index = All.Count;
            All.Add(this);
        }

        private void OnDisable()
        {
            Index = -1;
            All.Remove(this);
        }

        private void Update()
        {
            MoveDirection = moveAction.Vector2();

            var reachInput = reachAction.Vector2();
            Reaching = reachInput.magnitude > reachThreshold;
            if (Reaching) ReachVector = Vector2.ClampMagnitude(reachInput, 1);

            PrimaryUse = primaryUseAction.Flag(PrimaryUse);
            SecondaryUse = secondaryUseAction.Flag(SecondaryUse);
            Drop = dropAction.Flag(Drop);
            Dance = danceAction.State();
            Jump = jumpAction.Flag(Jump);

            DoMouseInput();
        }

        private void DoMouseInput()
        {
            var m = Mouse.current;
            if (m == null) return;
            
            var devices = inputAsset.devices;
            if (!devices.HasValue) return;

            var use = false;
            foreach (var device in devices)
            {
                if (device is not Keyboard) continue;
                use = true;
                break;
            }
            if (!use) return;

            Reaching = true;

            var mousePosition = mainCamera.ScreenToWorldPoint(m.position.ReadValue());
            ReachVector = Vector2.ClampMagnitude(mousePosition - transform.position, 1);

            if (m.leftButton.wasPressedThisFrame) PrimaryUse = true;
            if (m.rightButton.wasPressedThisFrame) SecondaryUse = true;
        }

        public void ResetTriggers()
        {
            PrimaryUse = false;
            SecondaryUse = false;
            Drop = false;
            Jump = false;
        }

        public SpiderInput SpawnWithUser(params InputDevice[] devices)
        {
            var instance = Instantiate(this);
            instance.inputAsset.devices = devices;
            return instance;
        }
    }
}