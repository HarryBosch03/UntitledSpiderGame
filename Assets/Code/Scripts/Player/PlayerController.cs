using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Crabs.Player
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class PlayerController : MonoBehaviour
    {
        public static readonly Color[] PlayerColors = 
        {
            new Color(1f, 0.67f, 0.04f),
            new Color(0.63f, 1f, 0.16f),
            new Color(0.15f, 0.35f, 1f),
            new Color(0.66f, 0.09f, 1f),
            new Color(1f, 0.18f, 0.09f),
            new Color(0.11f, 1f, 0.75f),
            new Color(1f, 0.11f, 0.88f),
        };
        
        public InputActionAsset inputAsset;
        [FormerlySerializedAs("spiderWeapon")] public Gun gun;

        private int playerIndex;
        private bool useMouse;
        private Camera mainCamera;
        private Dictionary<string, InputAction> actions;

        public SpiderController ActiveSpider { get; private set; }
        
        public static readonly List<PlayerController> All = new();

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            inputAsset = Instantiate(inputAsset);
            inputAsset.Enable();
            inputAsset.devices = new InputDevice[0];

            actions = new Dictionary<string, InputAction>();
            foreach (var e in inputAsset.FindActionMap("Spider"))
            {
                actions.Add(e.name, e);
            }

            playerIndex = All.Count;
            All.Add(this);
        }

        private void OnDisable()
        {
            Destroy(inputAsset);
            All.Remove(this);
        }

        private void Update()
        {
            if (ActiveSpider)
            {
                ActiveSpider.MoveDirection = actions["Move"].ReadValue<Vector2>();
                ActiveSpider.ReachVector = actions["Reach"].ReadValue<Vector2>();
                if (actions["Jump"].WasPerformedThisFrame()) ActiveSpider.Jump = true;
                if (actions["Web"].WasPerformedThisFrame()) ActiveSpider.Web = true;
                
                if (actions["Use"].WasPerformedThisFrame()) gun.Shoot = true;
                if (actions["Use"].WasReleasedThisFrame()) gun.Shoot = false;

                if (useMouse)
                {
                    var mousePos = (Vector2)mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                    ActiveSpider.ReachVector = mousePos - (Vector2)ActiveSpider.transform.position;
                }
            }
        }

        public void BindDevice(params InputDevice[] devices)
        {
            inputAsset.devices = devices;

            foreach (var e in devices)
            {
                if (e is not Keyboard && e is not Mouse) continue;
                
                useMouse = true;
                break;
            }

            name = "PlayerController [";

            for (var i = 0; i < devices.Length; i++)
            {
                var device = devices[i];
                name += i < devices.Length - 1 ? $"{device.name}, " : $"{device.name}]";
            }
        }

        public void AssignSpider(SpiderController spider)
        {
            ActiveSpider = spider;
            spider.SetColor(PlayerColors[playerIndex]);

            gun = spider.GetComponent<Gun>();
        }
    }
}