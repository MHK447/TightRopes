using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using DG.Tweening;


public class InGamePlayer : MonoBehaviour
{
    [SerializeField]
    private Rigidbody Rb;

    [SerializeField]
    private float forwardSpeed = 6f;

    [SerializeField]
    private Animator Anim;

    [SerializeField]
    private List<PlayerProductComponent> ProductItemList = new List<PlayerProductComponent>();

    [SerializeField]
    private GameObject CycleRoot;



    private InGameBase InGameBase;

    [HideInInspector]
    public bool IsDead = false;
    [HideInInspector]
    public bool IsDeadWait = false;


    private float RandBanlanceTime = 0.1f;

    private float BanlanceDeltime = 0f;

    [HideInInspector]
    public int GoalStreet = 0;




    [Header("레이스 이동 계산 변수들")]
    private Vector3 lastPosition;
    private float totalDistance = 0f;
    private float distanceUpdateTimer = 0f;
    private float distanceUpdateInterval = 0.1f; // 1초마다 업데이트



    [Header("기울기 변수")]
    private float SwayValue = 0;

    private float BalanceValue = 0;

    [Header("부스터 변수")]
    private bool isBoosterActive = false;
    private float boosterDuration = 2f;
    private float boosterTimer = 0f;
    private float boosterHeight = 7f; // 부스터 시 떠오르는 높이
    private float boosterSpeed = 16f; // 부스터 시 이동 속도 증가
    private float minBoosterSpeedHeight = 2f; // 이 높이 이상에서만 부스터 속도 적용
    private Vector3 originalPosition;
    private bool wasConstraintsFrozen = false;
    private bool isGrounded = true; // 땅에 닿아있는지 확인
    private float groundCheckDistance = 0.5f; // 땅 체크 거리 (증가)
    private LayerMask groundLayerMask = -1; // 땅으로 인식할 레이어 (기본값: 모든 레이어)

    private BoxCollider Col;

    private Vector3 TutorialDir = Vector3.zero;

    private float CycleSpeed = 0f;


    private PopupInGame PopupInGame;

    public void Init()
    {
        if (Rb == null)
            Rb = GetComponent<Rigidbody>();

        Col = GetComponent<BoxCollider>();


        InGameBase = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>();



        ReadyPlayr();

    }



    public void ReadyPlayr()
    {

        //스테이지마다 처음에 로프 기울기 및 허들을 정해준다. 


        SwayValue = GameRoot.Instance.UpgradeSystem.RopeUpgradeValue(GameRoot.Instance.UserData.Upgradedatas[(int)UpgradeSystem.UpgradeType.RopeUpgrade].GetUpgradeOrder);


        lastPosition = this.transform.position;
        totalDistance = 0f;
        //Rb.constraints = RigidbodyConstraints.FreezePositionX  | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
        Rb.constraints = RigidbodyConstraints.FreezeAll;
        Anim.Play("Idle");
        IsDead = false;
        IsDeadWait = false;
        this.transform.position = InGameBase.StageMap.StartTr.position;

        foreach (var product in ProductItemList)
        {
            ProjectUtility.SetActiveCheck(product.gameObject, false);
        }

        GameRoot.Instance.UserData.RaceData.RaceProductCount.Value = 0;

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


        CycleAdInit();
    }


    public void CycleAdInit()
    {
        var getcyclecount = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.AdCycleCount);

        CycleSpeed = getcyclecount * 2;

        if(CycleSpeed > 10)
        {
            CycleSpeed = 10;
        }

        ProjectUtility.SetActiveCheck(CycleRoot.gameObject, getcyclecount > 0);
    }


    public void StageClearEnd()
    {
        Anim.Play("Idle");
        IsDead = true;
        
        // 이동 완전히 멈추기
        Rb.linearVelocity = Vector3.zero;  // 이동 속도 초기화
        Rb.angularVelocity = Vector3.zero; // 회전 속도 초기화
        Rb.constraints = RigidbodyConstraints.FreezeAll; // 모든 움직임 고정
    }

    public void PlayGame()
    {
        var finddata = GameRoot.Instance.UserData.Upgradedatas[(int)UpgradeSystem.UpgradeType.BalanceUpgrade];

        var plusvalue = 1 + finddata.GetUpgradeOrder * 0.2f;

        BalanceValue =
         GameRoot.Instance.UpgradeSystem.BalanceUpgradeValue(GameRoot.Instance.UserData.Upgradedatas[(int)UpgradeSystem.UpgradeType.BalanceUpgrade]
         .GetUpgradeOrder) * plusvalue;

        if (BalanceValue >= 50)
        {
            BalanceValue = 83;
        }

        Col.enabled = true;
        Rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
        var aniname = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.AdCycleCount) > 0 ? "Idle" : "Walk";
        Anim.Play(aniname);
        IsDead = false;
        IsDeadWait = false;


        SetProductItem(0);
    }


    void Update()
    {
        if (InGameBase == null) return;
        if (InGameBase.StageMap.CurState != InGameStage.InGameState.Playing) return;
        if (IsDead) return;

        UpdateBooster();
        InputBalance();
        ApplyForwardMovement();
        if (!isBoosterActive) // 부스터 활성화 시에는 흔들림 적용 안함
        {
            ApplySwingMovement();
        }
        //DeadCheck();
        if (!isBoosterActive) // 부스터 활성화 시에는 기울기 체크 안함
        {
            CheckTiltLimit();
        }
    }

    private void UpdateBooster()
    {
        if (!isBoosterActive) return;

        boosterTimer += Time.deltaTime;


        // 땅에 닿았는지 체크 (Raycast 사용)
        CheckGrounded();

        // 땅에 닿으면 부스터 종료
        if (isGrounded && boosterTimer > 0.5f) // 0.5초 후부터 땅 체크 (점프 직후 바로 종료 방지)
        {
            BoosterOff();
            return;
        }
        else if (boosterTimer >= 1.5f)
        {
            BoosterOff();
            return;
        }



        // 부스터 활성화 중에는 공중에서 부드럽게 날아가는 효과
        // 목표 지점으로 향하는 방향 계산
        Vector3 directionToEnd = (InGameBase.StageMap.EndTr.position - transform.position).normalized;
        directionToEnd.y = 0; // Y축 이동 제거 (수평 이동만)

        // 현재 높이 확인 (원래 위치 기준)
        float currentHeight = transform.position.y - originalPosition.y;

        // 높이에 따른 속도 결정
        float currentSpeed;
        if (currentHeight >= minBoosterSpeedHeight)
        {
            // 충분히 높은 곳에 있을 때만 부스터 속도 적용
            currentSpeed = boosterSpeed + CycleSpeed;
        }
        else
        {
            // 낮은 곳에 있거나 내려오는 중일 때는 일반 속도
            currentSpeed = forwardSpeed + CycleSpeed;
        }

        // 계산된 속도로 이동
        Vector3 boosterVelocity = directionToEnd * currentSpeed;

        // Y축 속도는 현재 속도 유지 (중력 영향 받도록)
        boosterVelocity.y = Rb.linearVelocity.y;

        Rb.linearVelocity = boosterVelocity;

        // 부스터 중에는 회전을 목표 방향으로 고정
        if (directionToEnd != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToEnd);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 3f);
        }
    }

    private void CheckGrounded()
    {
        // 플레이어 발 아래로 Raycast를 쏴서 땅 감지
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = Vector3.down;

        // Raycast로 땅 체크 (플레이어 자신의 콜라이더는 제외)
        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, groundCheckDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            // 자기 자신의 콜라이더가 아닌 경우만 땅으로 인식
            if (hit.collider != Col)
            {
                isGrounded = true;
                Debug.DrawRay(rayOrigin, rayDirection * groundCheckDistance, Color.green);
            }
            else
            {
                isGrounded = false;
                Debug.DrawRay(rayOrigin, rayDirection * groundCheckDistance, Color.red);
            }
        }
        else
        {
            isGrounded = false;
            Debug.DrawRay(rayOrigin, rayDirection * groundCheckDistance, Color.red);
        }
    }

    private float targetZ = 0f;
    private float rotateSpeed = 5f;

    private float randomZ = 0f;       // 랜덤 흔들림 각도
    private float inputZ = 0f;        // 입력 보정 각도
    private float inputTiltAmount = 2f;
    private float lastSideOffset = 0f; // 이전 프레임의 사이드 오프셋

    // 3초 주기 방향 기울기 변수들
    private float directionTimer = 0f;    // 방향 타이머
    private float directionDuration = 3f; // 3초 주기
    private int currentDirection = 0;     // 현재 방향 (-1: 왼쪽, 1: 오른쪽, 0: 중앙)
    private float directionTiltAngle = 15f; // 방향별 기울기 각도

    private void ApplySwingMovement()
    {
        // 부스터 활성화 시에는 자동 흔들림 적용 안함
        if (isBoosterActive) return;

        // 3초 주기 방향 타이머 업데이트
        directionTimer += Time.deltaTime;

        if (directionTimer >= directionDuration && !InGameBase.StageMap.IsTutorialScreen)
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

        if (BanlanceDeltime >= RandBanlanceTime && !InGameBase.StageMap.IsTutorialScreen)
        {
            BanlanceDeltime = 0f;

            SwayValue = GameRoot.Instance.UpgradeSystem.RopeUpgradeValue(GameRoot.Instance.UserData.Upgradedatas[(int)UpgradeSystem.UpgradeType.RopeUpgrade].GetUpgradeOrder);
            var getcount = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.FirstSwayAdd);
            // 처음 카운트가 0일 때만 빠르게 기울기
            if (getcount == 0)
            {
                // 첫 번째 기울기는 더 크게 적용
                randomZ += currentDirection == -1 ? -SwayValue * 6f : SwayValue * 6f;
            }
            else
            {
                // 일반적인 기울기 적용
                randomZ += currentDirection == -1 ? -SwayValue : SwayValue;
            }
        }


        targetZ = (randomZ - inputZ);

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


    //터치형
    public void InputBalance()
    {
        if (IsDead) return;

        // A, D 입력 반영 (유니티 에디터용)
        if (Input.GetKey(KeyCode.A) && !IsDead && TutorialDir != Vector3.left)
        {
            inputZ -= 1f;
            GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.ArrowClick(true);
        }
        else if (Input.GetKey(KeyCode.D) && !IsDead && TutorialDir != Vector3.right)
        {
            inputZ += 1f;
            GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.ArrowClick(false);
        }

        // 터치 입력 처리 (모바일용) - A, D 키와 동일하게 계속 누르고 있는 동안 적용
        if (Input.GetMouseButton(0) || Input.touchCount > 0 && !IsDead)
        {
            Vector3 inputPosition = Vector3.zero;

            // 마우스 또는 터치 위치 가져오기
            if (Input.GetMouseButton(0))
            {
                inputPosition = Input.mousePosition;
            }
            else if (Input.touchCount > 0)
            {
                inputPosition = Input.GetTouch(0).position;
            }

            // 화면 중앙을 기준으로 좌우 판단
            float screenCenterX = Screen.width * 0.5f;

            if (inputPosition.x < screenCenterX && TutorialDir != Vector3.left)
            {
                // 왼쪽 터치
                inputZ -= 1f;
                Debug.Log("왼쪽 터치 inputZ: " + inputZ);
                GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.ArrowClick(true);
            }
            else if (TutorialDir != Vector3.right)
            {
                // 오른쪽 터치
                inputZ += 1f;
                Debug.Log("오른쪽 터치 inputZ: " + inputZ);
                GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.ArrowClick(false);
            }
        }
    }

    // 조이스틱 입력을 받는 새로운 메서드 (방향과 세기 적용)
    public void InputBalance(Vector3 inputVector)
    {
        if (IsDead) return;

        // 조이스틱의 X축 입력을 inputZ에 적용 (방향과 세기 모두 반영)
        inputZ += inputVector.x;

        Debug.Log("조이스틱 입력 - inputVector.x: " + inputVector.x + ", inputZ: " + inputZ);
    }


    private void ApplyForwardMovement()
    {
        if (IsDead) return;
        if (InGameBase == null) return;
        if (InGameBase.StageMap.CurState != InGameStage.InGameState.Playing) return;
        if (InGameBase.StageMap.IsTutorialScreen) return;
        if (isBoosterActive) return; // 부스터 활성화 시에는 UpdateBooster에서 이동 처리

        RaceCalcUpdate();

        // EndTr 방향으로 이동하도록 변경
        Vector3 directionToEnd = (InGameBase.StageMap.EndTr.position - transform.position).normalized;
        directionToEnd.y = 0; // Y축 이동 제거 (수평 이동만)

        // 기본 전진 이동
        Vector3 velocity = Rb.linearVelocity; // 현재 속도 유지
        velocity = directionToEnd * (forwardSpeed + CycleSpeed) + Vector3.up * velocity.y;
        Rb.linearVelocity = velocity;
    }

    public void RaceCalcUpdate()
    {
        if (IsDeadWait || IsDead || InGameBase.StageMap.IsTutorialScreen) return;

        // 부스터 활성화 시에도 거리 계산은 계속 진행
        if (isBoosterActive)
        {
            // 부스터 중에는 더 자주 거리 업데이트 (더 빠르게 이동하므로)
            distanceUpdateTimer += Time.deltaTime;

            if (distanceUpdateTimer >= distanceUpdateInterval * 0.5f) // 2배 빠르게 업데이트
            {
                Vector3 currentPosition = this.transform.position;
                float deltaDistance = Vector3.Distance(currentPosition, lastPosition);

                Vector3 moveDirection = (currentPosition - lastPosition).normalized;
                Vector3 endDirection = (InGameBase.StageMap.EndTr.position - transform.position).normalized;
                endDirection.y = 0;
                float forwardDot = Vector3.Dot(moveDirection, endDirection);

                if (forwardDot > 0)
                {
                    totalDistance += deltaDistance;
                    GameRoot.Instance.UserData.RaceData.RaceDistanceProperty.Value = totalDistance;
                }

                lastPosition = currentPosition;
                distanceUpdateTimer = 0f;
            }
            return;
        }
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
                GameRoot.Instance.UserData.RaceData.RaceDistanceProperty.Value = totalDistance;
            }

            lastPosition = currentPosition;
            distanceUpdateTimer = 0f; // 타이머 리셋
        }
    }


    public void StopPlayer(bool value, Vector3 dir)
    {
        if (value)
        {
            Rb.constraints = RigidbodyConstraints.FreezeAll;
            InGameBase.StageMap.IsTutorialScreen = true;
            Anim.Play("Idle");
            TutorialDir = dir;
        }
        else
        {
            var aniname = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.AdCycleCount) > 0 ? "Idle" : "Walk";
            Rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
            InGameBase.StageMap.IsTutorialScreen = false;
            Anim.Play(aniname);
            TutorialDir = Vector3.zero;
        }
    }



    private void CheckTiltLimit()
    {
        if (IsDead) return;

        // 현재 z축 회전값 (0~360 → -180~180으로 변환)
        float zRot = transform.eulerAngles.z;
        if (zRot > 180f) zRot -= 360f;

        // 변환된 값을 BalanceValueProperty에 전달
        GameRoot.Instance.UserData.RaceData.BalanceValueProperty.Value = -zRot;

        // 범위 체크
        if (zRot <= -BalanceValue || zRot >= BalanceValue)
        {
            var dir = zRot > 0 ? Vector3.right : Vector3.left;
            var reversedir = zRot > 0 ? Vector3.left : Vector3.right;

            var getcount = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.TutorialStageCount);

            if (GameRoot.Instance.UserData.Stageidx.Value == 1 && getcount <= 1)
            {
                GameRoot.Instance.UISystem.OpenUI<PageScreenTouch>(popup => popup.Set(dir == Vector3.right), () =>
                {
                    GameRoot.Instance.UserData.AddRecordCount(Config.RecordCountKeys.FirstSwayAdd, 1);
                    GameRoot.Instance.UserData.AddRecordCount(Config.RecordCountKeys.TutorialStageCount, 1);
                    StopPlayer(false, reversedir);
                });
                StopPlayer(true, reversedir);
            }
            else
            {
                OnTiltLimitReached(GameRoot.Instance.UserData.Stageidx.Value == 3 ? dir : reversedir);
            }
        }
    }


    public void BoosterOn()
    {
        if (isBoosterActive || IsDead) return;

        Debug.Log("부스터 활성화!");

        Anim.SetBool("Jump", true);

        GameRoot.Instance.EffectSystem.MultiPlay<FireWorkEffect>
        (new Vector3(this.transform.position.x, this.transform.position.y - 1, this.transform.position.z),
         (effect) =>
        {
            effect.SetAutoRemove(true, 1f);
        });

        // 부스터 상태 활성화
        isBoosterActive = true;
        boosterTimer = 0f;
        isGrounded = false; // 부스터 시작 시 공중 상태로 설정

        // 현재 위치 저장
        originalPosition = transform.position;

        // 물리 제약 해제하여 자유롭게 움직일 수 있도록 함
        wasConstraintsFrozen = (Rb.constraints == RigidbodyConstraints.FreezeAll);
        Rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;

        // 위로 떠오르는 힘 적용
        Vector3 upwardForce = Vector3.up * boosterHeight;
        Rb.AddForce(upwardForce, ForceMode.Impulse);

        // 애니메이션을 날아가는 상태로 변경 (있다면)
        if (Anim != null)
        {
            var aniname = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.AdCycleCount) > 0 ? "Idle" : "Walk";
            Anim.Play(aniname); // 또는 부스터 전용 애니메이션이 있다면 그것을 사용
        }
    }

    private void BoosterOff()
    {
        if (!isBoosterActive) return;

        Anim.SetBool("Jump", false);
        // 부스터 상태 비활성화
        isBoosterActive = false;
        boosterTimer = 0f;

        // 원래 물리 제약 복원
        if (wasConstraintsFrozen)
        {
            Rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        else
        {
            Rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
        }

        // 부드럽게 원래 상태로 복귀
        // 현재 속도를 줄여서 자연스럽게 착지하도록 함
        Vector3 currentVelocity = Rb.linearVelocity;
        currentVelocity.x *= 0.5f; // X축 속도 감소
        currentVelocity.z *= 0.5f; // Z축 속도 감소
        Rb.linearVelocity = currentVelocity;

        // 회전값 초기화 (기울기 변수들도 초기화)
        randomZ = 0f;
        inputZ = 0f;

        // 애니메이션을 걷기 상태로 복원
        if (Anim != null)
        {
            var aniname = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.AdCycleCount) > 0 ? "Idle" : "Walk";
            Anim.Play(aniname);
        }
    }




    // 호출할 함수
    private void OnTiltLimitReached(Vector3 dir)
    {


        Debug.Log("좌우로 너무 기울어짐!");
        Col.enabled = false;

        if (!IsDeadWait)
        {
            GameRoot.Instance.WaitTimeAndCallback(2f, () =>
                {
                    HighScoreCheck();
                });
        }

        IsDeadWait = true;

        // Rigidbody 제약 다 해제
        Rb.constraints = RigidbodyConstraints.None;

        // 위로 + 뒤로 큰 힘을 가해서 튕겨나가게
        Vector3 bounceDir = dir;
        float bouncePower = 20f; // 원하는 튕김 세기 (값 조절 가능)

        GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.StageEnd();

        foreach (var product in ProductItemList)
        {
            product.EndGame(bounceDir * bouncePower);
        }

        Rb.AddForce(bounceDir * bouncePower, ForceMode.Impulse);

        Anim.Play("Falling", 0, 0f);
    }


    public void CycleAction()
    {
        CycleAdInit();

        GameRoot.Instance.EffectSystem.MultiPlay<UpgradeEffect>(CycleRoot.transform.position, (effect) =>
        {
            effect.SetAutoRemove(true, 2.5f);
        });


    }


    // public void DeadCheck()
    // {
    //     if (InGameBase == null) return;

    //     if (!IsDead && InGameBase.StageMap.CurState == InGameStage.InGameState.Playing)
    //     {
    //         if (this.transform.position.y < InGameBase.StageMap.DeadYPos)
    //         {
    //             HighScoreCheck();
    //         }
    //     }
    // }


    public void EndGameClear()
    {
        GameRoot.Instance.UserData.RaceData.RaceProductCount.Value = 0;
        randomZ = 0f;
        inputZ = 0f;
        Rb.linearVelocity = Vector3.zero;  // 이동 속도 초기화
        Rb.angularVelocity = Vector3.zero; // 회전 속도 초기화 
        InGameBase.GetMainCam.SetFocus(false);
        InGameBase.StageMap.RetryGame();
        GameRoot.Instance.UserData.RaceData.RaceDistanceProperty.Value = 0;
    }

    public void HighScoreCheck()
    {
        if (IsDead) return;

        IsDead = true;

        if (GameRoot.Instance.UserData.RaceData.RaceProductCount.Value > GameRoot.Instance.UserData.Highscorevalue)
        {
            GameRoot.Instance.UserData.Highscorevalue = (int)GameRoot.Instance.UserData.RaceData.RaceProductCount.Value * 10;
            GameRoot.Instance.UISystem.OpenUI<PopupNewRecord>(null, EndGameClear);
        }
        else
        {
            GameRoot.Instance.WaitTimeAndCallback(0.5f, EndGameClear);
        }

    }


    public void AddProductItem()
    {
        GameRoot.Instance.UserData.RaceData.RaceProductCount.Value++;
        SetProductItem((int)GameRoot.Instance.UserData.RaceData.RaceProductCount.Value);
    }


    public void SetProductItem(int idx)
    {
        ProjectUtility.SetActiveCheck(ProductItemList[idx].gameObject, true);
        ProductItemList[idx].Init();
    }
}
