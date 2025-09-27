using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using UniRx;


public class BalanceUIComponent : MonoBehaviour
{
    [SerializeField]
    private Image LeftDangerImg;

    [SerializeField]
    private Image RightDangeImg;


    [SerializeField]
    private Slider CurPosSlider;

    private CompositeDisposable disposables = new CompositeDisposable();

    void OnEnable()
    {
        Init();
    }

    public void Init()
    {
        disposables.Clear();
        
        GameRoot.Instance.UserData.RaceData.BalanceValueProperty.Subscribe(StatusSliderCheck).AddTo(disposables);
    }

    public void StatusSliderCheck(float value)
    {
        CurPosSlider.value = value;
    }

    void OnDisable()
    {
        disposables.Clear();
    }
}
