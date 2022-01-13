using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    public Transform FollowTarget;
    [Range(0,1)]
    public float Speed;

    [Space] 
    public Vector3 DownPlace;
    public Vector3 DownRotation;
    [Range(0, 1)] public float Lerp;
    public Vector3 UpPlace;
    public Vector3 UpRotation;
    private void Update()
    {
        
    }
}
