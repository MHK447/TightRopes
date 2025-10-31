using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;


[UIPath("UI/Popup/PopupInGameLobby")]
public class PopupInGameLobby : UIBase
{
    [SerializeField]
    private List<LobbyUpgradeComponent> LobbyUpgradeComponents = new List<LobbyUpgradeComponent>();

    [SerializeField]
    private Button StageBtn;

    [SerializeField]
    private AdCycleComponent AdCycleComponent;



    [SerializeField]
    private TextMeshProUGUI TapToStartText;


    //TEXT 

    [SerializeField]
    private Image BgImg;

    [SerializeField]
    private Image MapImg;

    [SerializeField]
    private TextMeshProUGUI MapText;


    protected override void Awake()
    {
        base.Awake();

        // TapToStartText 스케일 애니메이션 설정
        StartTapToStartAnimation();

        StageBtn.onClick.AddListener(OnStageBtnClick);
    }


    public void Init()
    {

        for (int i = 0; i < LobbyUpgradeComponents.Count; i++)
        {
            LobbyUpgradeComponents[i].Set(i);
        }

        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;

        var td = Tables.Instance.GetTable<StageInfo>().GetData(stageidx);

        if (td != null)
        {
            MapImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Map, td.image);
            MapText.text = $"STAGE {stageidx}";
            BgImg.color = Config.Instance.GetImageColor(td.image_color);

            var fontvalue = GameRoot.Instance.UserData.Stageidx.Value % Config.Instance.TextMaterialList.Count;

            if (fontvalue == 0)
            {
                fontvalue = Config.Instance.TextMaterialList.Count - 1;
            }

            MapText.fontSharedMaterial = Config.Instance.TextMaterialList[fontvalue - 1];

            AdCycleComponent.Init();
        }
    }


    public LobbyUpgradeComponent GetLobbyUpgradeComponent(int index)
    {
        return LobbyUpgradeComponents[index];
    }

    private void StartTapToStartAnimation()
    {
        if (TapToStartText != null)
        {
            // 초기 스케일을 2로 설정
            TapToStartText.transform.localScale = Vector3.one * 1.3f;

            // 2에서 1로 스케일 다운 후 다시 2로 스케일 업하는 무한 반복 애니메이션
            TapToStartText.transform.DOScale(1f, 0.8f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }


    public void OnStageBtnClick()
    {
        GameRoot.Instance.UISystem.OpenUI<PageStage>(page => page.Init());
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SoundPlayer.Instance.PlayBGM("bgm");
        SoundPlayer.Instance.SetBGMVolume(0.1f);
    }

    void OnDisable()
    {
        SoundPlayer.Instance.SetBGMVolume(0f);
    }

}
