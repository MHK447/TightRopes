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

    private Vector3 fixedBehindDirection; // 고정된 뒤쪽 방향
    private bool isDirectionSet = false;  // 방향이 설정되었는지 확인

    void Awake()
    {
        IsFocus = true;
    }

    public void Init()
    {
        CurInGameBase = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>();

        // 방향 초기화 (새 스테이지 시작 시)
        isDirectionSet = false;

        // 카메라 위치 초기화
        ResetCameraPosition();
    }

    private void ResetCameraPosition()
    {
        // 스테이지와 플레이어가 준비될 때까지 기다렸다가 위치 초기화
        if (CurInGameBase?.StageMap?.StartTr != null && CurInGameBase.StageMap.Player != null)
        {
            Vector3 playerStartPosition = CurInGameBase.StageMap.Player.transform.position;

            // 기본 오프셋을 적용한 초기 위치로 설정
            transform.position = playerStartPosition + Offset;

            // 플레이어를 바라보도록 회전 설정
            transform.LookAt(playerStartPosition + Vector3.up * 1f);


            // StartTr에서 EndTr로의 방향을 기준으로 뒤쪽 방향 계산
            Vector3 forwardDirection = (CurInGameBase.StageMap.EndTr.position - CurInGameBase.StageMap.StartTr.position).normalized;
            forwardDirection.y = 0; // Y축 제거
            fixedBehindDirection = -forwardDirection; // 뒤쪽 방향
            isDirectionSet = true;
        }
    }


    private void Update()
    {
        if (!IsFocus) return;

        if (CurInGameBase == null) return;

        if (CurInGameBase.StageMap == null) return;

        Transform playerTransform = CurInGameBase.StageMap.Player.transform;

        // 처음 한 번만 방향 계산
        if (!isDirectionSet && CurInGameBase.StageMap.EndTr != null)
        {
            // 카메라 위치가 초기화되지 않았다면 다시 시도
            ResetCameraPosition();

            // StartTr에서 EndTr로의 방향을 기준으로 뒤쪽 방향 계산
            Vector3 forwardDirection = (CurInGameBase.StageMap.EndTr.position - CurInGameBase.StageMap.StartTr.position).normalized;
            forwardDirection.y = 0; // Y축 제거
            fixedBehindDirection = -forwardDirection; // 뒤쪽 방향
            isDirectionSet = true;
        }

        if (isDirectionSet)
        {
            // 고정된 뒤쪽 방향으로 카메라 위치 계산
            Vector3 behindPlayer = fixedBehindDirection * Mathf.Abs(Offset.z);
            Vector3 rightDirection = Vector3.Cross(Vector3.up, fixedBehindDirection).normalized;

            // 카메라 위치 = 플레이어 위치 + 뒤쪽 오프셋 + 높이 오프셋 + 좌우 오프셋
            Vector3 targetPosition = playerTransform.position + behindPlayer + Vector3.up * Offset.y + rightDirection * Offset.x;

            transform.position = targetPosition;

            // 카메라가 플레이어를 바라보도록 회전
            transform.LookAt(playerTransform.position + Vector3.up * 1f); // 플레이어보다 약간 위를 바라봄
        }
    }


    public void SetFocus(bool value)
    {
        IsFocus = value;
    }
}
