using UnityEngine;
using UnityEngine.UI;
using BanpoFri;

public class BoosterComponent : MonoBehaviour
{   
    [SerializeField]
    private Image BoosterOnImg;

    [SerializeField]
    private Button BoosterBtn;

    private float deltime = 0f;
    private float BoosterTime = 10f;
    private bool isBoosterReady = false;
    private bool isBoosterActive = false;

    private void Awake()
    {
        BoosterBtn.onClick.AddListener(OnClickBoosterBtn);
    }

    public void Init()
    {
        ProjectUtility.SetActiveCheck(this.gameObject, false);

        // 초기 상태 설정
        deltime = 0f;
        isBoosterReady = false;
        isBoosterActive = false;
        
        // 부스터 이미지 fillAmount 초기화 (0으로 시작)
        if (BoosterOnImg != null)
        {
            BoosterOnImg.fillAmount = 0f;
        }
        
        // 버튼 비활성화
        BoosterBtn.interactable = false;

        // ProjectUtility.SetActiveCheck(this.gameObject, 
        // GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.BoosterOpen));
    }

    public void OnClickBoosterBtn()
    {
        if (isBoosterReady && !isBoosterActive)
        {
            // 부스터 활성화
            isBoosterActive = true;
            isBoosterReady = false;
            
            InGameBase InGameBase = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>();
            InGameBase.StageMap.Player.BoosterOn();
            // 다시 충전 시작
            deltime = 0f;
            BoosterBtn.interactable = false;
            
            if (BoosterOnImg != null)
            {
                BoosterOnImg.fillAmount = 0f;
            }
        }
    }

    void Update()
    {
        if (!isBoosterReady)
        {
            // 부스터 충전 중
            deltime += Time.deltaTime;
            
            // fillAmount 업데이트 (0에서 1로 점진적 증가)
            if (BoosterOnImg != null)
            {
                BoosterOnImg.fillAmount = deltime / BoosterTime;
            }

            BoosterOnImg.fillAmount =  (float)deltime / (float)BoosterTime;
            
            // 10초가 되면 부스터 준비 완료
            if (deltime >= BoosterTime)
            {
                isBoosterActive = false;
                isBoosterReady = true;
                BoosterBtn.interactable = true;
                
                // fillAmount 완전히 채우기
                if (BoosterOnImg != null)
                {
                    BoosterOnImg.fillAmount = 1f;
                }
                
                Debug.Log("부스터 준비 완료! 클릭 가능합니다.");
            }
        }
    }
}
