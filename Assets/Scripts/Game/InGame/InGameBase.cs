using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using BanpoFri;
using UniRx;
using System.Linq;

public class InGameBase : InGameMode
{
    


    public override void Load()
    {
        base.Load();
        GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().StartGame();
    }


    public void StartGame()
    {
    }



    protected override void LoadUI()
    {
        base.LoadUI();
        GameRoot.Instance.InGameSystem.InitPopups();
        GameRoot.Instance.UISystem.OpenUI<HudTotal>();
    }



    public override void UnLoad()
    {
        base.UnLoad();
    }
}
