using System;
using Mirror;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    public float ExplosionRadius;
    public AnimationCurve ForceByRadius;
    public float Force;
    [Space]
    public Rigidbody Rigidbody;
    [Space] 
    public float DestroyAfter = 10f;
    
    public void Init(float speed, Vector3 direction)
    {
        Rigidbody.velocity = direction.normalized * speed;
    }
    
    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), DestroyAfter);
    }
    
    
    // destroy for everyone on the server
    [Server]
    void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
    
    // ServerCallback because we don't want a warning if OnTriggerEnter is
    // called on the client
    [ServerCallback]
    void OnTriggerEnter(Collider co)
    {
        Explosion();
        NetworkServer.Destroy(gameObject);
    }

    [Command]
    public void Explosion()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, ExplosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            var rb = hitCollider.attachedRigidbody;
            if (rb)
            {
                Debug.Log(rb.name);
                float DistVal = (transform.position - rb.transform.position).magnitude/ExplosionRadius;
                rb.AddForce((rb.transform.position - transform.position).normalized * Force * ForceByRadius.Evaluate(DistVal),ForceMode.Impulse);
            }
        }
    }
}