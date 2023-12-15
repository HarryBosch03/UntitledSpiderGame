using System;
using System.Collections;
using System.Collections.Generic;
using Crabs.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace Crabs.GameModes
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class TestingGameMode : GameMode
    {
        [SerializeField] private PlayerController playerControllerPrefab;
        [SerializeField] private SpiderController spiderPrefab;
        [SerializeField] private float respawnTime;
        [SerializeField] private float spawnMin;
        [SerializeField] private float spawnMax;
        [SerializeField] private float spawnHeight;
        [SerializeField] private InputAction joinAction = new(binding: "/*/<button>");

        private static readonly List<InputDevice> RegisteredDevices = new();

        protected override void OnEnable()
        {
            base.OnEnable();

            joinAction.Enable();
            joinAction.started += JoinDevice;

            SpiderController.DiedEvent += OnSpiderDied;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            joinAction.started -= JoinDevice;
            joinAction.Disable();
            
            SpiderController.DiedEvent -= OnSpiderDied;
        }

        private void OnSpiderDied(SpiderController spider)
        {
            var controllingPlayer = (PlayerController)null;

            foreach (var player in PlayerController.All)
            {
                if (player.ActiveController != spider) continue;

                controllingPlayer = player;
                break;
            }

            if (!controllingPlayer) return;

            StartCoroutine(Respawn(controllingPlayer));
        }

        private IEnumerator Respawn(PlayerController player)
        {
            yield return new WaitForSeconds(respawnTime);

            var spider = Instantiate(spiderPrefab);
            spider.transform.position = GetSpawnPoint();
            spider.gameObject.SetActive(true);
            
            player.AssignSpider(spider);
        }

        private void Update()
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame)
            {
                var target = FindObjectOfType<SpiderHealth>();
                target.Damage(9999, target.transform.position, Vector2.up);
            }
        }

        public Vector2 GetSpawnPoint()
        {
            for (var i = 0; i < 500; i++)
            {
                var x = Random.Range(spawnMin, spawnMax);

                var start = new Vector2(x, spawnHeight);
                var hit = Physics2D.CircleCast(start, 3.0f, Vector2.down);
                if (hit) return hit.point + Vector2.up * 2.0f;
            }
            return Vector2.up * spawnHeight;
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
                foreach (var e in devices) RegisteredDevices.Add(e);
                SpawnPlayer(devices);
            }
        }

        private void SpawnPlayer(params InputDevice[] devices)
        {
            var pcInstance = Instantiate(playerControllerPrefab);
            pcInstance.BindDevice(devices);
            pcInstance.transform.position = GetSpawnPoint();

            var sp = GetSpawnPoint();
            var spiderInstance = Instantiate(spiderPrefab, sp, Quaternion.identity);
            pcInstance.AssignSpider(spiderInstance);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            
            Gizmos.DrawLine(new Vector2(spawnMin, spawnHeight), new Vector3(spawnMax, spawnHeight));
        }
    }
}