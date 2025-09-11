using UnityEngine;

public class InGameCamera : MonoBehaviour
{
    [SerializeField]
    private InGamePlayer InGamePlayer;

    [SerializeField]
    private Vector3 Offset;
    private void Update()
    {
        transform.position = InGamePlayer.transform.position + Offset;
    }
}
