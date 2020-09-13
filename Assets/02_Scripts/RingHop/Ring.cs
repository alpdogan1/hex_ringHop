using System;
using UnityEngine;

namespace RingHop
{
    public class Ring: MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Success");
        }
    }
}