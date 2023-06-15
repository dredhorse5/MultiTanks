using Mirror;
using UnityEngine;

namespace MultiTanks
{
    [RequireComponent(typeof(Tank))]
    public class TankBodyNet : NetworkBehaviour
    {
        public Transform Holder;
        
        
        [SyncVar(hook = nameof(OnChangeBody))] public TankBody.Types bodyType = (TankBody.Types) 1;

        
        
        public TankBody nowBody { get; private set; }
        public Tank Tank => _tank ??= GetComponent<Tank>(); private Tank _tank;
        
        [Command]
        public void CmdSetBodyForce(float input)
        {
            if(nowBody)
                nowBody.SetForce(input);
        }
        [Command]
        public void CmdSetBodyTorque(float input)
        {
            if(nowBody)
                nowBody.SetTorque(input);
        }
        

        
        
        public void OnChangeBody(TankBody.Types oldType, TankBody.Types newType)
        {
            if (nowBody)
                Destroy(nowBody.gameObject);
            nowBody = Instantiate(GameManager.Instance.GeBodyPrefab(newType), Holder);
            nowBody.SetOwner(this);
            Tank.Rigidbody.mass = nowBody.Mass;
            Tank.GunNet.Holder.transform.position = nowBody.GunPlace.position;
            Debug.Log($"New body: {nowBody.name}");
        }
        [Command] 
        public void CmdChangeBody(TankBody.Types type) => ServerChangeBody(type);
        
        [Server] 
        public void ServerChangeBody(TankBody.Types type)
        {
            if (type == TankBody.Types.Body8)
                type = TankBody.Types.Body1;
            else if (type == TankBody.Types.None)
                type = TankBody.Types.Body7;
            OnChangeBody(bodyType, type);
            bodyType = type;
        }
    }
}