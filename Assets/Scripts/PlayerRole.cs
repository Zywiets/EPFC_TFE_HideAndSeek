using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerRole : MonoBehaviour
{
    [SerializeField] private GameObject hiderAvatar;
    [SerializeField] private GameObject seekerAvatar;
    [SerializeField] private GameObject thirdPersonCamera;
    
    private CinemachineFreeLook _pCam;
    private Transform _tFollowTarget;

    private void Start()
    {
        hiderAvatar.SetActive(true);
        _pCam = thirdPersonCamera.GetComponent<CinemachineFreeLook>();
        HiderCollision.Instance.touchedEvent.AddListener(ChangeToHider);
    }

    private void ChangeToHider()
    {
        hiderAvatar.SetActive(false);
        seekerAvatar.SetActive(true);
        SetFollowTarget(seekerAvatar);
        
    }

    private void SetFollowTarget(GameObject avatar)
    {
        Debug.Log("We could be Zombies");
        _tFollowTarget = avatar.transform;
        _pCam.Follow = _tFollowTarget;
        _pCam.LookAt = _tFollowTarget;
    }
}
