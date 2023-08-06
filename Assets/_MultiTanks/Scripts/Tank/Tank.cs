using System;
using DredPack.Camera;
using Mirror;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace MultiTanks
{
    [RequireComponent(typeof(TankGunNet))]
    [RequireComponent(typeof(TankBodyNet))]
    public class Tank : NetworkBehaviour
    {
        public CameraTarget CameraTarget;
        public TankBody.Types BodyOnStart;
        public TankGun.Types GunOnStart;

        public Image AmmoBar;




        public TankGunNet GunNet => _gunNet ??= GetComponent<TankGunNet>(); private TankGunNet _gunNet;
        public TankBodyNet BodyNet => _bodyNet ??= GetComponent<TankBodyNet>(); private TankBodyNet _bodyNet;
        public Rigidbody Rigidbody => _rigidbody ??= GetComponent<Rigidbody>(); private Rigidbody _rigidbody;


        public override void OnStartLocalPlayer()
        {
            CameraFollow3D.Instance.Target = CameraTarget.transform;
            BodyNet.CmdChangeBody(BodyOnStart);
            GunNet.CmdChangeGun(GunOnStart);
        }
        
        private void Update()
        {
            UpdateInput();
            UpdateBar();
        }

        private void UpdateBar()
        {
            if (!isLocalPlayer)
                return;
            AmmoBar.fillAmount = GunNet.CurrentAmmoValue;
        }

        private void UpdateInput()
        {
            if (!isLocalPlayer)
                return;

            
            //camera control
            if (Input.GetKey(KeyCode.Q))
                CameraTarget.SetMove(1);
            else if (Input.GetKey(KeyCode.E))
                CameraTarget.SetMove(-1);
            else
                CameraTarget.SetMove(0);
            

            //body control
            BodyNet.CmdSetBodyForce(Input.GetAxisRaw("Vertical"));
            BodyNet.CmdSetBodyTorque(Input.GetAxisRaw("Horizontal"));
            
            if(Input.GetKeyDown(KeyCode.R))
            {
                var b = (int) (BodyNet.bodyType);
                BodyNet.CmdChangeBody((TankBody.Types)(++b));
            }
            else if(Input.GetKeyDown(KeyCode.F))
            {
                var b = (int) (BodyNet.bodyType);
                BodyNet.CmdChangeBody((TankBody.Types)(--b));
            }
            
            //gun control
            if (Input.GetKey(KeyCode.Space))
                GunNet.CmdFire();
            
            if(Input.GetKeyDown(KeyCode.T))
            {
                var g = (int)(GunNet.gunType);
                GunNet.CmdChangeGun((TankGun.Types)(++g));
            }
            else if(Input.GetKeyDown(KeyCode.G))
            {
                var g = (int)(GunNet.gunType);
                GunNet.CmdChangeGun((TankGun.Types)(--g));
            }

            if (Input.GetKey(KeyCode.Z))
                GunNet.CmdSetGunRotate(-1);
            else if (Input.GetKey(KeyCode.X))
                GunNet.CmdSetGunRotate(1);
            else
                GunNet.CmdSetGunRotate(0);
            
            
        }


        public void AddImpulse(Vector3 impulse, Vector3 atPosition = default)
        {
            if (atPosition == default)
                Rigidbody.AddForce(impulse, ForceMode.Impulse);
            else
                Rigidbody.AddForceAtPosition(impulse, atPosition, ForceMode.Impulse);
        }
    }
}