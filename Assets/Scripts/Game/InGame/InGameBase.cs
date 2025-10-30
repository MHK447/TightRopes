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
    }


    public IEnumerator WaitStageLoad()
    {
        if (StageMap != null)
        {
            Destroy(StageMap.gameObject);
        }

        SetStage(GameRoot.Instance.UserData.Stageidx.Value);
        yield return new WaitUntil(() => StageMap != null);
        GetMainCam.Init();

        // 스테이지 로드 완료 후 로딩 숨기기
        GameRoot.Instance.Loading.Hide(true);

    }


    public void SetStage(int stageidx)
    {
        var stageinfotd = Tables.Instance.GetTable<StageInfo>().GetData(stageidx);



        if (stageinfotd != null)
        {
            //temp  
            Addressables.InstantiateAsync(stageinfotd.prefab).Completed += (handle) =>
            {
                StageMap = handle.Result.GetComponent<InGameStage>();
                StageMap.CallStartGame();
            };


            GameRoot.Instance.PluginSystem.ShowBanner(MaxSdkBase.BannerPosition.BottomCenter);
        }
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
