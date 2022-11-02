using UnityEngine;

[CreateAssetMenu(fileName = "Gun Type", menuName = "New Gun Type", order = 51)]
public class GunType : ScriptableObject
{
    [Header("Tower")] 
    public float TowerRotateSpeed = 1f;
    public float ReloadTime = 1f;
    public float ShootForce = 500;
    [Header("Projectile")] 
    public Projectile ProjectilePrefab;
    public float ProjectileSpeed;
    [Space]
    public BodyTankObject Prefab;

}