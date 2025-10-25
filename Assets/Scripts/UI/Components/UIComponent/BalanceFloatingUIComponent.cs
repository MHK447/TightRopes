using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;

[UIPath("UI/InGame/BalanceFloatingUIComponent", false)]
public class BalanceFloatingUIComponent : InGameFloatingUI
{
    [SerializeField]
    private Image LeftDangerImg;

    [SerializeField]
    private Image RightDangeImg;


    [SerializeField]
    private Slider CurPosSlider;

    private CompositeDisposable disposables = new CompositeDisposable();

    public override void Init(Transform parent)
    {
        base.Init(parent);
        disposables.Clear();

        GameRoot.Instance.UserData.RaceData.BalanceValueProperty.Subscribe(StatusSliderCheck).AddTo(disposables);


        SetBalanceValue((int)GameRoot.Instance.UpgradeSystem.BalanceUpgradeValue(GameRoot.Instance.UserData.Upgradedatas[(int)UpgradeSystem.UpgradeType.BalanceUpgrade].GetUpgradeOrder));
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


    public void SetBalanceValue(int anglevalue)
    {
        float dangevalue = (140 - anglevalue) * 0.01f;

        LeftDangerImg.fillAmount = dangevalue;
        RightDangeImg.fillAmount = dangevalue;

        float slidervalue = anglevalue * 3.25f;

        CurPosSlider.minValue = -slidervalue;
        CurPosSlider.maxValue = slidervalue;

        CurPosSlider.value = 0;


    }

}
