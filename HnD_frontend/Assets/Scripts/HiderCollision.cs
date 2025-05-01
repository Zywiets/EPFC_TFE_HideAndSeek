using UnityEngine;

public class HiderCollision : MonoBehaviour
{
    private const string SeekerName = "Seeker";
    public bool isSeeker;

    [SerializeField] private InGameMenuManager _inGameMenu;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(SeekerName);
        if (collision.gameObject.CompareTag(SeekerName) && !gameObject.CompareTag(SeekerName))
        {
            _inGameMenu.OnCollisionWithSeeker();
        }
    }
}
