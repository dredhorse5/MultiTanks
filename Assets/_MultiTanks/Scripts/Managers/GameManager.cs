using System.Collections.Generic;
using DredPack;
using UnityEngine;

namespace MultiTanks
{
    public class GameManager : GeneralSingleton<GameManager>
    {
        public int MaxFPS = 60;
        public void Start()
        {
            Application.targetFrameRate = MaxFPS;
        }
        public List<TankGun> Guns;
        public List<TankBody> Bodies;

        public TankGun GetGunPrefab(TankGun.Types byType) => Guns.Find(_ => _.Type == byType);
        public TankBody GeBodyPrefab(TankBody.Types byType) => Bodies.Find(_ => _.Type == byType);
    }
}