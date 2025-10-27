using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BanpoFri;
using DG.Tweening;
using System.Numerics;

[UIPath("UI/Page/PageStageClearReward")]
public class PageStageClearReward : UIBase
{
    [SerializeField]
    private TextMeshProUGUI DistanceText;

    [SerializeField]
    private TextMeshProUGUI RewardText;

    [SerializeField]
    private Button RewardBtn;

    [SerializeField]    
    private Button AdRewardBtn;

    private BigInteger RewardValue = 0;


    protected override void Awake()
    {
        base.Awake();
        RewardBtn.onClick.AddListener(OnRewardBtnClick);
        AdRewardBtn.onClick.AddListener(OnAdRewardBtnClick);
    }

    public void Set(int distance, BigInteger reward)
    {
        DistanceText.text = $"{distance}m";

        RewardValue = reward;

        RewardText.text = ProjectUtility.CalculateMoneyToString(reward);
    }

    private void OnRewardBtnClick()
    {

        Hide();
        ProjectUtility.SetRewardAndEffect((int)Config.RewardType.Currency, (int)Config.CurrencyID.Money, RewardValue, () =>
      {
          GameRoot.Instance.WaitTimeAndCallback(1f, () =>
          {
              GameRoot.Instance.UISystem.OpenUI<PageFade>(popup => popup.Set(() =>
                      {
                          GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().StageMap.ReadyPlayingGame();
                      }));
          });
      });
    }

    private void OnAdRewardBtnClick()
    {
        GameRoot.Instance.GetAdManager.ShowRewardedAd(() =>
        {

            Hide();
            ProjectUtility.SetRewardAndEffect((int)Config.RewardType.Currency, (int)Config.CurrencyID.Money, RewardValue * 3, () =>
         {
             GameRoot.Instance.WaitTimeAndCallback(1f, () =>
             {
                 GameRoot.Instance.UISystem.OpenUI<PageFade>(popup => popup.Set(() =>
                      {
                          GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().StageMap.ReadyPlayingGame();
                      }));
             });
         });
        });
    }


    public override void Hide()
    {
        base.Hide();

    }
}
