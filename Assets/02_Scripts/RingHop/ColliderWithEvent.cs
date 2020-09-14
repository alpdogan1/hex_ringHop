using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RingHop
{
    public class ColliderWithEvent: MonoBehaviour
    {
        public Action DidTouch;
        
        private void OnTriggerEnter(Collider other)
        {
            DidTouch?.Invoke();
        }
    }
}