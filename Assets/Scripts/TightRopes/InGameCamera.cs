using UnityEngine;

public class InGameCamera : MonoBehaviour
{
    [SerializeField]
    private InGamePlayer InGamePlayer;

    [SerializeField]    
    private Camera Cam;

    public Camera GetCam { get { return Cam; } }

    [SerializeField]
    private Vector3 Offset;
    private void Update()
    {
        transform.position = InGamePlayer.transform.position + Offset;
    }
}
