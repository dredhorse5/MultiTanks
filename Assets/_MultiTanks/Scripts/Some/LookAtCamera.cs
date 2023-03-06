using System;
using UnityEngine;

namespace MultiTanks
{
    public class LookAtCamera : MonoBehaviour
    {
        public Camera Camera => _camera ??= _camera = Camera.main;
        private Camera _camera;

        private void Update()
        {
            transform.LookAt(Camera.transform);
        }
    }
}