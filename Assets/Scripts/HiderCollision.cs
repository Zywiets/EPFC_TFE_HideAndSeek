using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HiderCollision : MonoBehaviour
{
    private const string SeekerName = "Seeker";

    public static HiderCollision Instance;
    public UnityEvent touchedEvent = new UnityEvent();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("L'instance n'est pas nulle");
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag(SeekerName)) return;
        Debug.Log("We are touched");
        touchedEvent.Invoke();
    }
    
}
