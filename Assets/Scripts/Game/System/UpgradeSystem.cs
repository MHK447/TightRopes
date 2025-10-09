using BanpoFri;
using UnityEngine;
using System.Numerics;
using System;
using BanpoFri.Data;
using UniRx;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
public class UpgradeSystem
{
    public float directincomevalue = 0f;

    public Dictionary<int, float> UpgradeCostDic = new Dictionary<int, float>();

    private int CostMaxLevel = 1000;

    public enum UpgradeType
    {
        RopeUpgrade = 0,
        ConteringUpgrade = 1,
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
                GameRoot.Instance.UserData.Upgradedatas.Add(new UpgradeData() { Upgradeidx = i, Upgradelevel = new ReactiveProperty<int>(1) });
            }
        }

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


    public BigInteger GetUpgradeCost(int idx, int level)
    {
        return Tables.Instance.GetTable<UpgradeInfo>().GetData(idx).GetCost(idx, level);
    }



}
