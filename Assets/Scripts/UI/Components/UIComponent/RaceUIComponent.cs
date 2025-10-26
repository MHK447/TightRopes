using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using BanpoFri;
using DG.Tweening;


public class RaceUIComponent : MonoBehaviour
{
    [SerializeField]
    private Slider GoalSlider;

    [SerializeField]
    private TextMeshProUGUI RaceGoalText;

    [SerializeField]
    private TextMeshProUGUI CurRaceText;

    [SerializeField]
    private Image StageProductImg;

    private int RaceStreet = 0;

    private StageInfoData InfoData;

    private CompositeDisposable disposables = new CompositeDisposable();
    
    private Tween sliderTween;
    private float currentDisplayValue = 0f;

    public void Init()
    {
        ProjectUtility.SetActiveCheck(this.gameObject, true);

        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;
        InfoData = Tables.Instance.GetTable<StageInfo>().GetData(stageidx);

        disposables.Clear();

        RaceGoalText.text = $"{InfoData.end_goal_value}m";

        GameRoot.Instance.UserData.RaceData.RaceProductCount.Subscribe(RaceStatusCheck).AddTo(disposables);


        StageProductImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, InfoData.product_img);
    }

    public void RaceStatusCheck(float value)
    {
        if (InfoData == null) return;

        var count = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().StageMap.ProductEntityCount;
        var targetSliderValue = (float)value / (float)count;

        // 기존 트윈이 있다면 중단
        if (sliderTween != null && sliderTween.IsActive())
        {
            sliderTween.Kill();
        }

        // 슬라이더와 텍스트를 부드럽게 애니메이션
        sliderTween = DOTween.To(() => currentDisplayValue, x => {
            currentDisplayValue = x;
            GoalSlider.value = currentDisplayValue / count;
            CurRaceText.text = $"{currentDisplayValue.ToString("F0")}m";
        }, value, 0.5f).SetEase(Ease.OutQuart);
    }

    void OnDisable()
    {
        disposables.Clear();
        
        // 트윈 정리
        if (sliderTween != null && sliderTween.IsActive())
        {
            sliderTween.Kill();
        }
    }

    void OnDestroy()
    {
        disposables.Clear();
        
        // 트윈 정리
        if (sliderTween != null && sliderTween.IsActive())
        {
            sliderTween.Kill();
        }
    }

}
