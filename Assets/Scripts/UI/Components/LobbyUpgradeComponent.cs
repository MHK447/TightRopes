using UnityEngine;
using UnityEngine.UI;
using TMPro;



public class LobbyUpgradeComponent : MonoBehaviour
{
    private int UpgradeIdx;

    [SerializeField]
    private TextMeshProUGUI LevelText;

    [SerializeField]
    private TextMeshProUGUI UpgradeCostText;

    

    [SerializeField]
    private Button UpgradeBtn;


    public void Set(int upgradeidx)
    {
        UpgradeIdx = upgradeidx;

    }
}
