using UnityEngine;
using UnityEngine.InputSystem;

namespace Crabs.Utility
{
    public static class Extensions
    {
        public static Vector2 ToDirection(this float angleRadians) => new (Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));
        public static float ToAngle(this Vector2 vector) => Mathf.Atan2(vector.y, vector.x);

        public static Vector2 Vector2(this InputActionReference actionReference)
        {
            return actionReference.action?.ReadValue<Vector2>() ?? UnityEngine.Vector2.zero;
        }

        public static float Axis(this InputActionReference actionReference)
        {
            return actionReference.action?.ReadValue<float>() ?? 0.0f;
        }

        public static bool State(this InputActionReference actionReference, float deadzone = 0.5f)
        {
            return (actionReference.action?.ReadValue<float>() ?? 0.0f) > deadzone;
        }

        public static void Flag(this InputActionReference actionReference, ref bool flag)
        {
            if (actionReference.action?.WasPerformedThisFrame() ?? false) flag = true;
        }
        
        public static bool Flag(this InputActionReference actionReference, bool flag)
        {
            actionReference.Flag(ref flag);
            return flag;
        }
    }
}