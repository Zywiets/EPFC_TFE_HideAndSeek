using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiderCollision : MonoBehaviour
{
    private string SeekerName = "Seeker";
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision detected");
        if (other.gameObject.CompareTag(SeekerName))
        {
            Debug.Log("Collision with seeker detected");
        }
    }
}
