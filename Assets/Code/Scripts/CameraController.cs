using System;
using Crabs.Player;
using UnityEngine;

namespace Crabs
{
    public class CameraController : MonoBehaviour
    {
        public float smoothing;
        public float padding = 15.0f;
        public float minSize = 10.0f;

        private new Camera camera;
        private Vector2 center;
        private float size;
        
        private Vector2 smoothedCenter;
        private float smoothedSize;

        private void Awake() { camera = Camera.main; }

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
                
                center = (max + min) * 0.5f;
                var size2 = max - min;
                size = Mathf.Max(size2.y, size2.x / camera.aspect) * 0.5f;
            }
            
            size = Mathf.Max(size, minSize);
            smoothedCenter = Vector2.Lerp(smoothedCenter, center, Time.deltaTime / Mathf.Max(Time.unscaledDeltaTime, smoothing));
            smoothedSize = Mathf.Lerp(smoothedSize, size, Time.deltaTime / Mathf.Max(Time.unscaledDeltaTime, smoothing));
            
            camera.orthographicSize = smoothedSize;
            camera.transform.position = new Vector3(center.x, center.y, -10.0f);
        }
    }
}