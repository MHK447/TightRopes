using BanpoFri;
using UnityEngine;
using System.Numerics;
using System;

public class UpgradeSystem
{

    public enum UpgradeType
    {
        GroundSizeUpgrade = 0,
        ConteringUpgrade,
        MoneyMultiUpgrade
    }

    public void Create()
    {
        if (GameRoot.Instance.UserData.Upgradedata.Count == 0)
        {
            for (int i = 0; i < (int)UpgradeType.GroundSizeUpgrade + 1; i++)
            {
                GameRoot.Instance.UserData.Upgradedata.Add(new UpgradeData() { Upgradeidx = i, Upgradelevel = 1 });
            }
        }

    }




    public float GetUpgradeValue(int idx)
    {

        float value = 0f;
        var td = Tables.Instance.GetTable<UpgradeInfo>().GetData(idx);

        if (td != null)
        {
            var level = GameRoot.Instance.UserData.Upgradedata[idx].Upgradelevel;


            value = td.upgrade_start_value + (td.level_up_value * level);

        }


        return value;

    }


    public BigInteger GetUpgradeCost(int idx)
    {
        var finddata = GameRoot.Instance.UserData.Upgradedata[idx];

        var td = Tables.Instance.GetTable<UpgradeInfo>().GetData(idx);

        if(td == null) return 0;

        int multivalue = finddata.Upgradelevel / td.level_up_multi;

        multivalue = multivalue ==  0 ? 1 : multivalue;

        var inceeaseCost = (td.inceease_upgrade_cost  *  100 + (td.inceease_upgrade_cost * finddata.Upgradelevel)) / 100;

        inceeaseCost *= multivalue;
        
        return inceeaseCost;
    }
}
