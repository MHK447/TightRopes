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

    private int UpgradeValueUp = 0;

    public void Set(int upgradeidx)
    {
        UpgradeIdx = upgradeidx;

        var upgradedata = GameRoot.Instance.UserData.Upgradedatas[upgradeidx];

        if (upgradedata == null)
        {
            Hide();
            return;
        }

        GameRoot.Instance.UISystem.GetUI<PopupInGameLobby>().Hide();

        UpgradeValueUp = upgradedata.Upgradelevel.Value % 6;

        ProjectUtility.SetActiveCheck(UpgradeDescText.gameObject, false);


        GameRoot.Instance.WaitTimeAndCallback(0.5f, () =>
        {
            GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().StageMap.RopeComponent.SetRopeDirection(AfterAction);
        });
    }



    public void AfterAction()
    {
        // 업그레이드 설명 텍스트 설정
        // 텍스트 활성화 및 스케일 애니메이션
        ProjectUtility.SetActiveCheck(UpgradeDescText.gameObject, true);

        // 초기 스케일을 0으로 설정
        UpgradeDescText.transform.localScale = Vector3.zero;

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
