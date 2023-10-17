using Crabs.Player;
using UnityEngine;

namespace Crabs.GameModes
{
    public abstract class GameMode : MonoBehaviour
    {
        public static GameMode ActiveGameMode { get; private set; }

        protected virtual void OnEnable()
        {
            if (ActiveGameMode) ActiveGameMode.enabled = false;
            ActiveGameMode = this;
            
            Debug.Log($"GameMode changed to [{GetType().Name}]{name}", this);
        }

        protected virtual void OnDisable()
        {
            if (ActiveGameMode == this) ActiveGameMode = null;
        }
    }
}