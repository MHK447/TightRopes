using UnityEngine;

public class InGamePlayer : MonoBehaviour
{
    [SerializeField]
    private Rigidbody Rb;

    [SerializeField]
    private float forwardSpeed = 5f;

    [SerializeField]
    private Animator Anim;



    private InGameBase InGameBase;

    [HideInInspector]
    public bool IsDead = false;


    private float RandBanlanceTime = 0.1f;

    private float BanlanceDeltime = 0f;




    public void Init()
    {
        if (Rb == null)
            Rb = GetComponent<Rigidbody>();


        InGameBase = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>();


        ReadyPlayr();
    }



    public void ReadyPlayr()
    {
        Rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
        Anim.Play("Idle");
        IsDead = false;
        this.transform.position = InGameBase.StartTr.position;
        transform.rotation = Quaternion.Euler(0f, -180f, 0f);



    }


    public void PlayGame()
    {
        Anim.Play("Walk");
        IsDead = false;
    }


    void Update()
    {

        if (InGameBase.CurState != InGameBase.InGameState.Playing) return;

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

    private void ApplySwingMovement()
    {
        BanlanceDeltime += Time.deltaTime;

        if (BanlanceDeltime >= RandBanlanceTime)
        {
            BanlanceDeltime = 0f;
            // 랜덤 목표 각도 갱신 (조금씩 누적 흔들림)
            randomZ += Random.Range(-20, 20);
            randomZ = Mathf.Clamp(randomZ, -20f, 20f); // 너무 과하게 안 흔들리도록 제한
        }

        // 최종 목표 각도 = 랜덤 흔들림 + 입력 보정
        targetZ = randomZ + inputZ;

        // 부드럽게 회전 적용
        float smoothZ = Mathf.LerpAngle(Rb.rotation.eulerAngles.z, targetZ, Time.fixedDeltaTime * rotateSpeed);
        Quaternion targetRot = Quaternion.Euler(0f, -180f, smoothZ);
        Rb.MoveRotation(targetRot);
    }

    public void InputBalance()
    {
        // A, D 입력 반영
        if (Input.GetKey(KeyCode.A))
        {
            inputZ += 2f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            inputZ -= 2f;
        }
    }


    private void ApplyForwardMovement()
    {
        if (IsDead) return;
        if (InGameBase == null) return;
        if (InGameBase.CurState != InGameBase.InGameState.Playing) return;


        Vector3 velocity = Rb.linearVelocity; // 현재 속도 유지
        velocity = transform.forward * forwardSpeed + Vector3.up * velocity.y;
        Rb.linearVelocity = velocity;
    }


    public void ResetPlayer()
    {
    }

    private void CheckTiltLimit()
    {
        if (IsDead) return;

        // 현재 z축 회전값 (0~360 → -180~180으로 변환)
        float zRot = transform.eulerAngles.z;
        if (zRot > 180f) zRot -= 360f;

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

        if (!IsDead && InGameBase.CurState == InGameBase.InGameState.Playing)
        {
            if (this.transform.position.y < InGameBase.DeadYPos)
            {
                randomZ = 0f;
                inputZ = 0f;
                IsDead = true;
                Rb.linearVelocity = Vector3.zero;  // 이동 속도 초기화
                Rb.angularVelocity = Vector3.zero; // 회전 속도 초기화 
                transform.rotation = Quaternion.Euler(0f, -180f, 0f);
                InGameBase.GetMainCam.SetFocus(false);
                InGameBase.EndGame();
            }
        }
    }
}
