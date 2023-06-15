using System;
using Mirror;
using UnityEngine;

namespace MultiTanks
{
    [RequireComponent(typeof(Tank))]
    public class TankGunNet : NetworkBehaviour
    {
        public Transform Holder;

        [Header("Sync vars")] 
        [SyncVar] public float CurrentAmmoValue;
        [SyncVar] public float CurrentReloadValue;
        [SyncVar] public int currentMuzzle;
        
        [SyncVar(hook = nameof(OnChangeGun))] public TankGun.Types gunType = (TankGun.Types) 1;
        
        
        public TankGun nowGun { get; private set; }
        public Tank Tank => _tank ??= GetComponent<Tank>(); private Tank _tank;


        private void Update()
        {
            if(!isServer)
                return;
            if(!nowGun)
                return;
            CurrentAmmoValue = nowGun.currentAmmo / nowGun.MaxAmmo;
            CurrentReloadValue = nowGun.reloadTimer / nowGun.ReloadTime;
        }

        [Command]
        public void CmdSetGunRotate(float value)
        {
            if (nowGun)
                nowGun.SetRotate(value);
        }

        [Command]
        public void CmdFire()
        {
            if (nowGun)
            {
                var (key, value, pos) = nowGun.Fire();
                if(key)
                {
                    Tank.AddImpulse(-value, pos);
                    RpcFireFeedback();
                }
            }
        }

        [ClientRpc]
        public void RpcFireFeedback()
        {
            nowGun.FireFeedback();
        }
        
        

        [Command] 
        public void CmdChangeGun(TankGun.Types type) => ServerChangeGun(type);
        [Server] 
        public void ServerChangeGun(TankGun.Types type)
        {
            if (type == TankGun.Types.Gun10)
                type = TankGun.Types.Gun1;
            else if (type == TankGun.Types.None)
                type = TankGun.Types.Gun9;
            OnChangeGun(gunType, type);
            gunType = type;
        }

        public void OnChangeGun(TankGun.Types oldType, TankGun.Types newType)
        {
            
            if (nowGun)
                Destroy(nowGun.gameObject);
            nowGun = Instantiate(GameManager.Instance.GetGunPrefab(newType), Holder);
            nowGun.SetOwner(this);
            Debug.Log($"New gun: {nowGun.name}");
        }
    }
}