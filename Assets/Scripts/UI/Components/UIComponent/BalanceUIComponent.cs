using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;

public class BalanceUIComponent : MonoBehaviour
{
    [SerializeField]
    private Image LeftDangerImg;

    [SerializeField]
    private Image RightDangeImg;


    [SerializeField]
    private Slider CurPosSlider;

    private CompositeDisposable disposables = new CompositeDisposable();


    public void Init()
    {
        ProjectUtility.SetActiveCheck(this.gameObject , true);

        disposables.Clear();

        GameRoot.Instance.UserData.RaceData.BalanceValueProperty.Subscribe(StatusSliderCheck).AddTo(disposables);


        SetBalanceValue(GameRoot.Instance.UpgradeSystem.BalanceUpgradeValue(GameRoot.Instance.UserData.Upgradedatas[(int)UpgradeSystem.UpgradeType.BalanceUpgrade].GetUpgradeOrder));
    }

    public void StatusSliderCheck(float value)
    {
        CurPosSlider.value = value;
    }

    void OnDisable()
    {
        disposables.Clear();
        LeftDangerImg.DOKill();
        RightDangeImg.DOKill();

    }


    public void SetBalanceValue(float anglevalue)
    {
        float dangevalue = (120 - anglevalue) * 0.01f;

        LeftDangerImg.fillAmount = dangevalue;
        RightDangeImg.fillAmount = dangevalue;

        float slidervalue = anglevalue * 3.25f;

        CurPosSlider.minValue = -slidervalue;
        CurPosSlider.maxValue = slidervalue;

        CurPosSlider.value = 0;
    }


    public void SetDirectionBalanceValue(float targetFillAmount, float beforefillamount, System.Action endaction = null)
    {
        ProjectUtility.SetActiveCheck(this.gameObject, true);

        // 기존 애니메이션 중단
        LeftDangerImg.DOKill();
        RightDangeImg.DOKill();


        this.transform.localScale = Vector3.zero;

        float dangevalue = (100 - targetFillAmount) * 0.01f;
        float beforevalue = (100 - beforefillamount) * 0.01f;




        // 초기 설정
        LeftDangerImg.fillAmount = beforevalue;
        RightDangeImg.fillAmount = beforevalue;
        LeftDangerImg.transform.localScale = Vector3.one;
        RightDangeImg.transform.localScale = Vector3.one;



        this.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            // 스케일 애니메이션 완료 후 0.5초 딜레이
            DOVirtual.DelayedCall(0.5f, () =>
            {
                // 시퀀스 생성
                Sequence leftSequence = DOTween.Sequence();
                Sequence rightSequence = DOTween.Sequence();

                // 1단계: fillAmount 0에서 목표값까지 증가 (0.5초)
                leftSequence.Append(LeftDangerImg.DOFillAmount(dangevalue, 0.5f));
                rightSequence.Append(RightDangeImg.DOFillAmount(dangevalue, 0.5f));

                leftSequence.Play();
                rightSequence.Play();

                leftSequence.OnComplete(() =>
                {
                    var sequence = DOTween.Sequence();
                    sequence.Append(this.transform.DOScale(1.4f, 0.15f).SetEase(Ease.OutQuad));
                    sequence.Append(this.transform.DOScale(1.0f, 0.15f).SetEase(Ease.InOutQuad));
                    sequence.OnComplete(() =>
                    {
                        endaction?.Invoke();
                    });
                });
            });
        });
    }

}
