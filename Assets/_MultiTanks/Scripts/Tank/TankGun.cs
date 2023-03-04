using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace MultiTanks
{
    public class TankGun : MonoBehaviour
    {
        public Types Type;
        
        [Header("Tower")] 
        public float TowerRotateSpeed = 1f;
        public float ReloadTime = 1f;
        public float ShootForce = 500;
        
        [Header("Projectile")] 
        public Projectile ProjectilePrefab;
        public Transform ProjectileSpawnPlace;
        [Space]
        public ParticleSystem FireParticles;



        private float reloadTimer;
        private Tank owner;
        private float rotateValue;
        private Transform parent;
        public void SetOwner(Tank owner) => this.owner = owner;

        private void Start()
        {
            parent = transform.parent.transform;
        }

        private void Update()
        {
            if(owner.isServer)
            {
                if (reloadTimer > 0)
                    reloadTimer -= Time.deltaTime;
                parent.Rotate(Vector3.up, rotateValue);
            }
        }

        public void FireFeedback()
        {
            FireParticles.Play();
        }
        
        
        public void SetRotate(float value) => rotateValue = Mathf.Clamp(value, -1f, 1f);

        public KeyValuePair<bool,Vector3> Fire()
        {
            if (reloadTimer > 0 || !owner.isServer)
                return new KeyValuePair<bool, Vector3>(false,Vector3.zero);
            
            reloadTimer = ReloadTime;
            var pool = Instantiate(ProjectilePrefab, ProjectileSpawnPlace.position, ProjectileSpawnPlace.rotation);
            NetworkServer.Spawn(pool.gameObject);


            return new KeyValuePair<bool, Vector3>(true,ProjectileSpawnPlace.forward * ShootForce);
        }
        
        public enum Types : byte
        {
            None,
            Gun1,
            Gun2,
            Gun3
        }
    }
}