using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HiderCollision : MonoBehaviour
{
    private const string SeekerName = "Seeker";
    public bool isSeeker;

    [SerializeField] private InGameMenuManager _inGameMenu;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision detected in COLLIDER");
        if (collision.gameObject.CompareTag(SeekerName) && !gameObject.CompareTag(SeekerName))
        {
            Debug.Log("Contact avec un objet dans le COLLIDER: " + collision.gameObject.tag);
            _inGameMenu.OnCollisionWithSeeker();
        }
    }
}
