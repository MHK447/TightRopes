using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;


[UIPath("UI/Popup/PopupInGame")]
public class PopupInGame : UIBase
{
    [SerializeField]
    private RaceUIComponent RaceUIComponent; 

    [SerializeField]
    private BalanceUIComponent BalanceUIComponent;

    [SerializeField]
    private BoosterComponent BoosterComponent;

    [SerializeField]
    private Image LeftArrowImg;

    [SerializeField]
    private Image RightArrowImg;

    // 애니메이션 중복 실행 방지를 위한 플래그
    private bool isLeftArrowAnimating = false;
    private bool isRightArrowAnimating = false;

    public void Init()
    {
        BoosterComponent.Init();
        BalanceUIComponent.Init();
        RaceUIComponent.Init();
        
        
        // 화살표 초기 스케일 설정
        LeftArrowImg.transform.localScale = Vector3.zero;
        RightArrowImg.transform.localScale = Vector3.zero; // Right는 X축 반전

        
    }


    public void StageEnd()
    {
        ProjectUtility.SetActiveCheck(BoosterComponent.gameObject , false);
        ProjectUtility.SetActiveCheck(BalanceUIComponent.gameObject , false);
        ProjectUtility.SetActiveCheck(RaceUIComponent.gameObject , false);
    }


    public void ArrowClick(bool isleft)
    {
        // 애니메이션 중복 실행 방지
        if (isleft && isLeftArrowAnimating) return;
        if (!isleft && isRightArrowAnimating) return;
        
        Image targetArrow = isleft ? LeftArrowImg : RightArrowImg;
        
        // 애니메이션 플래그 설정
        if (isleft) isLeftArrowAnimating = true;
        else isRightArrowAnimating = true;
        
        // 기존 애니메이션이 있다면 완전히 중단하고 초기화
        targetArrow.transform.DOKill(true);
        targetArrow.DOKill(true);
        DOTween.Kill(targetArrow);
        DOTween.Kill(targetArrow.transform);
        
        // 초기 상태 강제 설정
        targetArrow.color = new Color(1, 1, 1, 1);
        Vector3 initialScale = isleft ? Vector3.one : new Vector3(-1, 1, 1);    
        targetArrow.transform.localScale = initialScale;
        
        // 애니메이션 시퀀스 생성
        Sequence arrowSequence = DOTween.Sequence();
        
        // 알파값: 서서히 사라지기
        arrowSequence.Join(targetArrow.DOFade(0f, 0.25f).SetEase(Ease.InOutQuad).SetDelay(0.1f));
        
        // 애니메이션 완료 후 정리
        arrowSequence.OnComplete(() => {
            targetArrow.transform.localScale = Vector3.zero;
            targetArrow.color = new Color(1, 1, 1, 0); // 알파값도 초기화
            
            // 애니메이션 플래그 해제
            if (isleft) isLeftArrowAnimating = false;
            else isRightArrowAnimating = false;
        });
        
        // 애니메이션이 중간에 중단될 경우를 대비한 안전장치
        arrowSequence.OnKill(() => {
            if (isleft) isLeftArrowAnimating = false;
            else isRightArrowAnimating = false;
        });
    }






}
