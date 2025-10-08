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
    private TextMeshProUGUI TapToStartText;

    protected override void Awake()
    {
        base.Awake();

        // TapToStartText 스케일 애니메이션 설정
        StartTapToStartAnimation();
    }


    public void Init()
    {

        for (int i = 0; i < LobbyUpgradeComponents.Count; i++)
        {
            LobbyUpgradeComponents[i].Set(i);
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

}
