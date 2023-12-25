using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UntitledSpiderGame.Runtime.Spider;

namespace UntitledSpiderGame.Runtime.Player
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
        public float gamepadCursorDistance = 5.0f;

        private int playerIndex;
        private bool useMouse;
        private Camera mainCamera;
        private Dictionary<string, InputAction> actions;

        public SpiderController ActiveSpider { get; private set; }

        public static readonly List<PlayerController> All = new();
        public static bool FreezeInput { get; set; }


        public static event Action<PlayerController, GameObject, DamageArgs> KillEvent;
        public static event Action<PlayerController, GameObject, DamageArgs> DiedEvent;

        private void Awake() { mainCamera = Camera.main; }

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

            SpiderHealth.DiedEvent += OnSpiderDied;
        }

        private void OnDisable()
        {
            Destroy(inputAsset);
            All.Remove(this);

            SpiderHealth.DiedEvent -= OnSpiderDied;
        }

        private void OnSpiderDied(SpiderHealth spider, GameObject invoker, DamageArgs args)
        {
            if (!ActiveSpider) return;
            if (spider.gameObject == ActiveSpider.gameObject)
            {
                DiedEvent?.Invoke(this, invoker, args);
            }

            if (invoker == ActiveSpider.gameObject)
            {
                KillEvent?.Invoke(this, invoker, args);
            }
        }

        private void Update()
        {
            if (!FreezeInput)
            {
                if (ActiveSpider)
                {
                    ActiveSpider.MoveDirection = actions["Move"].ReadValue<Vector2>();
                    ActiveSpider.ReachVector = actions["Reach"].ReadValue<Vector2>() * gamepadCursorDistance;
                    if (actions["Jump"].WasPerformedThisFrame()) ActiveSpider.Jump = true;
                    
                    if (actions["Web"].WasPerformedThisFrame()) ActiveSpider.Web = true;
                    if (actions["Web"].WasReleasedThisFrame()) ActiveSpider.Web = false;

                    if (actions["Use"].WasPerformedThisFrame()) ActiveSpider.ArmLeg.Use = true;
                    if (actions["Use"].WasReleasedThisFrame()) ActiveSpider.ArmLeg.Use = false;

                    if (useMouse)
                    {
                        var mousePos = (Vector2)mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                        ActiveSpider.ReachVector = mousePos - (Vector2)ActiveSpider.transform.position;
                    }
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
        }
    }
}