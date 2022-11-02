using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class BodyTankObject : MonoBehaviour
{
    public Transform ForcePoint;
    public WheelCollider[] LeftWheels;
    public WheelCollider[] RightWheels;
    public Transform GunPlace;
    
    
    
    public float wheelGroundedValue { get; protected set; }
    public Vector3 forwardVelocity => Vector3.Project(owner._rigidbody.velocity, transform.forward);
    public Vector3 sideVelocity => Vector3.Project(owner._rigidbody.velocity, transform.right);

    private Tank owner;

    public void SetOwner(Tank owner) => this.owner = owner;

    private void Start()
    {
        InitWheels();
    }

    private void FixedUpdate()
    {
        CalculateWheelGrounded();
    }

    
    
    public void SetForce(float value)
    {
        if(wheelGroundedValue <= 0.01)
            return;
        var curForce = Mathf.Lerp(MinForce,MaxForce,wheelGroundedValue);
        owner._rigidbody.AddForceAtPosition(-1000f * curForce * value * transform.forward,ForcePoint.position);
        
        owner._rigidbody.AddForce(1000f * -ForwardFriction * wheelGroundedValue * forwardVelocity );
    }

    public void SetTorque(float value)
    {
        if (wheelGroundedValue <= 0.01)
            return;
        
        owner._rigidbody.AddTorque(1000f * Torque * value * transform.up * TorqueForceBySpeed.Evaluate(owner._rigidbody.velocity.magnitude));
        owner._rigidbody.AddForce(1000f * -sideVelocity * SideFriction);
    }
    
    
    
    

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

}
