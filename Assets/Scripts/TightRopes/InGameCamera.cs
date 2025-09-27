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

    private bool IsFocus = true;

    void Awake()
    {
        IsFocus = true;
    }

    
    private void Update()
    {
        if(!IsFocus) return;

        transform.position = InGamePlayer.transform.position + Offset;
    }


    public void SetFocus(bool value)
    {
        IsFocus = value;    
    }
}
