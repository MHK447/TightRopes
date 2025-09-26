using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using BanpoFri;


public class RaceUIComponent : MonoBehaviour
{
    [SerializeField]
    private Slider GoalSlider;

    [SerializeField]
    private TextMeshProUGUI RaceGoalText;

    [SerializeField]
    private Slider BalanceSlider;

    private int RaceStreet = 0;

    private StageInfoData InfoData;
    
    private CompositeDisposable disposables = new CompositeDisposable();

    void OnEnable()
    {
        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;
        InfoData = Tables.Instance.GetTable<StageInfo>().GetData(stageidx);

        disposables.Clear();

        GameRoot.Instance.UserData.RaceData.RaceStreetProeprty.Subscribe(RaceStatusCheck).AddTo(disposables);

        GameRoot.Instance.UserData.RaceData.BalanceValueProperty.Subscribe(BalanceStatusCheck).AddTo(disposables);

        
    }

    public void BalanceStatusCheck(float value)
    {
        BalanceSlider.value = value;
    }


    public void RaceStatusCheck(float value)
    {
        GoalSlider.value = (float)value / (float)InfoData.end_goal_value;
        RaceGoalText.text = $"{value}m";
    }

    void OnDisable()
    {
        disposables.Clear();
    }

    void OnDestroy()
    {
        disposables.Clear();
    }

}
