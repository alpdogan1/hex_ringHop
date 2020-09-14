using System;
using UnityEngine;

namespace RingHop
{
    public class LookAtCamera: MonoBehaviour
    {
        private Camera _camera;

        public Camera Camera
        {
            get
            {
                if (!_camera) _camera = Camera.main;
                return _camera;
            }
        }


        private void Update()
        {
            transform.rotation = Camera.transform.rotation;
        }
    }
}