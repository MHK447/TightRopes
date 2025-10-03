using UnityEngine;

public class InGameCamera : MonoBehaviour
{
    private InGameBase CurInGameBase;

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

    public void Init()
    {
        CurInGameBase = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>();


    }

    
    private void Update()
    {
        if(!IsFocus) return;

        if(CurInGameBase == null) return;

        if(CurInGameBase.StageMap == null) return;

        transform.position = CurInGameBase.StageMap.Player.transform.position + Offset;
    }


    public void SetFocus(bool value)
    {
        IsFocus = value;    
    }
}
