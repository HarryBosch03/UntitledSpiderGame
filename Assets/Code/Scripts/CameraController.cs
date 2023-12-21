using System;
using UnityEngine;
using UntitledSpiderGame.Runtime.Data;
using UntitledSpiderGame.Runtime.Player;
using UntitledSpiderGame.Runtime.Spider;
using UntitledSpiderGame.Runtime.Utility;

namespace UntitledSpiderGame.Runtime
{
    public class CameraController : MonoBehaviour
    {
        public float spring = 100.0f;
        public float damper = 10.0f;

        public float shakeAmplitude;
        public float shakeFrequency;
        public float shakeDecay;

        public float padding = 15.0f;
        public float minSize = 10.0f;

        [Space]
        public Color highContrastClearColor = Color.white;
        [HideInInspector] public Color defaultClearColor;
        
        private new Camera camera;

        private DampedSpring min;
        private DampedSpring max;

        private float shakeVolume;

        private event Action FixedUpdateEvent;
        private static event Action<float> ShakeEvent;

        private void Awake()
        {
            camera = Camera.main;
            defaultClearColor = camera.backgroundColor;
        }

        private void OnFileSaved(GameSettings saveData)
        {
            camera.backgroundColor = saveData.highContrastLevel ? highContrastClearColor : defaultClearColor;
        }

        private void OnEnable()
        {
            SaveFile.Settings.FileSavedEvent += OnFileSaved;
            OnFileSaved(SaveFile.Settings);
            
            var v = new Vector2(camera.aspect, 1.0f);
            min = new DampedSpring((Vector2)camera.transform.position - v * minSize, ref FixedUpdateEvent);
            max = new DampedSpring((Vector2)camera.transform.position + v * minSize, ref FixedUpdateEvent);

            ShakeEvent += OnShake;
        }

        private void OnDisable()
        {
            SaveFile.Settings.FileSavedEvent -= OnFileSaved;
            ShakeEvent -= OnShake;
        }

        private void OnShake(float shake) { shakeVolume += shake; }

        private void FixedUpdate() { FixedUpdateEvent?.Invoke(); }

        private void Update()
        {
            if (SpiderController.All.Count > 0)
            {
                var min = new Vector2(float.MaxValue, float.MaxValue);
                var max = new Vector2(float.MinValue, float.MinValue);

                foreach (var player in SpiderController.All)
                {
                    min.x = Mathf.Min(min.x, player.transform.position.x);
                    min.y = Mathf.Min(min.y, player.transform.position.y);

                    max.x = Mathf.Max(max.x, player.transform.position.x);
                    max.y = Mathf.Max(max.y, player.transform.position.y);
                }

                min -= Vector2.one * padding;
                max += Vector2.one * padding;

                this.min.SetConstants(spring, damper);
                this.max.SetConstants(spring, damper);

                this.min.targetPosition = min;
                this.max.targetPosition = max;
            }

            var center = (max.currentPosition + min.currentPosition) * 0.5f;
            var size2 = max.currentPosition - min.currentPosition;
            var size = Mathf.Max(minSize, size2.y * 0.5f, size2.x / camera.aspect * 0.5f);

            center += CalculateShake();

            camera.orthographicSize = size;
            camera.transform.position = new Vector3(center.x, center.y, -10.0f);
        }

        private Vector2 CalculateShake()
        {
            var t = Time.time * shakeFrequency;
            shakeVolume -= shakeVolume * shakeDecay * Time.deltaTime;

            var vector2 = new Vector2();
            Noise.seed = 0;
            vector2.x = Noise.Perlin(t);
            Noise.seed = 1;
            vector2.y = Noise.Perlin(t);
            return vector2 * shakeAmplitude * shakeVolume;
        }

        public static void Shake(float shake) { ShakeEvent?.Invoke(shake); }
    }
}