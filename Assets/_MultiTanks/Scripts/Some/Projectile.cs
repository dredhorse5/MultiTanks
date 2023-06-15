using System;
using Mirror;
using NaughtyAttributes;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    public float Speed = 200f;
    public DestroyAfter HitPrefab;
    public float LifeTime = 10f;
    public AnimationCurve ScaleByLifeTime;
    
    [Header("Modifiers")] 
    
    //Exploding
    public bool Exploding = false;
    [ShowIf(nameof(Exploding))] public float ExplosionRadius;
    [ShowIf(nameof(Exploding))] public AnimationCurve ForceByRadius;
    [ShowIf(nameof(Exploding))] public float Force;
    
    //Bounced
    [Space] 
    public bool Bounced = false;
    [ShowIf(nameof(Bounced))] public int MaxBounce = 256;

    private float lifeTimer;
    private Vector3 defaultScale;

    public override void OnStartServer()
    {
        defaultScale = transform.localScale;
    }

    private void Update()
    {
        if(!isServer)
            return;
        transform.localScale = ScaleByLifeTime.Evaluate(lifeTimer/LifeTime) * defaultScale;
        lifeTimer += Time.deltaTime;
        if (lifeTimer > LifeTime)
            DestroySelf();
    }

    [ServerCallback]
    private void FixedUpdate()
    {
        Vector3 curPos = transform.position;
        Vector3 futPos = transform.position + transform.forward * Speed * Time.fixedDeltaTime;

        var moveDir = (futPos - curPos).normalized;
        var maxDist = (curPos - futPos).magnitude;

        bool move = true;
        if (Physics.Raycast(curPos, moveDir, out RaycastHit hit, maxDist))
        {
            //        TODO:  !false --- replace to damage
            if (Bounced && MaxBounce > 0 && !false)
            {
                move = false;
                transform.forward = Vector3.Reflect(moveDir, hit.normal);
                transform.position = hit.point;
                MaxBounce--;
            }
            else
            {
                Explosion(hit.point);
                DestroySelf();
            }
        }
        if(move)
            transform.position += transform.forward * Speed * Time.fixedDeltaTime;
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
        if(HitPrefab)
        {
            var exp = Instantiate(HitPrefab, pos, Quaternion.identity);
            NetworkServer.Spawn(exp.gameObject);
        }
    }
    
}