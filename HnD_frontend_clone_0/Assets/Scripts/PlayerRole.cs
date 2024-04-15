using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerRole : MonoBehaviour
{
    // [SerializeField] private GameObject hiderAvatar;
    [SerializeField] private GameObject seekerAvatar;
    [SerializeField] private GameObject thirdPersonCamera;
    [SerializeField] private GameObject playerCamera;
    
    private CinemachineFreeLook _pCam;
    private Transform _tFollowTarget;

    private void Start()
    {
        seekerAvatar.SetActive(true);
        _pCam = thirdPersonCamera.GetComponent<CinemachineFreeLook>();
        // HiderCollision.Instance.touchedEvent.AddListener(ChangeToHider);
    }

    private void ChangeToHider()
    {
        // hiderAvatar.SetActive(false);
        // var impactPosition = hiderAvatar.transform.position;
        // Debug.Log("La position du hider est " + impactPosition);
        // SetSeekerTargetFollow(impactPosition);
        // seekerAvatar.SetActive(true); //this line was on 27 but the position change didn't work
    }

    private void SetSeekerTargetFollow(Vector3 impPos)
    {
        Debug.Log("SetSeekerTargetFollow called with position: " + impPos);
        _tFollowTarget = seekerAvatar.transform;
        _pCam.Follow = _tFollowTarget;
        _pCam.LookAt = _tFollowTarget;
        seekerAvatar.transform.position = impPos;
        var v = seekerAvatar.transform.position;
        Debug.Log("seekerAvatar position set to: " + v);
    }
    
    public void ChangeLocalPlayerStatus()
    {
        // Only called when Local player connect to this game
        // ThirdPersonMovement thirdHider = hiderAvatar.GetComponent<ThirdPersonMovement>();
        ThirdPersonMovement thirdSeeker = seekerAvatar.GetComponent<ThirdPersonMovement>();

        // Changing commands to be only responsive if local user
        thirdSeeker.isLocalPlayer = !thirdSeeker.isLocalPlayer;
        playerCamera.SetActive(true);
    }
}
