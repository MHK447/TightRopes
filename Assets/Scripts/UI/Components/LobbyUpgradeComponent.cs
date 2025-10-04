using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BanpoFri;
using UniRx;
using BanpoFri.Data;
using System.Collections.Generic;



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
    private List<Image> UpgradeImgList = new List<Image>();

    [SerializeField]
    private UpgradeState State;





    private UpgradeData UpgradeData;



    [SerializeField]
    private Button UpgradeBtn;

    private System.Numerics.BigInteger UpgradeCost;


    private CompositeDisposable disposables = new CompositeDisposable();

    void Awake()
    {
        UpgradeBtn.onClick.AddListener(OnClickUpgradeBtn);
    }


    public void Set(int upgradeidx)
    {
        UpgradeIdx = upgradeidx;

        UpgradeData = GameRoot.Instance.UserData.Upgradedatas[upgradeidx];

        disposables.Clear();

        UpgradeData.Upgradelevel.Subscribe(x=> { SetUpgradeValue(); }).AddTo(disposables);

        GameRoot.Instance.UserData.Money.Subscribe(x=> { SetUpgradeValue(); }).AddTo(disposables);

        
    }

    void OnDestroy()
    {
        disposables.Clear();
    }

    void OnDisable()
    {
        disposables.Clear();
    }


    public void SetUpgradeValue()
    {
        LevelText.text = UpgradeData.Upgradelevel.ToString();
        UpgradeCost = GameRoot.Instance.UpgradeSystem.GetUpgradeCost(UpgradeIdx);

        UpgradeCostText.text = ProjectUtility.CalculateMoneyToString(UpgradeCost);      

        UpgradeBtn.interactable = GameRoot.Instance.UserData.Money.Value >= UpgradeCost;
        
        SetUpgradeImg();
    }

    public void SetUpgradeImg()
    {
        if(UpgradeData == null) return;
        
        int activeCount = UpgradeData.Upgradelevel.Value % UpgradeImgList.Count + 1;

        for(int i = 0; i < UpgradeImgList.Count; i++)
        {
            if(i < activeCount)
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
    }

    public void OnClickUpgradeBtn()
    {
        if(GameRoot.Instance.UserData.Money.Value >= UpgradeCost)
        {
            GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency , (int)Config.CurrencyID.Money , -UpgradeCost);
            UpgradeData.Upgradelevel.Value += 1;

            SetUpgradeValue();
        }
    }

    public Color GetStateColor()
    {
        switch(State)
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
}
