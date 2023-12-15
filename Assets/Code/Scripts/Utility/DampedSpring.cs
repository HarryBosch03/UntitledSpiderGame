using System;
using UnityEngine;

namespace Crabs.Utility
{
    [System.Serializable]
    public class DampedSpring
    {
        public float spring;
        public float damping;
        
        [HideInInspector] public Vector2 currentPosition;
        [HideInInspector] public Vector2 currentVelocity;
        [HideInInspector] public Vector2 targetPosition;
        [HideInInspector] public Vector2 targetVelocity;

        public DampedSpring(Vector2 position, ref Action fixedUpdateEvent)
        {
            currentPosition = position;
            fixedUpdateEvent += FixedUpdate;
        }

        public DampedSpring SetConstants(float spring, float damping)
        {
            this.spring = spring;
            this.damping = damping;
            return this;
        }

        private void FixedUpdate()
        {
            var force = (targetPosition - currentPosition) * spring + (targetVelocity - currentVelocity) * damping;

            currentPosition += currentVelocity * Time.deltaTime;
            currentVelocity += force * Time.deltaTime;
        }
    }
}