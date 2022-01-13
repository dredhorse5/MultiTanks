using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Tank : NetworkBehaviour
{
    public float MinForce = .5f;
    public float MaxForce = 1f;
    public float Torque = 2.5f;
    public Transform ForcePoint;
    [Space] 
    [Min(0)] public float SideFriction = 1;
    [Min(0)] public float ForwardFriction = 1;
    public AnimationCurve TorqueForceBySpeed;
    [Header("Tower")]
    public GameObject Tower;
    public float TowerRotateSpeed = 1f;
    public float ReloadTime = 1f;
    public float ShootForce = 500;
    public ParticleSystem FireParticles;
    [Space] 
    public Projectile ProjectilePrefab;
    public float ProjectileSpeed;
    public Transform ProjectileSpawnPlace;
    
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
        if (reloadTimer > 0)
            reloadTimer -= Time.deltaTime;
        if(isLocalPlayer == false)
            return;
        if (Input.GetKey(KeyCode.C)) Fire();
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer)
            return;
        SetForce(Input.GetAxisRaw("Vertical"));
        SetTorque(Input.GetAxisRaw("Horizontal"));

        SetRotateTower(Input.GetAxis("TowerRotate"));
        CalculateWheelGrounded();
    }
    
    [ClientRpc]
    public void AddImpulse(Vector3 impulse, Vector3 atPosition)
    {
        if (atPosition == default)
            _rigidbody.AddForce(impulse, ForceMode.Impulse);
        else
            _rigidbody.AddForceAtPosition(impulse, atPosition, ForceMode.Impulse);
    }

    [ClientRpc]
    public void FireFeedback()
    {
        if(isServer)
            AddImpulse(Tower.transform.forward * ShootForce, Tower.transform.position);
        FireParticles.Play();
    }
    #region Control

    
    [Command]
    private void Fire()
    {
        if(reloadTimer > 0)
            return;
        reloadTimer = ReloadTime;

        FireFeedback();
        
        var proj = Instantiate(ProjectilePrefab,ProjectileSpawnPlace.position, ProjectileSpawnPlace.rotation );
        NetworkServer.Spawn(proj.gameObject);
        proj.Init(ProjectileSpeed, ProjectileSpawnPlace.forward);
    }
    private void SetRotateTower(float value)
    {
        Tower.transform.localRotation *= Quaternion.Euler(Vector3.up * value * TowerRotateSpeed) ;
    }

    private void SetForce(float value)
    {
        if(wheelGroundedValue <= 0.01)
            return;
        var curForce = Mathf.Lerp(MinForce,MaxForce,wheelGroundedValue);
        _rigidbody.AddForceAtPosition(1000f * curForce * value * transform.forward,ForcePoint.position);
        
        _rigidbody.AddForce(1000f * -ForwardFriction * wheelGroundedValue * forwardVelocity );
    }

    private void SetTorque(float value)
    {
        if (wheelGroundedValue <= 0.01)
            return;
        
        _rigidbody.AddTorque(1000f * Torque * value * transform.up * TorqueForceBySpeed.Evaluate(_rigidbody.velocity.magnitude));
        _rigidbody.AddForce(1000f * -sideVelocity * SideFriction);
    }

    private void InitWheels()
    {
        foreach (var wheel in LeftWheels)
            wheel.motorTorque = 0.1f;
        foreach (var wheel in RightWheels)
            wheel.motorTorque = 0.1f;
    }

    #endregion

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

}
