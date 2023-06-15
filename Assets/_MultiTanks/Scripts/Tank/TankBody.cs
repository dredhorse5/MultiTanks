using Mirror;
using UnityEngine;

namespace MultiTanks
{
    public class TankBody : MonoBehaviour
    {
        public Types Type;
        public float Mass = 100;
        public Transform GunPlace;
        [Space] public float MinForce = .5f;
        public float MaxForce = 1f;
        public float Torque = 2.5f;
        [Space] 
        [Min(0)] public float SideFriction = 1;
        [Min(0)] public float ForwardFriction = 1;
        public AnimationCurve TorqueForceBySpeed;

        public Transform ForcePoint;
        public WheelCollider[] LeftWheels;
        public WheelCollider[] RightWheels;



        public float wheelGroundedValue { get; protected set; }
        public Vector3 forwardVelocity => Vector3.Project(Rigidbody.velocity, transform.forward);
        public Vector3 sideVelocity => Vector3.Project(Rigidbody.velocity, transform.right);
        private Rigidbody Rigidbody => owner.Tank.Rigidbody;

        private TankBodyNet owner;
        private float forceValue;
        private float torqueValue;

        public void SetOwner(TankBodyNet owner) => this.owner = owner;

        private void Start()
        {
            InitWheels();
        }

        private void FixedUpdate()
        {
            CalculateWheelGrounded();
            
            
            if (wheelGroundedValue <= 0.01f)
                return;
            var curForce = Mathf.Lerp(MinForce, MaxForce, wheelGroundedValue);
            Rigidbody.AddForceAtPosition(-1000f * curForce * forceValue * transform.forward, ForcePoint.position);
            Rigidbody.AddForce(1000f * -ForwardFriction * wheelGroundedValue * forwardVelocity);

            Rigidbody.AddTorque(1000f * Torque * torqueValue * transform.up * TorqueForceBySpeed.Evaluate(Rigidbody.velocity.magnitude));
            Rigidbody.AddForce(1000f * -sideVelocity * SideFriction);
        }
        



        public void SetForce(float value) => forceValue = value;

        public void SetTorque(float value)=> torqueValue = value;





        private void CalculateWheelGrounded()
        {
            wheelGroundedValue = 0;
            foreach (var wheel in LeftWheels)
            {
                if (wheel.isGrounded)
                    wheelGroundedValue++;
            }

            foreach (var wheel in RightWheels)
            {
                if (wheel.isGrounded)
                    wheelGroundedValue++;
            }

            wheelGroundedValue /= LeftWheels.Length + RightWheels.Length;
        }

        private void InitWheels()
        {
            foreach (var wheel in LeftWheels)
                wheel.motorTorque = 0.1f;
            foreach (var wheel in RightWheels)
                wheel.motorTorque = 0.1f;
        }

        public enum Types : byte
        {
            None,
            Body1,
            Body2,
            Body3,
            Body4,
            Body5,
            Body6,
            Body7,
            Body8,
            Body9
        }
    }
}