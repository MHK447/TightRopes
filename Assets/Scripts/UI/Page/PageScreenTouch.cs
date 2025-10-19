using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using DG.Tweening;
using UniRx;
using TMPro;

[UIPath("UI/Page/PageScreenTouch")]

public class PageScreenTouch : UIBase
{
    [SerializeField]
    private GameObject LeftToucObj;

    [SerializeField]
    private GameObject RightTouchObj;

    [SerializeField]
    private TextMeshProUGUI BalanceDescText;

    private Tween textTween;

    private InGameStage InGameStage;
    
    private bool IsLeft = false;
     
     private CompositeDisposable disposables = new CompositeDisposable();

    public void Set(bool left)
    {
        IsLeft = left;

        ProjectUtility.SetActiveCheck(LeftToucObj, !left);
        ProjectUtility.SetActiveCheck(RightTouchObj, left);

        StartTextAnimation();

        InGameStage = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().StageMap;

        disposables.Clear();

        GameRoot.Instance.UserData.RaceData.BalanceValueProperty.Subscribe(RaceDistanceCheck).AddTo(disposables);

        BalanceDescText.text = Tables.Instance.GetTable<Localize>().GetString(left ? "str_touch_hold_right" : "str_touch_hold_left");
    }

    private void StartTextAnimation()
    {
        if (BalanceDescText == null) return;

        // 기존 트윈이 있다면 정리
        textTween?.Kill();

        // 초기 스케일 설정
        BalanceDescText.transform.localScale = Vector3.one;

        // 스케일 애니메이션 (1.0 -> 1.2 -> 1.0 반복)
        // SetUpdate(true)로 타임스케일에 영향받지 않도록 설정
        textTween = BalanceDescText.transform.DOScale(1.2f, 0.8f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);
    }

    private void OnDestroy()
    {
        disposables.Clear();
        textTween?.Kill();
    }

    public void RaceDistanceCheck(float value)
    {
        if(InGameStage == null) return;

        if(IsLeft && value >= -5)
        {
            Hide();
        }
        else if(!IsLeft && value <= 5)
        {
            Hide();
        }
    }


    
}
