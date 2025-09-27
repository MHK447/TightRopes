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




    private int RaceStreet = 0;

    private StageInfoData InfoData;
    
    private CompositeDisposable disposables = new CompositeDisposable();

    void OnEnable()
    {
        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;
        InfoData = Tables.Instance.GetTable<StageInfo>().GetData(stageidx);

        disposables.Clear();

        GameRoot.Instance.UserData.RaceData.RaceStreetProeprty.Subscribe(RaceStatusCheck).AddTo(disposables);
        
    }

    public void RaceStatusCheck(float value)
    {
        if(InfoData == null) return;


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
