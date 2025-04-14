using UnityEngine;

public class PlayerRole : MonoBehaviour
{
    [SerializeField] private GameObject seekerAvatar;
    [SerializeField] private GameObject playerCamera;

    [SerializeField] private Material _seekerMaterial;
    [SerializeField] private Material _hiderMaterial;
    [SerializeField] private SkinnedMeshRenderer _rendererAvatar;
    
    private const string SeekerName = "Seeker";

    private void Start()
    {
        seekerAvatar.SetActive(true);
    }

    public void ChangeLocalPlayerStatus()
    {
        // Only called when Local player connect to this game
        ThirdPersonMovement thirdSeeker = seekerAvatar.GetComponent<ThirdPersonMovement>();

        // Changing commands to be only responsive if local user
        thirdSeeker.isLocalPlayer = !thirdSeeker.isLocalPlayer;
        playerCamera.SetActive(true);
    }
    
    public void SetHiderMaterial()
    {
        _rendererAvatar.material = _hiderMaterial;
    }

    public void SetSeekerMaterial()
    {
        //Debug.Log("Setting ht e sekker skin");
        _rendererAvatar.material = _seekerMaterial;
        seekerAvatar.tag = SeekerName;
    }

}
