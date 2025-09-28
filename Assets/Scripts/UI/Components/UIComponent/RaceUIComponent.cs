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
    private TextMeshProUGUI CurRaceText;

    private int RaceStreet = 0;

    private StageInfoData InfoData;

    private CompositeDisposable disposables = new CompositeDisposable();

    void OnEnable()
    {
        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;
        InfoData = Tables.Instance.GetTable<StageInfo>().GetData(stageidx);

        disposables.Clear();

        RaceGoalText.text = $"{InfoData.end_goal_value}m";

        GameRoot.Instance.UserData.RaceData.RaceStreetProeprty.Subscribe(RaceStatusCheck).AddTo(disposables);

    }

    public void RaceStatusCheck(float value)
    {
        if (InfoData == null) return;


        GoalSlider.value = (float)value / (float)InfoData.end_goal_value;
        CurRaceText.text = $"{value.ToString("F0")}m";
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
