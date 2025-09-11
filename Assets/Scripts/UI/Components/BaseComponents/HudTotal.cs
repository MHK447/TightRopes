using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;    
using UniRx;


[UIPath("UI/Page/HudTotal", true)]
public class HudTotal : UIBase
{

    protected override void Awake()
    {
        base.Awake();

    }

    public void OnClickUpgradeBtn()
    {
        GameRoot.Instance.UISystem.OpenUI<PopupUpgrade>();

    }


    public void OnClickAquariumBtn()
    {
        GameRoot.Instance.UISystem.OpenUI<PageAquarium>(popup=> popup.Init());
    }
}
