using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;

public class ContentsOpenSystem : MonoBehaviour
{
    public enum ContentsOpenType
    { 
        Interstitial = 1,
        BoosterOpen = 2,
        AdCycleOpen = 3,
    }


    public bool ContentsOpenCheck(ContentsOpenType opentype)
    {
        bool isopencheck = false;

        var td = Tables.Instance.GetTable<ContentsOpenCheck>().GetData((int)opentype);

        if(td != null)
        {
            if(td.stage_idx <= GameRoot.Instance.UserData.Stageidx.Value)
            {
                isopencheck = true;
                return isopencheck;
            }
        }

        return isopencheck;
    }


}
