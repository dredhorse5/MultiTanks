using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace MultiTanks
{
    public class TankGun : MonoBehaviour
    {
        public Types Type;

        [Header("Rotate")] 
        public float RotateSpeed = 50f;
        public float RotateAcceleration = 1;

        [Header("Ammo and Reload")] 
        public float MaxAmmo = 100;
        public float AmmoForShoot = 100;
        public float AddAmmoPerSecond = 100f;
        [Space] 
        public float ReloadTime = 1f;
        
        [Header("Shoot")]
        public float ShootForce = 500;
        public Projectile ProjectilePrefab;
        public List<Muzzle> Muzzles;


        private float rawRotateValue;
        private float rotateValue;
        private TankGunNet owner;
        private Transform parent;
        
        public float currentAmmo { get; private set; }
        public float reloadTimer { get; private set; }
        private Muzzle currentMuzzle => Muzzles[owner.currentMuzzle];
        
        public void SetOwner(TankGunNet owner) => this.owner = owner;

        private void Start()
        {
            parent = transform.parent.transform;
            currentAmmo = MaxAmmo;
            reloadTimer = 0;
        }

        private void Update()
        {
            if (!owner || !owner.isServer)
                return;
            UpdateRotate();
            
            if (currentAmmo < MaxAmmo)
            {
                currentAmmo += AddAmmoPerSecond * Time.deltaTime;
                if (currentAmmo > MaxAmmo)
                    currentAmmo = MaxAmmo;
            }

            if (reloadTimer > 0)
                reloadTimer -= Time.deltaTime;
        }

        public void FireFeedback()
        {
            if(currentMuzzle.Particles)
                currentMuzzle.Particles.Play();
        }

        private void UpdateRotate()
        {
            rotateValue = Mathf.Lerp(rotateValue, rawRotateValue, Time.deltaTime * RotateAcceleration);
            parent.Rotate(Vector3.up, rotateValue * Time.deltaTime * RotateSpeed);
        }
        public void SetRotate(float value) => rawRotateValue = Mathf.Clamp(value, -1f, 1f);

        public (bool,Vector3,Vector3) Fire()
        {
            if (!owner || !owner.isServer || AmmoForShoot > currentAmmo || reloadTimer > 0f)
                return (false,Vector3.zero, Vector3.zero);

            currentAmmo -= AmmoForShoot;
            reloadTimer = ReloadTime;
            owner.currentMuzzle++;
            if (owner.currentMuzzle >= Muzzles.Count)
                owner.currentMuzzle = 0;
            var pool = Instantiate(ProjectilePrefab, currentMuzzle.SpawnPlace.position, currentMuzzle.SpawnPlace.rotation);
            NetworkServer.Spawn(pool.gameObject);


            return new (true,currentMuzzle.SpawnPlace.forward * ShootForce,currentMuzzle.SpawnPlace.position);
        }
        
        public enum Types : byte
        {
            None,
            Gun1,
            Gun2,
            Gun3,
            Gun4,
            Gun5,
            Gun6,
            Gun7,
            Gun8,
            Gun9,
            Gun10
        }

        [Serializable]
        public struct Muzzle
        {
            public Transform SpawnPlace;
            public ParticleSystem Particles;
        }
    }
}