using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using BanpoFri;


public class RaceUIComponent : MonoBehaviour
{
    [SerializeField]
    private Slider RaceSlider;

    [SerializeField]
    private TextMeshProUGUI RaceStreetText;

    private int RaceStreet = 0;

    private StageInfoData InfoData;
    
    private CompositeDisposable disposables = new CompositeDisposable();

    void OnEnable()
    {
        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;
        InfoData = Tables.Instance.GetTable<StageInfo>().GetData(stageidx);

        disposables.Clear();

        GameRoot.Instance.UserData.RaceData.RaceStreetProeprty.Subscribe(x=> {
            RaceStatusCheck((int)x);
        }).AddTo(disposables);

        
    }



    public void RaceStatusCheck(int value)
    {
        RaceSlider.value = (float)value / (float)InfoData.end_goal_value;
        RaceStreetText.text = $"{value}m";
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
