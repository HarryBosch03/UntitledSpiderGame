using UnityEngine;

namespace UntitledSpiderGame.Runtime.Player
{
    public class WorldAlign : MonoBehaviour
    {
        private void LateUpdate() { transform.rotation = Quaternion.identity; }
    }
}
