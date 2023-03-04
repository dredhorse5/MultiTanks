using System;
using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : NetworkBehaviour
{
    public float Speed = 200f;
    public float ExplosionRadius;
    public AnimationCurve ForceByRadius;
    public float Force;
    public DestroyAfter ExplosionPrefab;
    [Space] 
    public float DestroyAfter = 10f;
    
    
    public Rigidbody Rigidbody => _rigidbody ??= GetComponent<Rigidbody>(); private Rigidbody _rigidbody;
    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), DestroyAfter);
        Rigidbody.velocity = transform.forward * Speed;
    }

    [ServerCallback]
    private void FixedUpdate()
    {
        Vector3 curPos = transform.position;
        Vector3 futPos = transform.position + Rigidbody.velocity * Time.fixedDeltaTime;
        
        if (Physics.Raycast(curPos, (futPos - curPos).normalized, out RaycastHit hit,
                (curPos - futPos).magnitude))
        {
            Explosion(hit.point);
        }
    }

    // destroy for everyone on the server
    [Server]
    void DestroySelf() => NetworkServer.Destroy(gameObject);

    [Server]
    public void Explosion(Vector3 pos)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, ExplosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            var rb = hitCollider.attachedRigidbody;
            if (rb)
            {
                float DistVal = (transform.position - rb.transform.position).magnitude/ExplosionRadius;
                rb.AddForce((rb.transform.position - transform.position).normalized * Force * ForceByRadius.Evaluate(DistVal),ForceMode.Impulse);
            }
        }
        var exp = Instantiate(ExplosionPrefab, pos, Quaternion.identity);
        NetworkServer.Spawn(exp.gameObject);
        DestroySelf();
    }
}