using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BanpoFri;
using UniRx;
using BanpoFri.Data;


public class LobbyUpgradeComponent : MonoBehaviour
{
    private int UpgradeIdx;

    [SerializeField]
    private TextMeshProUGUI LevelText;

    [SerializeField]
    private TextMeshProUGUI UpgradeCostText;


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
}
