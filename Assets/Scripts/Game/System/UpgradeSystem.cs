using BanpoFri;
using UnityEngine;
using System.Numerics;
using System;
using BanpoFri.Data;
using UniRx;

public class UpgradeSystem
{

    public enum UpgradeType
    {
        RopeUpgrade = 0,
        ConteringUpgrade = 1,
        MoneyMultiUpgrade = 2,

    }

    public void Create()
    {
        if (GameRoot.Instance.UserData.Upgradedatas.Count == 0)
        {
            for (int i = 0; i < (int)UpgradeType.MoneyMultiUpgrade + 1; i++)
            {
                GameRoot.Instance.UserData.Upgradedatas.Add(new UpgradeData() { Upgradeidx = i, Upgradelevel = new ReactiveProperty<int>(0) });
            }
        }

    }

    public float GetUpgradeValue(int idx)
    {

        float value = 0f;
        var td = Tables.Instance.GetTable<UpgradeInfo>().GetData(idx);

        if (td != null)
        {
            var level = GameRoot.Instance.UserData.Upgradedatas[idx].Upgradelevel.Value;


            value = td.upgrade_start_value + (td.level_up_value * level);

        }


        return value;

    }


    public BigInteger GetUpgradeCost(int idx)
    {
        var finddata = GameRoot.Instance.UserData.Upgradedatas[idx];

        var td = Tables.Instance.GetTable<UpgradeInfo>().GetData(idx);

        if(td == null) return 0;

        int multivalue = finddata.Upgradelevel.Value / td.level_up_multi;

        multivalue = multivalue ==  0 ? 1 : multivalue;

        var inceeaseCost = (td.inceease_upgrade_cost  *  100 + (td.inceease_upgrade_cost * finddata.Upgradelevel.Value)) / 100;

        inceeaseCost *= multivalue;
        
        return inceeaseCost;
    }
}
