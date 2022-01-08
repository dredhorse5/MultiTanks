using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Tank : NetworkBehaviour
{
    public float MinFoce;
    public float MaxForce;
    public float Torque;
    [Space] 
    [Min(0)] public float SideFriction = 1;
    [Min(0)] public float ForwardFriction = 1;
    public AnimationCurve TorqueForceBySpeed;
    [Header("Tower")]
    public GameObject Tower;
    public float TowerRotateSpeed = 1f;
    public float ReloadTime = 1f;
    public ParticleSystem FireParticles;
    [Space]
    public WheelCollider[] LeftWheels;
    public WheelCollider[] RightWheels;

    
    
    private Rigidbody _rigidbody;
    private float wheelGroundedValue;
    private Vector3 forwardVelocity => Vector3.Project(_rigidbody.velocity, transform.forward);
    private Vector3 sideVelocity => Vector3.Project(_rigidbody.velocity, transform.right);
    private float reloadTimer;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        InitWheels();

        if (isLocalPlayer)
        {
            CameraFollow3D.Instance.Target = transform;
        }
    }

    private void Update()
    {
        if(isLocalPlayer == false)
            return;
        if (reloadTimer > 0)
            reloadTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer)
            return;
        SetForce(Input.GetAxisRaw("Vertical"));
        SetTorque(Input.GetAxisRaw("Horizontal"));

        SetRotateTower(Input.GetAxis("TowerRotate"));
        if (Input.GetKeyDown(KeyCode.C)) Fire();
        CalculateWheelGrounded();
    }

    public void Fire()
    {
        if(reloadTimer > 0)
            return;
        reloadTimer = ReloadTime;
        _rigidbody.AddForce(Tower.transform.forward * 1000f,ForceMode.Impulse);
        FireParticles.Play();
    }
    private void SetRotateTower(float value)
    {
        Tower.transform.localRotation *= Quaternion.Euler(Vector3.up * value * TowerRotateSpeed) ;
    }

    public void SetForce(float value)
    {
        if(wheelGroundedValue <= 0.01)
            return;
        var curForce = Mathf.Lerp(MinFoce,MaxForce,wheelGroundedValue);
        _rigidbody.AddForce(1000f * curForce * value * transform.forward);
        
        _rigidbody.AddForce(1000f * -ForwardFriction * wheelGroundedValue * forwardVelocity );
    }

    public void SetTorque(float value)
    {
        if (wheelGroundedValue <= 0.01)
            return;
        
        _rigidbody.AddTorque(1000f * Torque * value * transform.up * TorqueForceBySpeed.Evaluate(_rigidbody.velocity.magnitude));
        _rigidbody.AddForce(1000f * -sideVelocity * SideFriction);
    }

    public void InitWheels()
    {
        foreach (var wheel in LeftWheels)
            wheel.motorTorque = 0.1f;
        foreach (var wheel in RightWheels)
            wheel.motorTorque = 0.1f;
    }

    public void CalculateWheelGrounded()
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

}
