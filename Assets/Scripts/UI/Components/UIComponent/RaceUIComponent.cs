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


    void OnEnable()
    {
        //var stageidx = GameRoot.Instance.UserData.Stageidx.Value;
        //var td = Tables.Instance.GetTable<StageInfo>().GetData()
        
    }



    public void RaceStatusCheck(int value)
    {
        //RaceSlider.value = (float)value / (float);
        RaceStreetText.text = $"{value}m";
    }
    
}
