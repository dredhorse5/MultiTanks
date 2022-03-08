using UnityEngine;

[CreateAssetMenu(fileName = "Body Type", menuName = "New Body Type", order = 52)]
public class BodyType : ScriptableObject
{

    public float Mass;
    [Space] 
    public float MinForce = .5f;
    public float MaxForce = 1f;
    public float Torque = 2.5f;
    [Space] 
    [Min(0)] public float SideFriction = 1;
    [Min(0)] public float ForwardFriction = 1;
    public AnimationCurve TorqueForceBySpeed;
    [Space]
    public BodyTankObject Prefab;
}