using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Crabs.Player
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public sealed class PauseMenu : MonoBehaviour
    {
        private UIDocument document;
        private bool isPaused;
        
        public event Action PauseEvent;

        private void Awake()
        {
            document = transform.Find("PauseMenu").GetComponent<UIDocument>();
            document.rootVisualElement.visible = false;
        }

        public void SetPause(bool isPaused)
        {
            if (isPaused == this.isPaused) return;
            this.isPaused = isPaused;

            Time.timeScale = 0.0f;
            document.rootVisualElement.visible = true;
            PauseEvent?.Invoke();
        }
    }
}
