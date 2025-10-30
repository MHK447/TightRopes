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
        try
        {
            if (GameRoot.Instance?.InGameSystem?.GetInGame<InGameBase>()?.StageMap != null)
            {
                GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().StageMap.SetState(InGameStage.InGameState.WaitPlay);
            }
            else
            {
                Debug.LogError("[PopupInGameLobby] StageMap is null");
            }

            for (int i = 0; i < LobbyUpgradeComponents.Count; i++)
            {
                if (LobbyUpgradeComponents[i] != null)
                {
                    LobbyUpgradeComponents[i].Set(i);
                }
                else
                {
                    Debug.LogError($"[PopupInGameLobby] LobbyUpgradeComponent at index {i} is null");
                }
            }

            var stageidx = GameRoot.Instance.UserData.Stageidx.Value;

            var td = Tables.Instance.GetTable<StageInfo>().GetData(stageidx);

            if(td != null)
            {
                if (MapImg != null)
                {
                    MapImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Map, td.image);
                }
                else
                {
                    Debug.LogError("[PopupInGameLobby] MapImg is null");
                }

                if (MapText != null)
                {
                    MapText.text = Tables.Instance.GetTable<Localize>().GetString(td.name);
                    
                    if (Config.Instance?.TextMaterialList != null && stageidx > 0 && stageidx <= Config.Instance.TextMaterialList.Count)
                    {
                        MapText.fontSharedMaterial = Config.Instance.TextMaterialList[stageidx - 1];
                    }
                    else
                    {
                        Debug.LogError($"[PopupInGameLobby] TextMaterialList is null or index out of range. stageidx: {stageidx}");
                    }
                }
                else
                {
                    Debug.LogError("[PopupInGameLobby] MapText is null");
                }

                if (BgImg != null)
                {
                    BgImg.color = Config.Instance.GetImageColor(td.image_color);
                }
                else
                {
                    Debug.LogError("[PopupInGameLobby] BgImg is null");
                }

                if (AdCycleComponent != null)
                {
                    AdCycleComponent.Init();
                }
                else
                {
                    Debug.LogError("[PopupInGameLobby] AdCycleComponent is null");
                }
            }
            else
            {
                Debug.LogError($"[PopupInGameLobby] StageInfo data not found for stageidx: {stageidx}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PopupInGameLobby] Exception in Init: {e.Message}\n{e.StackTrace}");
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
