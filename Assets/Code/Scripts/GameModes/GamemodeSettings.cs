using UnityEngine;

namespace UntitledSpiderGame.Runtime.GameModes
{
    [CreateAssetMenu(menuName = "Scriptable Object/Gamemode Settings")]
    public abstract class GamemodeSettings : ScriptableObject
    {
        public float respawnTime;
        public int scoreOnDeath;
        public int scoreOnKill;
    }
}