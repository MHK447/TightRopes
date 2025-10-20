using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using DG.Tweening;
using UnityEditor.SceneManagement;
public class InGamePlayer : MonoBehaviour
{
    [SerializeField]
    private Rigidbody Rb;

    [SerializeField]
    private float forwardSpeed = 5f;

    [SerializeField]
    private Animator Anim;

    [SerializeField]
    private List<PlayerProductComponent> ProductItemList = new List<PlayerProductComponent>();



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


    private BoxCollider Col;

    private Vector3 TutorialDir = Vector3.zero;

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
        BalanceValue = GameRoot.Instance.UpgradeSystem.BalanceUpgradeValue(GameRoot.Instance.UserData.Upgradedatas[(int)UpgradeSystem.UpgradeType.BalanceUpgrade].GetUpgradeOrder);

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
    }


    public void StageClearEnd()
    {
        Anim.Play("Idle");
        IsDead = true;
    }

    public void PlayGame()
    {
        Col.enabled = true;
        Rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
        Anim.Play("Walk");
        IsDead = false;
        IsDeadWait = false;


        SetProductItem(0);
    }


    void Update()
    {
        if (InGameBase == null) return;
        if (InGameBase.StageMap.CurState != InGameStage.InGameState.Playing) return;
        if (IsDead) return;

        InputBalance();
        ApplyForwardMovement();
        ApplySwingMovement();
        //DeadCheck();
        CheckTiltLimit();
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
            
            randomZ += currentDirection == -1 ? -SwayValue : SwayValue;
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
        }
        else if (Input.GetKey(KeyCode.D) && !IsDead && TutorialDir != Vector3.right)
        {
            inputZ += 1f;
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
            }
            else if(TutorialDir != Vector3.right)
            {
                // 오른쪽 터치
                inputZ += 1f;
                Debug.Log("오른쪽 터치 inputZ: " + inputZ);
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

        RaceCalcUpdate();

        // EndTr 방향으로 이동하도록 변경
        Vector3 directionToEnd = (InGameBase.StageMap.EndTr.position - transform.position).normalized;
        directionToEnd.y = 0; // Y축 이동 제거 (수평 이동만)

        // 기본 전진 이동
        Vector3 velocity = Rb.linearVelocity; // 현재 속도 유지
        velocity = directionToEnd * forwardSpeed + Vector3.up * velocity.y;
        Rb.linearVelocity = velocity;
    }

    public void RaceCalcUpdate()
    {
        if (IsDeadWait || IsDead || InGameBase.StageMap.IsTutorialScreen) return;
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
            Rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
            InGameBase.StageMap.IsTutorialScreen = false;
            Anim.Play("Walk");
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

            if (GameRoot.Instance.UserData.Stageidx.Value == 1)
            {
                GameRoot.Instance.UISystem.OpenUI<PageScreenTouch>(popup => popup.Set(dir == Vector3.right), () =>
                {
                    StopPlayer(false, reversedir);
                });
                StopPlayer(true, reversedir);
            }
            else
            {
                OnTiltLimitReached(dir);
            }
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

        foreach (var product in ProductItemList)
        {
            product.EndGame(bounceDir * bouncePower);
        }

        Rb.AddForce(bounceDir * bouncePower, ForceMode.Impulse);

        Anim.Play("Falling", 0, 0f);
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
            GameRoot.Instance.UserData.Highscorevalue = (int)GameRoot.Instance.UserData.RaceData.RaceProductCount.Value;
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


        GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().StageMap.StageClearCheck();
    }


    public void SetProductItem(int idx)
    {
        ProjectUtility.SetActiveCheck(ProductItemList[idx].gameObject, true);
        ProductItemList[idx].Init();
    }
}
