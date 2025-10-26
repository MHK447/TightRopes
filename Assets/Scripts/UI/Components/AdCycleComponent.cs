
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class AdCycleComponent : MonoBehaviour
{


    [SerializeField]
    private Button UpgradeBtn;

    [SerializeField]
    private TextMeshProUGUI CycleCountText;

    void Awake()
    {
        UpgradeBtn.onClick.AddListener(OnClickUpgradeBtn);
    }

    public void Init()
    {
        SetCycleCount(GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.AdCycleCount));

        ProjectUtility.SetActiveCheck(this.gameObject, GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.AdCycleOpen));
    }

    public void OnClickUpgradeBtn()
    {
        GameRoot.Instance.GetAdManager.ShowRewardedAd(() =>
        {
            GameRoot.Instance.UserData.AddRecordCount(Config.RecordCountKeys.AdCycleCount, 1);

            GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().StageMap.Player.CycleAction();

            SetCycleCount(GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.AdCycleCount));
        });
    }


    public void SetCycleCount(int count)
    {
        CycleCountText.text = count == 0 ? "UnLock" : $"Lv{count}";
    }

}
