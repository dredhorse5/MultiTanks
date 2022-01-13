using Mirror;
using UnityEngine;

public class DestroyAfter : NetworkBehaviour
{
    public float TimeToDestroy = 5f;

    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), TimeToDestroy);
    }
    
    // destroy for everyone on the server
    [Server]
    void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
}