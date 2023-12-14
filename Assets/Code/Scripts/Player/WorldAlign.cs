using UnityEngine;

namespace Crabs.Player
{
    public class WorldAlign : MonoBehaviour
    {
        private void LateUpdate() { transform.rotation = Quaternion.identity; }
    }
}
