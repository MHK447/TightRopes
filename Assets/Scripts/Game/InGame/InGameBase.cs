using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using BanpoFri;
using UniRx;
using System.Linq;
using UnityEngine.UI;

public class InGameBase : InGameMode
{
    public InGameStage StageMap = null;




    public override void Load()
    {
        base.Load();
        SetStage(1);
    }

    public void SetStage(int stageidx)
    {

        //temp  
        Addressables.LoadAssetAsync<GameObject>("Stage" + stageidx).Completed += (handle) =>
        {
            StageMap = handle.Result.GetComponent<InGameStage>();
            StageMap.CallStartGame();
        };

    }


    protected override void LoadUI()
    {
        base.LoadUI();
        GameRoot.Instance.InGameSystem.InitPopups();
    }



    public override void UnLoad()
    {
        base.UnLoad();
    }

    protected override void Update()
    {
        base.Update();
    }

}
