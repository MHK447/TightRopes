using UnityEngine;
using System.Collections;
using BanpoFri;
using DG.Tweening;
using UniRx;

public class InGameCamera : MonoBehaviour
{
    private InGameBase CurInGameBase;

    [SerializeField]
    private Camera Cam;

    public Camera GetCam { get { return Cam; } }

    [SerializeField]
    private Vector3 Offset;

    [SerializeField]
    private Transform DirectionLightTr;

    private bool IsFocus = true;

    private Vector3 fixedBehindDirection; // 고정된 뒤쪽 방향
    private bool isDirectionSet = false;  // 방향이 설정되었는지 확인

    // 카메라 무브 관련 변수들
    private bool isMoving = false;
    private Vector3 moveStartPosition;
    private Vector3 moveTargetPosition;
    private Quaternion moveStartRotation;
    private Quaternion moveTargetRotation;
    private float moveDuration = 1f;
    private float moveElapsedTime = 0f;

    private float BaseFieldOfView = 50f;

    CompositeDisposable disposables = new CompositeDisposable();
    
    private Vector3 savedDirectionLightRotation; // DirectionLightTr의 목표 rotation 저장


    void Awake()
    {
        IsFocus = true;
    }

    public void Init()
    {
        CurInGameBase = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>();

        // 방향 초기화 (새 스테이지 시작 시)
        isDirectionSet = false;

        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;
        var stageData = Tables.Instance.GetTable<StageInfo>().GetData(stageidx);

        // 카메라 로테이션 설정
        Vector3 cameraRotation = new Vector3(stageData.cam_rot[0], stageData.cam_rot[1], 0);
        
        // DirectionLightTr이 카메라의 자식인지 확인하고 적절한 rotation 설정
        if (DirectionLightTr.parent == transform)
        {
            // 자식 오브젝트인 경우 localRotation 사용
            DirectionLightTr.localRotation = Quaternion.Euler(cameraRotation);
        }
        else
        {
            // 독립적인 오브젝트인 경우 world rotation 사용
            DirectionLightTr.rotation = Quaternion.Euler(cameraRotation);
        }
        
        savedDirectionLightRotation = cameraRotation; // 목표 rotation 저장
        Debug.Log($"DirectionLightTr rotation set to: {cameraRotation}, actual rotation: {DirectionLightTr.rotation.eulerAngles}");
        
        // 카메라 위치 초기화
        ResetCameraPosition();

        disposables.Clear();

        GameRoot.Instance.UserData.RaceData.RaceProductCount.Subscribe(count =>
        {
            SetFieldOfView((int)count);
        }).AddTo(disposables);
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
        // 카메라가 이동 중이면 일반 추적을 중단
        if (isMoving) return;

        if (!IsFocus) return;

        if (CurInGameBase == null) return;

        if (CurInGameBase.StageMap == null) return;

        Transform playerTransform = CurInGameBase.StageMap.Player.transform;

        // 처음 한 번만 방향 계산
        if (!isDirectionSet && CurInGameBase.StageMap.EndTr != null)
        {
            // 카메라 위치가 초기화되지 않았다면 다시 시도
            //ResetCameraPosition();

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

    public void SetFieldOfView(int count, float duration = 0.5f)
    {
        float plusvalue = 2.5f * count;
        float targetFOV = BaseFieldOfView + plusvalue;

        // DOTween을 사용해서 부드럽게 FOV 변경
        Cam.DOFieldOfView(targetFOV, duration).SetEase(Ease.OutQuad);
    }


    public void SetFocus(bool value)
    {
        IsFocus = value;
    }

    /// <summary>
    /// 카메라를 특정 위치로 부드럽게 이동시킵니다
    /// </summary>
    /// <param name="targetPosition">목표 위치</param>
    /// <param name="duration">이동 시간 (초)</param>
    /// <param name="lookAtTarget">바라볼 대상 (null이면 현재 회전 유지)</param>
    public void MoveTo(Vector3 targetPosition, float duration = 1f, Vector3? lookAtTarget = null)
    {
        if (isMoving)
        {
            StopCoroutine(nameof(MoveCoroutine));
        }

        moveStartPosition = transform.position;
        moveTargetPosition = targetPosition;
        moveStartRotation = transform.rotation;

        if (lookAtTarget.HasValue)
        {
            Vector3 direction = (lookAtTarget.Value - targetPosition).normalized;
            moveTargetRotation = Quaternion.LookRotation(direction);
        }
        else
        {
            moveTargetRotation = transform.rotation;
        }

        moveDuration = duration;
        moveElapsedTime = 0f;
        isMoving = true;

        StartCoroutine(MoveCoroutine());
    }

    /// <summary>
    /// 카메라를 특정 위치로 즉시 이동시킵니다
    /// </summary>
    /// <param name="targetPosition">목표 위치</param>
    /// <param name="lookAtTarget">바라볼 대상 (null이면 현재 회전 유지)</param>
    public void MoveToImmediate(Vector3 targetPosition, Vector3? lookAtTarget = null)
    {
        if (isMoving)
        {
            StopCoroutine(nameof(MoveCoroutine));
            isMoving = false;
        }

        transform.position = targetPosition;

        if (lookAtTarget.HasValue)
        {
            transform.LookAt(lookAtTarget.Value);
        }
    }

    /// <summary>
    /// 카메라 이동을 중단하고 일반 추적 모드로 돌아갑니다
    /// </summary>
    public void StopMoving()
    {
        if (isMoving)
        {
            StopCoroutine(nameof(MoveCoroutine));
            isMoving = false;
        }
    }

    /// <summary>
    /// 카메라가 현재 이동 중인지 확인합니다
    /// </summary>
    public bool IsMoving()
    {
        return isMoving;
    }

    private IEnumerator MoveCoroutine()
    {
        while (moveElapsedTime < moveDuration)
        {
            moveElapsedTime += Time.deltaTime;
            float t = moveElapsedTime / moveDuration;

            // Ease-in-out 곡선 적용
            t = t * t * (3f - 2f * t);

            // 위치 보간
            transform.position = Vector3.Lerp(moveStartPosition, moveTargetPosition, t);

            // 회전 보간
            transform.rotation = Quaternion.Lerp(moveStartRotation, moveTargetRotation, t);

            yield return null;
        }

        // 최종 위치와 회전 설정
        transform.position = moveTargetPosition;
        transform.rotation = moveTargetRotation;

        isMoving = false;
    }
}
