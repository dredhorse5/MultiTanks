using System;
using UnityEngine;

namespace MultiTanks
{
    public class TankWheel : MonoBehaviour
    {
        public Rigidbody Rigidbody;
        [Space]
        public Vector2 MinFriction;
        public Vector2 Friction;
        public float Spring;
        public float SuspensionDistance;
        public float Damping;

        public Vector3 rayDirection => -transform.up;
        public float currentDistance { get; private set; }
        public float currentValueDistance => currentDistance / SuspensionDistance;
        public Vector3 currentHitPosition { get; private set; }
        public Rigidbody groundRigidbody { get; private set; }
   
        private float lastDistance = -1;
        public bool isGround { get; private set; }

        private float forwardForce = 0;

        private void FixedUpdate()
        {
            if (!Rigidbody)
                return;

            RaycastHit hit;
            if (!Physics.Raycast(transform.position, rayDirection, out hit, SuspensionDistance))
            {
                currentDistance = SuspensionDistance;
                lastDistance = SuspensionDistance;
                currentHitPosition = hit.point;
                isGround = false;
                groundRigidbody = null;
                return;
            }

            isGround = true;
            currentHitPosition = hit.point;
            currentDistance = hit.distance;
            groundRigidbody = hit.rigidbody;
            Vector3 groundVelocity = groundRigidbody == null ? Vector3.zero : groundRigidbody.GetPointVelocity(currentHitPosition);

            
            
            // suspension
            if (lastDistance <= -1)
                lastDistance = currentDistance;
            float deltaDistance = lastDistance - currentDistance;
            lastDistance = currentDistance;

            float damping = 0;
            if (deltaDistance > 0)
                damping = (deltaDistance / SuspensionDistance) * Damping;

            float upForceValue = (SuspensionDistance - currentDistance) * Spring + damping;
            Vector3 upForce = hit.normal * upForceValue;

            Rigidbody.AddForceAtPosition(upForce, transform.position, ForceMode.Force);

            
            
            
            var pointVelocity = Rigidbody.GetPointVelocity(currentHitPosition);
            pointVelocity -= groundVelocity;
            //Debug.DrawRay(currentHitPosition, pointVelocity, Color.green);
            
            
            //side friction


            Vector3 side = Vector3.Project(pointVelocity, transform.right);
            side *= -Friction.x;
            side += side.normalized * MinFriction.x;
            
            Debug.DrawRay(currentHitPosition, side/Rigidbody.mass, Color.red);
            
            Rigidbody.AddForceAtPosition(side, currentHitPosition, ForceMode.Force);
            
            
            
            //forward friction
            Vector3 forward = Vector3.Project(pointVelocity, transform.forward);
            forward *= -Friction.y;
            forward += forward.normalized * MinFriction.y;
            forward += transform.forward * forwardForce;
            Debug.DrawRay(currentHitPosition, forward/Rigidbody.mass, Color.blue);

            Rigidbody.AddForceAtPosition(forward, currentHitPosition, ForceMode.Force);

        }

        public void SetForce(float force) => forwardForce = force;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position,rayDirection * currentDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position + rayDirection * currentDistance,rayDirection * (SuspensionDistance - currentDistance));
        }
    }
}