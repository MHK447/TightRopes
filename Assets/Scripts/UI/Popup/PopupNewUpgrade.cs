using UnityEngine;
using BanpoFri;
using TMPro;
using DG.Tweening;


[UIPath("UI/Popup/PopupNewUpgrade")]
public class PopupNewUpgrade : UIBase
{
    private int UpgradeIdx = 0;

    [SerializeField]
    private TextMeshProUGUI UpgradeDescText;

    [SerializeField]
    private BalanceUIComponent BalanceUIComponent;

    private UpgradeData UpgradeData;

    public void Set(int upgradeidx)
    {
        UpgradeIdx = upgradeidx;

        UpgradeData = GameRoot.Instance.UserData.Upgradedatas[upgradeidx];

        if (UpgradeData == null)
        {
            Hide();
            return;
        }

        ProjectUtility.SetActiveCheck(BalanceUIComponent.gameObject, false);
        ProjectUtility.SetActiveCheck(UpgradeDescText.gameObject, false);

        UpgradeSet();
    }

    public void UpgradeSet()
    {
        GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().StageMap.SetState(InGameStage.InGameState.Direction);
        switch (UpgradeIdx)
        {
            case (int)UpgradeSystem.UpgradeType.RopeUpgrade:
                {
                    GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().StageMap.SetState(InGameStage.InGameState.Direction);
                    GameRoot.Instance.UISystem.GetUI<PopupInGameLobby>().Hide();
                    GameRoot.Instance.WaitTimeAndCallback(0.5f, () =>
                    {
                        GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().StageMap.RopeComponent.SetRopeDirection(RopeUpgradeAction);
                    });
                }
                break;
            case (int)UpgradeSystem.UpgradeType.MoneyMultiUpgrade:
                MoneyUpgradeAction();
                break;
            case (int)UpgradeSystem.UpgradeType.ConteringUpgrade:
                GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().StageMap.SetState(InGameStage.InGameState.Direction);
                GameRoot.Instance.UISystem.GetUI<PopupInGameLobby>().Hide();
                GameRoot.Instance.WaitTimeAndCallback(0.5f, () =>
                {
                    BalanceUIComponent.SetDirectionBalanceValue(50f, 15f, BalanceUpgradeAction);
                });
                break;
        }
    }



    public void RopeUpgradeAction()
    {
        // 업그레이드 설명 텍스트 설정
        // 텍스트 활성화 및 스케일 애니메이션
        ProjectUtility.SetActiveCheck(UpgradeDescText.gameObject, true);

        // 초기 스케일을 0으로 설정
        UpgradeDescText.transform.localScale = Vector3.zero;

        UpgradeDescText.text = Tables.Instance.GetTable<Localize>().GetString($"rope_upgrade_desc_{UpgradeData.GetUpgradeOrder}");

        // 스케일 0에서 1로 애니메이션 (0.5초 동안, Ease.OutBack 효과)
        UpgradeDescText.transform.DOScale(Vector3.one, 0.5f)
            .SetEase(Ease.OutBack);

        var playerpos = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().StageMap.Player.transform.position;

        GameRoot.Instance.EffectSystem.MultiPlay<UpgradeEffect>(new Vector3(playerpos.x, playerpos.y + 10, playerpos.z), (effect) =>
        {
            effect.SetAutoRemove(true, 2.5f);
        });


        GameRoot.Instance.WaitTimeAndCallback(3.5f, () =>
        {
            GameRoot.Instance.UISystem.OpenUI<PopupInGameLobby>(popup => popup.Init());
            Hide();
        });
    }


    public void MoneyUpgradeAction()
    {
        var getlobbyui = GameRoot.Instance.UISystem.GetUI<PopupInGameLobby>();

        if (getlobbyui == null)
        {
            return;
        }

        var getupgradecomponent = getlobbyui.GetLobbyUpgradeComponent((int)UpgradeSystem.UpgradeType.MoneyMultiUpgrade);

        var getupgradevalue = Tables.Instance.GetTable<UpgradelevelOrder>().GetData(UpgradeData.GetUpgradeOrder).value * 0.01f;

        GameRoot.Instance.EffectSystem.MultiPlay<TextEffectMoneyUpgrade>(new Vector3(getupgradecomponent.transform.position.x, getupgradecomponent.transform.position.y + 30, getupgradecomponent.transform.position.z), (effect) =>
      {
          effect.Set($"x{getupgradevalue}", getupgradecomponent.InComeMultiTr, () =>
          {
              Hide();
              getupgradecomponent.InComeUpgradeAction();
              effect.SetAutoRemove(true, 2.5f);
          });


      });
    }

    public void BalanceUpgradeAction()
    {
        ProjectUtility.SetActiveCheck(UpgradeDescText.gameObject, true);

        // 초기 스케일을 0으로 설정
        UpgradeDescText.transform.localScale = Vector3.zero;

        UpgradeDescText.text = Tables.Instance.GetTable<Localize>().GetString($"balance_upgrade_desc_{UpgradeData.GetUpgradeOrder}");

        // 스케일 0에서 1로 애니메이션 (0.5초 동안, Ease.OutBack 효과)
        UpgradeDescText.transform.DOScale(Vector3.one, 0.5f)
            .SetEase(Ease.OutBack);

        var playerpos = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().StageMap.Player.transform.position;

        GameRoot.Instance.EffectSystem.MultiPlay<UpgradeEffect>(new Vector3(playerpos.x, playerpos.y + 10, playerpos.z), (effect) =>
        {
            effect.SetAutoRemove(true, 2.5f);
        });


        GameRoot.Instance.WaitTimeAndCallback(3.5f, () =>
        {
            GameRoot.Instance.UISystem.OpenUI<PopupInGameLobby>(popup => popup.Init());
            Hide();
        });

    }


    public override void Hide()
    {
        base.Hide();

        GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().StageMap.SetState(InGameStage.InGameState.WaitPlay);
    }




}
