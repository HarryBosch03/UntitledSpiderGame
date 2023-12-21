using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UntitledSpiderGame.Runtime.Player;
using UntitledSpiderGame.Runtime.Spider;
using Random = UnityEngine.Random;

namespace UntitledSpiderGame.Runtime.GameModes
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class GameController : MonoBehaviour
    {
        [SerializeField] private PlayerController playerControllerPrefab;
        [SerializeField] private SpiderController spiderPrefab;
        [SerializeField] private GamemodeSettings settings;
        [SerializeField] private float spawnMin;
        [SerializeField] private float spawnMax;
        [SerializeField] private float spawnHeight;
        [SerializeField] private InputAction joinAction = new(binding: "/*/<button>");

        private static readonly List<InputDevice> RegisteredDevices = new();

        public List<PlayerController> players = new();
        public List<int> scores = new();

        private void OnEnable()
        {
            joinAction.Enable();
            joinAction.started += JoinDevice;

            SpiderHealth.DiedEvent += OnSpiderDied;
        }

        private void OnDisable()
        {
            joinAction.started -= JoinDevice;
            joinAction.Disable();
            
            SpiderHealth.DiedEvent -= OnSpiderDied;
        }

        private void OnSpiderDied(SpiderHealth spiderHealth, GameObject o, DamageArgs arg3)
        {
            var controllingPlayer = (PlayerController)null;

            foreach (var player in players)
            {
                if (player.ActiveSpider.gameObject != spiderHealth.gameObject) continue;

                controllingPlayer = player;
                break;
            }

            if (!controllingPlayer) return;

            StartCoroutine(Respawn(controllingPlayer));
        }

        private IEnumerator Respawn(PlayerController player)
        {
            yield return new WaitForSeconds(settings.respawnTime);

            var spider = player.ActiveSpider;
            spider.gameObject.SetActive(true);
            spider.transform.position = GetSpawnPoint();
            spider.gameObject.SetActive(true);
            
            player.AssignSpider(spider);
        }

        private void Update()
        {
            if (Keyboard.current.rightBracketKey.wasPressedThisFrame)
            {
                var target = FindObjectOfType<SpiderHealth>();
                if (target) target.Damage(new DamageArgs
                {
                    damage = 9999
                }, null, target.transform.position, Vector2.up);
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
            
            players.Add(pcInstance);
            scores.Add(0);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            
            Gizmos.DrawLine(new Vector2(spawnMin, spawnHeight), new Vector3(spawnMax, spawnHeight));
        }
    }
}