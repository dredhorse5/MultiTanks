using System;
using DredPack.Camera;
using Mirror;
using MultiTanks.Managers;
using NaughtyAttributes;
using UnityEngine;

namespace MultiTanks
{
    public class Tank : NetworkBehaviour
    {
        public CameraTarget CameraTarget;
        [Space]
        public Transform BodyHolder;
        public Transform GunHolder;



        [SyncVar(hook = nameof(OnChangeBody))] public TankBody.Types bodyType = (TankBody.Types) 1;
        public TankBody nowBody { get; private set; }

        [SyncVar(hook = nameof(OnChangeGun))] public TankGun.Types gunType = (TankGun.Types) 1;
        public TankGun nowGun { get; private set; }



        public Rigidbody Rigidbody => _rigidbody ??= GetComponent<Rigidbody>();
        private Rigidbody _rigidbody;


        public override void OnStartLocalPlayer()
        {
            CameraFollow3D.Instance.Target = CameraTarget.transform;
            CmdChangeBody((TankBody.Types)1);
            CmdChangeGun((TankGun.Types)1);
        }

        private void Update()
        {
            UpdateInput();
        }

        private void UpdateInput()
        {
            if(!isLocalPlayer)
                return;
            
            if (Input.GetKey(KeyCode.Q))
                CameraTarget.SetMove(1);
            else if(Input.GetKey(KeyCode.E))
                CameraTarget.SetMove(-1);
            else
                CameraTarget.SetMove(0);
            
            if (nowBody)
            {
                CmdSetBodyForce(Input.GetAxisRaw("Vertical"));
                CmdSetBodyTorque(Input.GetAxisRaw("Horizontal"));
            }

            if (nowGun)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                    CmdFire();
                
                if (Input.GetKey(KeyCode.Z))
                    CmdSetGunRotate(-1);
                else if(Input.GetKey(KeyCode.X))
                    CmdSetGunRotate(1);
                else
                    CmdSetGunRotate(0);
            }
        }
        [Command]
        private void CmdSetBodyForce(float input)
        {
            if(nowBody)
                nowBody.SetForce(input);
        }
        [Command]
        private void CmdSetBodyTorque(float input)
        {
            if(nowBody)
                nowBody.SetTorque(input);
        }

        [Command]
        private void CmdSetGunRotate(float value)
        {
            if (nowGun)
                nowGun.SetRotate(value);
        }

        [Command]
        private void CmdFire()
        {
            if (nowGun)
            {
                var (key, value) = nowGun.Fire();
                if(key)
                {
                    AddImpulse(-value, nowGun.ProjectileSpawnPlace.transform.position);
                    RpcFireFeedback();
                }
            }
        }

        [ClientRpc]
        private void RpcFireFeedback()
        {
            nowGun.FireFeedback();
        }
        
        public void AddImpulse(Vector3 impulse, Vector3 atPosition = default)
        {
            if (atPosition == default)
                Rigidbody.AddForce(impulse, ForceMode.Impulse);
            else
                Rigidbody.AddForceAtPosition(impulse, atPosition, ForceMode.Impulse);
        }

        #region Change Body and Gun

        public void OnChangeGun(TankGun.Types oldType, TankGun.Types newType)
        {
            if (nowGun)
                Destroy(nowGun.gameObject);
            nowGun = Instantiate(GameManager.Instance.GetGunPrefab(newType), GunHolder);
            nowGun.SetOwner(this);
            Debug.Log($"New gun: {nowGun.name}");
        }

        public void OnChangeBody(TankBody.Types oldType, TankBody.Types newType)
        {
            if (nowBody)
                Destroy(nowBody.gameObject);
            nowBody = Instantiate(GameManager.Instance.GeBodyPrefab(newType), BodyHolder);
            GunHolder.transform.position = nowBody.GunPlace.position;
            nowBody.SetOwner(this);
            Debug.Log($"New body: {nowBody.name}");
        }

        [Command] public void CmdChangeGun(TankGun.Types type) => ServerChangeGun(type);
        [Server] public void ServerChangeGun(TankGun.Types type)
        {
            OnChangeGun(gunType, type);
            gunType = type;
        }

        [Command]  public void CmdChangeBody(TankBody.Types type) => ServerChangeBody(type);
        [Server] public void ServerChangeBody(TankBody.Types type)
        {
            OnChangeBody(bodyType, type);
            bodyType = type;
        }
        
        

        #endregion
    }
}