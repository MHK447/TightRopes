using UnityEngine;
using UnityEngine.UI;
using BanpoFri;
using TMPro;

public class StageComponent : MonoBehaviour
{
    [SerializeField]
    private Image StageImg;

    [SerializeField]
    private Image StageColorImg;

    [SerializeField]
    private TextMeshProUGUI StageNameText;


    [SerializeField]
    private GameObject LockObj;

    [SerializeField]
    private GameObject NoneLockObj;


    public void Set(int stageidx)
    {
        var stageinfotd = Tables.Instance.GetTable<StageInfo>().GetData(stageidx);

        if (stageinfotd != null)
        {
            StageImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Map, $"MapIcon_{stageidx:00}");
            StageImg.color = stageidx <= GameRoot.Instance.UserData.Stageidx.Value ? Color.white : Color.black;

            StageNameText.text = Tables.Instance.GetTable<Localize>().GetString(stageinfotd.name);
            StageColorImg.color = Config.Instance.GetImageColor(stageinfotd.image_color);
            ProjectUtility.SetActiveCheck(LockObj, stageidx > GameRoot.Instance.UserData.Stageidx.Value);
            ProjectUtility.SetActiveCheck(NoneLockObj, stageidx <= GameRoot.Instance.UserData.Stageidx.Value);
        }
    }

}
