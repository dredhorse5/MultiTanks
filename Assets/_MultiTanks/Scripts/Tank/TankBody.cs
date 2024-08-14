using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace MultiTanks
{
    public class TankBody : MonoBehaviour
    {
        public Types Type;
        public float Mass = 100;
        public Transform GunPlace;
        [Space]
        public float MaxSpeed = 20f;
        public float MaxBackSpeed = 5;
        public float MoveForce = 1000;
        public AnimationCurve ForceCurveBySpeed= AnimationCurve.Constant(0,1,1);
        public float Torque = 2.5f;
        [Space] 
        [Min(0)] public float SideFriction = 1;
        [Min(0)] public float ForwardFriction = 1;
        public AnimationCurve TorqueForceBySpeed;

        public Transform ForcePoint;
        public List<TankWheel> Wheels;



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
            Wheels.ForEach(_ => _.Rigidbody = Rigidbody);
        }

        private void FixedUpdate()
        {
            CalculateWheelGrounded();


            float velocity = forwardVelocity.magnitude * (Vector3.Angle(forwardVelocity, transform.forward) > 90f ? 1f : -1f);
            Debug.Log(velocity);
            var force = ForceCurveBySpeed.Evaluate(velocity > 0 ? Mathf.InverseLerp(0f, MaxSpeed, velocity) : Mathf.InverseLerp(0f,-MaxBackSpeed, velocity)) * MoveForce;

            Wheels.ForEach(_ => _.SetForce(-force * forceValue));
            /*Rigidbody.AddForceAtPosition(-1000f * curForce * forceValue * transform.forward, ForcePoint.position);
            Rigidbody.AddForce(1000f * -ForwardFriction * wheelGroundedValue * forwardVelocity);*/
            if (wheelGroundedValue <= 0.01f)
                return;
            Rigidbody.AddTorque(1000f * Torque * torqueValue * transform.up * TorqueForceBySpeed.Evaluate(Rigidbody.velocity.magnitude));
            Rigidbody.AddForce(1000f * -sideVelocity * SideFriction);
        }
        



        public void SetForce(float value) => forceValue = value;

        public void SetTorque(float value)=> torqueValue = value;





        private void CalculateWheelGrounded()
        {
            wheelGroundedValue =Wheels.Find(_ => _.currentValueDistance < 1f) ? 1 : 0;
            /*float dist = 1;
            foreach (var wh in Wheels)
            {
                if (wh.currentValueDistance < dist)
                    dist = wh.currentValueDistance;
            }*/

            //wheelGroundedValue = (1f - dist); //Wheels.Find(_ => _.currentValueDistance < 1f) ? 1 : 0;
            /*
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

            wheelGroundedValue /= LeftWheels.Length + RightWheels.Length;*/
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