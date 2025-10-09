using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using DG.Tweening;
public class InGamePlayer : MonoBehaviour
{
    [SerializeField]
    private Rigidbody Rb;

    [SerializeField]
    private float forwardSpeed = 5f;

    [SerializeField]
    private Animator Anim;

    [SerializeField]
    private List<GameObject> ProductItemList = new List<GameObject>();



    private InGameBase InGameBase;

    [HideInInspector]
    public bool IsDead = false;
    [HideInInspector]
    public bool IsDeadWait = false;


    private float RandBanlanceTime = 0.1f;

    private float BanlanceDeltime = 0f;

    [HideInInspector]
    public int GoalStreet = 0;


    [Header("Product Item")]
    private int ProductItemCount = 0;



    [Header("레이스 이동 계산 변수들")]
    private Vector3 lastPosition;
    private float totalDistance = 0f;
    private float distanceUpdateTimer = 0f;
    private float distanceUpdateInterval = 0.1f; // 1초마다 업데이트

    public void Init()
    {
        if (Rb == null)
            Rb = GetComponent<Rigidbody>();


        InGameBase = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>();


        ReadyPlayr();

    }



    public void ReadyPlayr()
    {
        lastPosition = this.transform.position;
        totalDistance = 0f;
        Rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
        Anim.Play("Idle");
        IsDead = false;
        IsDeadWait = false;
        this.transform.position = InGameBase.StageMap.StartTr.position;

        foreach (var product in ProductItemList)
        {
            ProjectUtility.SetActiveCheck(product, false);
        }
        ProductItemCount = 0;

        SetProductItem(ProductItemCount);

        // EndTr 방향을 바라보도록 회전 설정
        Vector3 directionToEnd = (InGameBase.StageMap.EndTr.position - transform.position).normalized;
        directionToEnd.y = 0; // Y축 회전 제거 (수평 회전만)
        if (directionToEnd != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(directionToEnd);
        }

        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;

        GoalStreet = Tables.Instance.GetTable<StageInfo>().GetData(stageidx).end_goal_value;

        // 거리 추적 초기화
        lastPosition = this.transform.position;
        totalDistance = 0f;
        distanceUpdateTimer = 0f;
        GameRoot.Instance.UserData.RaceData.DataClear(); // RaceStreetProperty 초기화

        // 방향 기울기 변수 초기화
        directionTimer = 0f;
        currentDirection = Random.Range(0, 2) == 0 ? -1 : 1; // 시작 시 랜덤 방향 선택
        randomZ = 0f;
        inputZ = 0f;
    }


    public void StageClearEnd()
    {
        Anim.Play("Idle");
        IsDead = true;
    }

    public void PlayGame()
    {
        Anim.Play("Walk");
        IsDead = false;
        IsDeadWait = false;
    }


    void Update()
    {
        if (InGameBase == null) return;
        if (InGameBase.StageMap.CurState != InGameStage.InGameState.Playing) return;

        InputBalance();
        ApplyForwardMovement();
        ApplySwingMovement();
        DeadCheck();
        CheckTiltLimit();
    }

    private float targetZ = 0f;
    private float rotateSpeed = 5f;

    private float randomZ = 0f;       // 랜덤 흔들림 각도
    private float inputZ = 0f;        // 입력 보정 각도
    private float inputTiltAmount = 2f;

    // 3초 주기 방향 기울기 변수들
    private float directionTimer = 0f;    // 방향 타이머
    private float directionDuration = 3f; // 3초 주기
    private int currentDirection = 0;     // 현재 방향 (-1: 왼쪽, 1: 오른쪽, 0: 중앙)
    private float directionTiltAngle = 15f; // 방향별 기울기 각도

    private void ApplySwingMovement()
    {
        // 3초 주기 방향 타이머 업데이트
        directionTimer += Time.deltaTime;

        if (directionTimer >= directionDuration)
        {
            directionTimer = 0f;

            // StartTr과 EndTr의 위치 관계를 기반으로 방향 결정
            Vector3 startToEnd = (InGameBase.StageMap.EndTr.position - InGameBase.StageMap.StartTr.position).normalized;
            Vector3 playerToEnd = (InGameBase.StageMap.EndTr.position - transform.position).normalized;

            // Cross product를 사용하여 플레이어가 목표 방향의 왼쪽/오른쪽에 있는지 판단
            Vector3 cross = Vector3.Cross(startToEnd, playerToEnd);

            // Y축 기준으로 방향 결정 (+ = 오른쪽으로 기울어야 함, - = 왼쪽으로 기울어야 함)
            currentDirection = cross.y > 0 ? 1 : -1;

            Debug.Log($"StartTr-EndTr 기반 방향 선택: {(currentDirection == -1 ? "왼쪽" : "오른쪽")}, Cross.y: {cross.y}");
        }

        // 기존 미세 흔들림 로직 (더 작은 범위로 조정)
        BanlanceDeltime += Time.deltaTime;

        if (BanlanceDeltime >= RandBanlanceTime)
        {
            BanlanceDeltime = 0f;
            randomZ += currentDirection == -1 ? -1f : 1f;
        }


        // 최종 목표 각도 = 방향 기울기 + 미세 흔들림 + 입력 보정
        targetZ = randomZ + inputZ;

        // EndTr 방향을 기준으로 회전 계산
        Vector3 directionToEnd = (InGameBase.StageMap.EndTr.position - transform.position).normalized;
        directionToEnd.y = 0; // Y축 회전 제거 (수평 회전만)

        float baseYRotation = 0f;
        if (directionToEnd != Vector3.zero)
        {
            baseYRotation = Quaternion.LookRotation(directionToEnd).eulerAngles.y;
        }

        // 부드럽게 회전 적용
        float smoothZ = Mathf.LerpAngle(Rb.rotation.eulerAngles.z, targetZ, Time.fixedDeltaTime * rotateSpeed);
        Quaternion targetRot = Quaternion.Euler(0f, baseYRotation, smoothZ);
        Rb.MoveRotation(targetRot);
    }

    public void InputBalance()
    {
        // A, D 입력 반영
        if (Input.GetKey(KeyCode.A))
        {
            inputZ += 1f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            inputZ -= 1f;
        }
    }


    private void ApplyForwardMovement()
    {
        if (IsDead) return;
        if (InGameBase == null) return;
        if (InGameBase.StageMap.CurState != InGameStage.InGameState.Playing) return;


        RaceCalcUpdate();

        // EndTr 방향으로 이동하도록 변경
        Vector3 directionToEnd = (InGameBase.StageMap.EndTr.position - transform.position).normalized;
        directionToEnd.y = 0; // Y축 이동 제거 (수평 이동만)

        Vector3 velocity = Rb.linearVelocity; // 현재 속도 유지
        velocity = directionToEnd * forwardSpeed + Vector3.up * velocity.y;
        Rb.linearVelocity = velocity;
    }

    public void RaceCalcUpdate()
    {
        if (IsDeadWait || IsDead) return;
        // 1초마다 거리 계산 및 업데이트
        distanceUpdateTimer += Time.deltaTime;

        if (distanceUpdateTimer >= distanceUpdateInterval)
        {
            Vector3 currentPosition = this.transform.position;
            float deltaDistance = Vector3.Distance(currentPosition, lastPosition);

            // EndTr 방향으로만 이동하는 경우만 거리에 추가 (뒤로 가는 것은 제외)
            Vector3 moveDirection = (currentPosition - lastPosition).normalized;
            Vector3 endDirection = (InGameBase.StageMap.EndTr.position - transform.position).normalized;
            endDirection.y = 0; // Y축 제거
            float forwardDot = Vector3.Dot(moveDirection, endDirection);

            if (forwardDot > 0) // 앞으로 이동하는 경우만
            {
                totalDistance += deltaDistance;
                GameRoot.Instance.UserData.RaceData.RaceStreetProeprty.Value = totalDistance;
            }

            lastPosition = currentPosition;
            distanceUpdateTimer = 0f; // 타이머 리셋
        }
    }






    private void CheckTiltLimit()
    {
        if (IsDead) return;

        // 현재 z축 회전값 (0~360 → -180~180으로 변환)
        float zRot = transform.eulerAngles.z;
        if (zRot > 180f) zRot -= 360f;

        // 변환된 값을 BalanceValueProperty에 전달
        GameRoot.Instance.UserData.RaceData.BalanceValueProperty.Value = zRot;

        // 범위 체크
        if (zRot <= -30f || zRot >= 30f)
        {
            var dir = zRot > 0 ? Vector3.right : Vector3.left;
            OnTiltLimitReached(dir);
        }
    }


    // 호출할 함수
    private void OnTiltLimitReached(Vector3 dir)
    {
        Debug.Log("좌우로 너무 기울어짐!");
        IsDeadWait = true;

        // Rigidbody 제약 다 해제
        Rb.constraints = RigidbodyConstraints.None;

        // 위로 + 뒤로 큰 힘을 가해서 튕겨나가게
        Vector3 bounceDir = dir;
        float bouncePower = 20f; // 원하는 튕김 세기 (값 조절 가능)

        Rb.AddForce(bounceDir * bouncePower, ForceMode.Impulse);
    }

    public void DeadCheck()
    {
        if (InGameBase == null) return;

        if (!IsDead && InGameBase.StageMap.CurState == InGameStage.InGameState.Playing)
        {
            if (this.transform.position.y < InGameBase.StageMap.DeadYPos)
            {
                HighScoreCheck();
            }
        }
    }


    public void EndGameClear()
    {
        ProductItemCount = 0;
        SetProductItem(ProductItemCount);
        randomZ = 0f;
        inputZ = 0f;
        IsDead = true;
        Rb.linearVelocity = Vector3.zero;  // 이동 속도 초기화
        Rb.angularVelocity = Vector3.zero; // 회전 속도 초기화 
        InGameBase.GetMainCam.SetFocus(false);
        InGameBase.StageMap.RetryGame();
    }

    public void HighScoreCheck()
    {
        IsDead = true;

        if (GameRoot.Instance.UserData.RaceData.RaceStreetProeprty.Value > GameRoot.Instance.UserData.Highscorevalue)
        {
            GameRoot.Instance.UserData.Highscorevalue = (int)GameRoot.Instance.UserData.RaceData.RaceStreetProeprty.Value;
            GameRoot.Instance.UISystem.OpenUI<PopupNewRecord>(null, EndGameClear);
        }
        else
        {
            EndGameClear();
        }

    }


    public void AddProductItem()
    {
        ProductItemCount++;
        SetProductItem(ProductItemCount);
    }


    public void SetProductItem(int idx)
    {
        ProjectUtility.SetActiveCheck(ProductItemList[idx], true);
        ProductItemList[idx].transform.localScale = Vector3.zero;
        ProductItemList[idx].transform.DOScale(0.3f, 0.3f).SetEase(Ease.OutBack);
    }
}
