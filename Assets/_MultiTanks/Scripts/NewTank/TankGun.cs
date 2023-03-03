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
        public float ProjectileSpeed;


        public enum Types : byte
        {
            None,
            Gun1,
            Gun2,
            Gun3
        }
    }
}