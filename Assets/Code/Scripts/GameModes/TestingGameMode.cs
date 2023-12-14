using System;
using System.Collections;
using System.Collections.Generic;
using Crabs.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace Crabs.GameModes
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class TestingGameMode : GameMode
    {
        [SerializeField] private SpiderInput spiderPrefab;
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
            SpiderController.SpiderDiedEvent += OnSpiderDied;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            joinAction.started -= JoinDevice;
            joinAction.Disable();
            SpiderController.SpiderDiedEvent -= OnSpiderDied;
        }

        private void OnSpiderDied(SpiderController spider)
        {
            StartCoroutine(Respawn(spider));
        }

        private IEnumerator Respawn(SpiderController spider)
        {
            yield return new WaitForSeconds(respawnTime);

            spider.transform.position = GetSpawnPoint();
            spider.gameObject.SetActive(true);
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
            var instance = spiderPrefab.SpawnWithUser(devices);
            instance.transform.position = GetSpawnPoint();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            
            Gizmos.DrawLine(new Vector2(spawnMin, spawnHeight), new Vector3(spawnMax, spawnHeight));
        }
    }
}