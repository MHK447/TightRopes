using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BanpoFri;
using UniRx;
using BanpoFri.Data;
using System.Collections.Generic;
using DG.Tweening;



public class LobbyUpgradeComponent : MonoBehaviour
{
    [System.Serializable]
    public enum UpgradeState
    {
        RopeUp,
        Balance,
        InCome,
    }


    private int UpgradeIdx;

    [SerializeField]
    private TextMeshProUGUI LevelText;

    [SerializeField]
    private TextMeshProUGUI UpgradeCostText;

    [SerializeField]
    private TextMeshProUGUI InComeMultiText;


    public Transform InComeMultiTr { get { return InComeMultiText.transform; } }

    [SerializeField]
    private List<Image> UpgradeImgList = new List<Image>();

    [SerializeField]
    private Image UpgradeBtnImg;




    [SerializeField]
    private Image BgImg;

    [SerializeField]
    private UpgradeState State;





    private UpgradeData UpgradeData;



    [SerializeField]
    private Button UpgradeBtn;

    private System.Numerics.BigInteger UpgradeCost;


    private CompositeDisposable disposables = new CompositeDisposable();

    private Color DefaultColor;

    void Awake()
    {
        UpgradeBtn.onClick.AddListener(OnClickUpgradeBtn);

        DefaultColor = UpgradeBtnImg.color;
    }


    public void Set(int upgradeidx)
    {
        UpgradeIdx = upgradeidx;

        UpgradeData = GameRoot.Instance.UserData.Upgradedatas[upgradeidx];

        disposables.Clear();

        UpgradeData.Upgradelevel.Subscribe(x => { SetUpgradeValue(); }).AddTo(disposables);

        GameRoot.Instance.UserData.Money.Subscribe(x => { SetUpgradeValue(); }).AddTo(disposables);

        SetInComeValue();

        SetUpgradeImg();
    }

    void OnDestroy()
    {
        disposables.Clear();
    }

    void OnDisable()
    {
        disposables.Clear();
    }

    public void SetInComeValue()
    {
        if (InComeMultiText == null) return;


        InComeMultiText.text = $"x{GameRoot.Instance.UserData.Incomemultivalue.ToString("0.0")}";
    }


    public void SetUpgradeValue()
    {
        LevelText.text = UpgradeData.Upgradelevel.ToString();
        UpgradeCost = GameRoot.Instance.UpgradeSystem.GetUpgradeCost(UpgradeIdx, UpgradeData.Upgradelevel.Value);

        UpgradeCostText.text = ProjectUtility.CalculateMoneyToString(UpgradeCost);

        UpgradeBtn.interactable = GameRoot.Instance.UserData.Money.Value >= UpgradeCost;

        UpgradeBtnImg.color = UpgradeBtn.interactable ? DefaultColor : Config.Instance.GetImageColor("Bg_Gray");

        SetUpgradeImg();
    }

    public void SetUpgradeImg()
    {
        if (UpgradeData == null) return;

        int activeCount = UpgradeData.Upgradelevel.Value % (UpgradeImgList.Count + 1);

        for (int i = 0; i < UpgradeImgList.Count; i++)
        {
            if (i < activeCount)
            {
                // 활성화된 이미지는 빨간색으로
                UpgradeImgList[i].color = GetStateColor();
            }
            else
            {
                // 비활성화된 이미지는 회색으로
                UpgradeImgList[i].color = Config.Instance.GetImageColor("Bg_Gray");
            }
        }

        if (UpgradeData.Upgradeidx == (int)UpgradeSystem.UpgradeType.MoneyMultiUpgrade)
            BgImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, $"Common_Icon_Money_{UpgradeData.GetUpgradeOrder}");
    }

    public void OnClickUpgradeBtn()
    {
        if (GameRoot.Instance.UserData.Money.Value >= UpgradeCost)
        {
            GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.Money, -UpgradeCost);
            UpgradeData.Upgradelevel.Value += 1;
            UpgradeData.Upgradeternallevel += 1;

            if (UpgradeData.Upgradeidx == (int)UpgradeSystem.UpgradeType.MoneyMultiUpgrade)
            {
                GameRoot.Instance.UpgradeSystem.InComeUpgrade();
            }

            DirectionUpgrade();
            SetUpgradeValue();
            SetInComeValue();

        }
    }

    public void DirectionUpgrade()
    {
        bool isnewupgrade = UpgradeData.Upgradelevel.Value % 6 == 0;

        if (isnewupgrade)
        {
            GameRoot.Instance.UISystem.OpenUI<PopupNewUpgrade>(popup => popup.Set(UpgradeIdx));
        }
    }

    public Color GetStateColor()
    {
        switch (State)
        {
            case UpgradeState.RopeUp:
                return Config.Instance.GetImageColor("Upgrade_Blue");
            case UpgradeState.Balance:
                return Config.Instance.GetImageColor("Upgrade_Orange");
            case UpgradeState.InCome:
                return Config.Instance.GetImageColor("Upgrade_Green");
        }

        return Color.white;
    }



    public void InComeUpgradeAction()
    {
        //UpgradeImg.sprite = Config.Instance.GetImageSprite("Upgrade_Green");
        SetInComeValue();

        // 스케일 애니메이션: 1 → 1.2 → 1
        InComeMultiText.transform.localScale = Vector3.one;
        var sequence = DOTween.Sequence();
        sequence.Append(InComeMultiText.transform.DOScale(1.4f, 0.15f).SetEase(Ease.OutQuad));
        sequence.Append(InComeMultiText.transform.DOScale(1.0f, 0.15f).SetEase(Ease.InOutQuad));
    }
}
