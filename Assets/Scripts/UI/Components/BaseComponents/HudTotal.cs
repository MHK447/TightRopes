using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;    
using UniRx;


[UIPath("UI/Page/HudTotal", true)]
public class HudTotal : UIBase
{
    public Transform MoneyIconTr;

    protected override void Awake()
    {
        base.Awake();

    }

    public void OnClickUpgradeBtn()
    {
        GameRoot.Instance.UISystem.OpenUI<PopupUpgrade>();

    }

    
}
