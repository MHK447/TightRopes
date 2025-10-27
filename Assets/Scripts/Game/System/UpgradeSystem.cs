using BanpoFri;
using UnityEngine;
using System.Numerics;
using System;
using BanpoFri.Data;
using UniRx;
using System.Collections.Generic;
public class UpgradeSystem
{
    public float directincomevalue = 0f;

    public Dictionary<int, float> UpgradeCostDic = new Dictionary<int, float>();

    private int CostMaxLevel = 1000;

    public enum UpgradeType
    {
        RopeUpgrade = 0,
        BalanceUpgrade = 1,
        MoneyMultiUpgrade = 2,

    }

    public void Create()
    {
        // 각 업그레이드 타입별로 테이블 계산
        for (int i = 0; i <= (int)UpgradeType.MoneyMultiUpgrade; i++)
        {
            var upgradeData = Tables.Instance.GetTable<UpgradeInfo>().GetData(i);
            if (upgradeData != null)
            {
                upgradeData.CalculateUpgradeTable(i, CostMaxLevel);
            }
        }

        if (GameRoot.Instance.UserData.Upgradedatas.Count == 0)
        {
            GameRoot.Instance.UserData.Incomemultivalue = GameRoot.Instance.UserData.Incomestartupgrade
             = Tables.Instance.GetTable<Define>().GetData("start_income_value").value / 100;

            for (int i = 0; i < (int)UpgradeType.MoneyMultiUpgrade + 1; i++)
            {
                GameRoot.Instance.UserData.Upgradedatas.Add(new UpgradeData() { Upgradeidx = i, Upgradelevel = new ReactiveProperty<int>(1), Upgradeternallevel = 1 });
            }
        }
    }

    public float RopeUpgradeValue(int upgradeorder)
    {
        var finddata = GameRoot.Instance.UserData.Upgradedatas[(int)UpgradeType.RopeUpgrade];

        if (finddata == null) return 0f;

        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;

        var stageinfotd = Tables.Instance.GetTable<StageInfo>().GetData(stageidx);

        float productswayvalue = GameRoot.Instance.UserData.RaceData.RaceProductCount.Value * stageinfotd.product_sway_value;

        float swayvalue = stageinfotd.base_sway_value + productswayvalue;

        var adcyclecount = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.AdCycleCount);

        for (int i = 0; i < upgradeorder + adcyclecount; i++)
        {
            swayvalue -= ProjectUtility.PercentCalc(swayvalue, 10);
        }

          for (int i = 0; i <  adcyclecount; i++)
        {
            swayvalue -= ProjectUtility.PercentCalc(swayvalue, 5);
        }


        if (swayvalue <= stageinfotd.end_sway_value)
        {
            swayvalue = stageinfotd.end_sway_value;
        }



        return swayvalue;
    }

    public float BalanceUpgradeValue(int upgradeorder)
    {
        var finddata = GameRoot.Instance.UserData.Upgradedatas[(int)UpgradeType.BalanceUpgrade];

        if (finddata == null) return 0f;

        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;

        var stageinfotd = Tables.Instance.GetTable<StageInfo>().GetData(stageidx);

        float balancevalue = stageinfotd.base_balance_value;

        for (int i = 0; i < upgradeorder; i++)
        {
            balancevalue += ProjectUtility.PercentCalc(balancevalue, 20);
        }

        if (balancevalue >= stageinfotd.end_balance_value)
        {
            balancevalue = stageinfotd.end_balance_value;
        }

        if(balancevalue > 50)
        {
            balancevalue = 50;
        }



        return balancevalue;
    }

    public void InComeUpgrade()
    {
        var finddata = GameRoot.Instance.UserData.Upgradedatas[(int)UpgradeType.MoneyMultiUpgrade];

        if (finddata == null) return;

        directincomevalue = 0f;

        float inc = ProjectUtility.PercentCalc(GameRoot.Instance.UserData.Incomestartupgrade, 10);
        inc = Mathf.Round(inc * 10f) / 10f; // 소수점 1자리 반올림
        GameRoot.Instance.UserData.Incomemultivalue += inc;

        if (finddata.Upgradelevel.Value % 6 == 0)
        {
            double powVal = System.Math.Pow(2.0, finddata.GetUpgradeOrder);
            directincomevalue = (float)System.Math.Round(powVal, 1, MidpointRounding.AwayFromZero); // 한 자리 반올림

            GameRoot.Instance.UserData.Incomemultivalue += directincomevalue;

            GameRoot.Instance.UserData.Incomestartupgrade = (int)directincomevalue;
        }
    }

    public void StageClearInComeLevelUp()
    {
        var finddata = GameRoot.Instance.UserData.Upgradedatas[(int)UpgradeType.MoneyMultiUpgrade];

        if (finddata == null) return;

        int currentLevel = finddata.Upgradeternallevel;
        int nextMultipleOfSix = ((currentLevel / 6) + 1) * 6;


        var plusvalue = nextMultipleOfSix - finddata.Upgradeternallevel;

        for(int i = 0; i < plusvalue; i++)
        {
            InComeUpgrade();
        }

        finddata.Upgradeternallevel = finddata.Upgradeternallevel + plusvalue;
    }


    public BigInteger GetUpgradeCost(int idx, int level)
    {
        int levelvalue = level;
        if(idx == (int)UpgradeType.MoneyMultiUpgrade)
        {
            var stageidx = 5 * (GameRoot.Instance.UserData.Stageidx.Value - 1);
            levelvalue = level - stageidx;
        }

        return Tables.Instance.GetTable<UpgradeInfo>().GetData(idx).GetCost(idx, levelvalue);
    }



}
