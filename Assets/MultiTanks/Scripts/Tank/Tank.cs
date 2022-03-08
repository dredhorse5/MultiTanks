using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Tank : NetworkBehaviour
{
    public class GunBrain
    {
        /*private float reloadTimer;

        public void Update()
        {
            if (reloadTimer > 0)
                reloadTimer -= Time.deltaTime;
        }
        
        [Command]
        public void Fire()
        {
            if(reloadTimer > 0)
                return;
            reloadTimer = ReloadTime;

            FireFeedback();
        
            var proj = Instantiate(ProjectilePrefab,ProjectileSpawnPlace.position, ProjectileSpawnPlace.rotation );
            NetworkServer.Spawn(proj.gameObject);
            proj.Init(ProjectileSpeed, ProjectileSpawnPlace.forward);
        }
        public void RotateTower(float value)
        {
            transform.localRotation *= Quaternion.Euler(Vector3.up * value * TowerRotateSpeed) ;
        }*/
    }
    public class BodyBrain
    {
        public BodyType Settings { get; protected set; }
        public BodyTankObject Object { get; protected set; }
        private Tank Owner;

        public BodyBrain(BodyType settings, Tank owner)
        {
            Settings = settings;
            this.Owner = owner;
            
            Object = Instantiate(settings.Prefab, owner.transform);
            NetworkServer.Spawn(Object.gameObject,owner.gameObject);
            Object.transform.localPosition = Vector3.zero;
        }

        ~BodyBrain()
        {
            
            NetworkServer.Destroy(Object.gameObject);
            Destroy(Object.gameObject);
        }
        
        [ServerCallback]
        public void SetForce(float value)
        {
            if(Object.wheelGroundedValue <= 0.01)
                return;
            var curForce = Mathf.Lerp(Settings.MinForce,Settings.MaxForce,Object.wheelGroundedValue);
            Owner._rigidbody.AddForceAtPosition(-1000f * curForce * value * Owner.transform.forward,Object.ForcePoint.position);
        
            Owner._rigidbody.AddForce(1000f * -Settings.ForwardFriction * Object.wheelGroundedValue * Object.forwardVelocity );
        }

        [ServerCallback]
        public void SetTorque(float value)
        {
            if (Object.wheelGroundedValue <= 0.01)
                return;
        
            Owner._rigidbody.AddTorque(1000f * Settings.Torque * value * Owner.transform.up * Settings.TorqueForceBySpeed.Evaluate(Owner._rigidbody.velocity.magnitude));
            Owner._rigidbody.AddForce(1000f * -Object.sideVelocity * Settings.SideFriction);
        }


    }
    
    [Header("Camera")]
    public CameraTarget CameraTarget;
    public float CameraSpeed;
    [Space] 
    public BodyType[] Bodies;
    public GunType[] Guns;


    private BodyBrain bodyBrain;
    private int nowBodyIndex;
    private NetworkTransformChild bodyNetwork;
    
    
    private GunBrain gunBrain;
    private int nowGunIndex;
    private NetworkTransformChild gunNetwork;

    
    public Rigidbody _rigidbody { get; protected set; }
    

    public override void OnStartClient()
    {
        _rigidbody = GetComponent<Rigidbody>();

        if (isLocalPlayer)
        {
            CameraFollow3D.Instance.Target = CameraTarget.transform;
        }
        SetBody(nowBodyIndex);
        SetGun(nowGunIndex);
    }


    private void Update()
    {
        UpdateInput();
    }

    public void UpdateInput()
    {
        if(isLocalPlayer == false)
            return;
        
        //if (Input.GetKey(KeyCode.C)) gun.Fire();

        
        
        if (Input.GetKeyDown(KeyCode.Q))
            CameraTarget.SetMove(1,CameraSpeed);
        if(Input.GetKeyDown(KeyCode.E))
            CameraTarget.SetMove(-1,CameraSpeed);
        if (Input.GetKeyUp(KeyCode.Q) || Input.GetKeyUp(KeyCode.E))
            CameraTarget.SetMove(0,CameraSpeed);
        
        
        if (Input.GetKeyUp(KeyCode.Keypad1))
            ChangeBody(-1);
        else if (Input.GetKeyUp(KeyCode.Keypad3))
            ChangeBody(1);
        
        if (Input.GetKeyUp(KeyCode.Keypad4))
            ChangeGun(-1);
        else if (Input.GetKeyUp(KeyCode.Keypad6))
            ChangeGun(1);
    }
    
    
    private void FixedUpdate()
    {
        if (!isLocalPlayer)
            return;

        if (_bodyTankObject)
        {
            _bodyTankObject.SetForce(Input.GetAxisRaw("Vertical"));
            _bodyTankObject.SetTorque(Input.GetAxisRaw("Horizontal"));
        }

        if (gun)
        {
           // gun.RotateTower(Input.GetAxis("TowerRotate"));
        }
    }

    #region Body & Gun management

    
    public void ChangeBody(int dir)
    {
        nowBodyIndex += dir > 0 ? 1 : -1;

        if (nowBodyIndex >= Bodies.Length)
            nowBodyIndex = 0;
        else if (nowBodyIndex < 0)
            nowBodyIndex = Bodies.Length - 1;
        
        SetBody(nowBodyIndex);
    }
    public void ChangeGun(int dir)
    {
        nowGunIndex += dir > 0 ? 1 : -1;

        if (nowGunIndex >= Guns.Length)
            nowGunIndex = 0;
        else if (nowGunIndex < 0)
            nowGunIndex = Guns.Length - 1;
        
        SetGun(nowGunIndex);
    }
    public void SetGun(int index)
    {
        /*if(gun)
        {
            NetworkServer.Destroy(gun.gameObject);
            Destroy(gun.gameObject);
        }
        
        gun = Instantiate(Guns[index],transform);
        //Gun.target = gun.transform;
        NetworkServer.Spawn(gun.gameObject,gameObject);
        
        gun.transform.position = _bodyTankObject.GunPlace.position;
        gun.SetOwner(this);

        CameraTarget.FollowTarget = gun.transform;*/
    }
    public void SetBody(int index)
    {
        /*
        if(_bodyTankObject)
        {
            NetworkServer.Destroy(_bodyTankObject.gameObject);
            Destroy(_bodyTankObject.gameObject);
        }
        
        _bodyTankObject = Instantiate(Bodies[index],transform);
        NetworkServer.Spawn(_bodyTankObject.gameObject,gameObject);*/
        
        /*
        _bodyTankObject.transform.localPosition = Vector3.zero;*/
        
        //Body

        if (gun)
            gun.transform.position = _bodyTankObject.GunPlace.position;

        _rigidbody.mass = _bodyTankObject.Mass;
    }

    #endregion
    
    public void AddImpulse(Vector3 impulse, Vector3 atPosition = default)
    {
        if (atPosition == default)
            _rigidbody.AddForce(impulse, ForceMode.Impulse);
        else
            _rigidbody.AddForceAtPosition(impulse, atPosition, ForceMode.Impulse);
    }

    public void AddForce(Vector3 force, Vector3 atPosition = default)
    {
        if (atPosition == default)
            _rigidbody.AddForce(force, ForceMode.Impulse);
        else
            _rigidbody.AddForceAtPosition(force, atPosition, ForceMode.Impulse);
    }

}

/*
public class PrefabPoolManager : MonoBehaviour
{
    [Header("Settings")] public int startSize = 5;
    public int maxSize = 20;
    public GameObject prefab;

    [Header("Debug")] [SerializeField] Queue pool;
    [SerializeField] int currentCount;


    void Start()
    {
        InitializePool();

        NetworkClient.RegisterPrefab(prefab, SpawnHandler, UnspawnHandler);
    }

    void OnDestroy()
    {
        NetworkClient.UnregisterPrefab(prefab);
    }

    private void InitializePool()
    {
        pool = new Queue();
        for (int i = 0; i < startSize; i++)
        {
            GameObject next = CreateNew();

            pool.Enqueue(next);
        }
    }

    GameObject CreateNew()
    {
        if (currentCount > maxSize)
        {
            Debug.LogError($"Pool has reached max size of {maxSize}");
            return null;
        }

        // use this object as parent so that objects dont crowd hierarchy
        GameObject next = Instantiate(prefab, transform);
        next.name = $"{prefab.name}_pooled_{currentCount}";
        next.SetActive(false);
        currentCount++;
        return next;
    }

    // used by NetworkClient.RegisterPrefab
    GameObject SpawnHandler(SpawnMessage msg)
    {
        return GetFromPool(msg.position, msg.rotation);
    }

    // used by NetworkClient.RegisterPrefab
    void UnspawnHandler(GameObject spawned)
    {
        PutBackInPool(spawned);
    }

    /// 
    /// Used to take Object from Pool.
    /// Should be used on server to get the next Object
    /// Used on client by NetworkClient to spawn objects
    /// 
    /// 
    /// 
    /// 
    public GameObject GetFromPool(Vector3 position, Quaternion rotation)
    {
        GameObject next = pool.Count > 0
            ? pool.Dequeue() // take from pool
            : CreateNew(); // create new because pool is empty

        // CreateNew might return null if max size is reached
        if (next == null)
        {
            return null;
        }

        // set position/rotation and set active
        next.transform.position = position;
        next.transform.rotation = rotation;
        next.SetActive(true);
        return next;
    }

    /// 
    /// Used to put object back into pool so they can b
    /// Should be used on server after unspawning an object
    /// Used on client by NetworkClient to unspawn objects
    /// 
    /// 
    public void PutBackInPool(GameObject spawned)
    {
        // disable object
        spawned.SetActive(false);

        // add back to pool
        pool.Enqueue(spawned);
    }
}
*/
