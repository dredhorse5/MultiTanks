using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    public Transform FollowTarget;
    [Space]
    [Space] 
    public float CloseDistance = 5;
    public float CloseHeight = 3;
    public Vector3 CloseRotationOffset;
    [Range(0, 1)] public float Lerp;
    public float FarDistance;
    public float FarHeight;
    public Vector3 FarRotationOffset;


    private Vector3 direction => Vector3.ProjectOnPlane(FollowTarget.forward, Vector3.up).normalized;

    private float distance => Mathf.Lerp(CloseDistance,FarDistance,Lerp);
    private float height => Mathf.Lerp(CloseHeight,FarHeight,Lerp);
    private Quaternion rotateOffset => Quaternion.Lerp( Quaternion.Euler(CloseRotationOffset), Quaternion.Euler(FarRotationOffset), Lerp);

    private int moveDirection = 0;
    private float moveSpeed = 1;


    private void Update()
    {
        if(!FollowTarget)
            return;
        Vector3 dir = direction;
        transform.position = FollowTarget.position + dir * distance + Vector3.up * height;
        
        transform.LookAt(FollowTarget);
        transform.rotation *= rotateOffset;
        
        if (moveDirection > 0)
        {
            Raise(moveSpeed);
        }
        else if (moveDirection < 0)
        {
            Lower(moveSpeed);
        }
    }

    public void SetMove(int value, float speed)
    {
        moveDirection = value;
        moveSpeed = speed;
    }
    public void Raise(float speed)
    {
        if (speed < 0f)
            return;
        if (Lerp > 1f)
        {
            Lerp = 1f;
            return;
        }

        Lerp += speed * Time.deltaTime;
    }

    public void Lower(float speed)
    {
        if (speed < 0f)
            return;
        if (Lerp < 0f)
        {
            Lerp = 0f;
            return;
        }

        Lerp -= speed * Time.deltaTime;
    } 
}
