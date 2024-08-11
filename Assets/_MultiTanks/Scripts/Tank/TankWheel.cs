using System;
using UnityEngine;

namespace MultiTanks
{
    public class TankWheel : MonoBehaviour
    {
        public Rigidbody Rigidbody;
        [Space]
        public Vector2 Friction;
        public AnimationCurve FrictionCurve;
        public float Spring;
        public float SuspensionDistance;
        public float Damping;

        public Vector3 rayDirection => -transform.up;
        public float currentDistance { get; private set; }
        public float currentValueDistance => currentDistance / SuspensionDistance;
        public Vector3 currentHitPosition { get; private set; }
   
        private float lastDistance = -1;
        public bool isGround { get; private set; }

        private void FixedUpdate()
        {
            if(!Rigidbody)
                return;
            
            Vector3 upForce = GetUpForce();
            Rigidbody.AddForceAtPosition(upForce, transform.position, ForceMode.Force);
            if(isGround)
            {
                var mult = FrictionCurve.Evaluate(1f - currentValueDistance);
                var vel = Rigidbody.GetPointVelocity(currentHitPosition);

                Debug.DrawRay(currentHitPosition, vel, Color.green);

                Vector3 side = Vector3.Project(vel, transform.right);
                side *= Friction.x;
                Debug.DrawRay(currentHitPosition, side, Color.red);
                Rigidbody.AddForceAtPosition(side, currentHitPosition, ForceMode.Force);
            }



        }

        private Vector3 GetUpForce()
        {
            RaycastHit hit;
            if(!Physics.Raycast(transform.position, rayDirection,out hit, SuspensionDistance))
            {
                currentDistance = SuspensionDistance;
                lastDistance = SuspensionDistance;
                currentHitPosition = hit.point;
                isGround = false;
                return Vector3.zero;
            }

            isGround = true;
            currentHitPosition = hit.point;
            currentDistance = hit.distance;

            if (lastDistance <= -1)
                lastDistance = currentDistance;
            float deltaDistance = lastDistance - currentDistance;
            lastDistance = currentDistance;

            float damping = 0;
            if(deltaDistance > 0)
                damping = (deltaDistance / SuspensionDistance) * Damping;

            float upForceValue = (SuspensionDistance - currentDistance) * Spring + damping;
            Vector3 upForce = hit.normal * upForceValue;
            return upForce;
        }

        public void SetForce(Vector2 force)
        {
            
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position,rayDirection * currentDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position + rayDirection * currentDistance,rayDirection * (SuspensionDistance - currentDistance));
        }
    }
}