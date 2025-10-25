using System;
using System.Collections.Generic;
using System.Numerics;
using UniRx;
using Google.FlatBuffers;
using BanpoFri;
public partial class UserDataSystem
{
    public List<UpgradeData> Upgradedatas { get; private set; } = new List<UpgradeData>();
    private void SaveData_UpgradeData(FlatBufferBuilder builder)
    {
        // 선언된 변수들은 모두 저장되어야함

        // Upgradedatas Array 저장
        Offset<BanpoFri.Data.UpgradeData>[] upgradedatas_Array = null;
        VectorOffset upgradedatas_Vector = default;

        if(Upgradedatas.Count > 0){
            upgradedatas_Array = new Offset<BanpoFri.Data.UpgradeData>[Upgradedatas.Count];
            int index = 0;
            foreach(var pair in Upgradedatas){
                var item = pair;
                upgradedatas_Array[index++] = BanpoFri.Data.UpgradeData.CreateUpgradeData(
                    builder,
                    item.Upgradeidx,
                    item.Upgradelevel.Value,
                    item.Upgradeternallevel
                );
            }
            upgradedatas_Vector = BanpoFri.Data.UserData.CreateUpgradedatasVector(builder, upgradedatas_Array);
        }



        Action cbAddDatas = () => {
            BanpoFri.Data.UserData.AddUpgradedatas(builder, upgradedatas_Vector);
        };

        cb_SaveAddDatas += cbAddDatas;

    }
    private void LoadData_UpgradeData()
    {
        // 로드 함수 내용

        // Upgradedatas 로드
        Upgradedatas.Clear();
        int Upgradedatas_length = flatBufferUserData.UpgradedatasLength;
        for (int i = 0; i < Upgradedatas_length; i++)
        {
            var Upgradedatas_item = flatBufferUserData.Upgradedatas(i);
            if (Upgradedatas_item.HasValue)
            {
                var upgradedata = new UpgradeData
                {
                    Upgradeidx = Upgradedatas_item.Value.Upgradeidx,
                    Upgradelevel = new ReactiveProperty<int>(Upgradedatas_item.Value.Upgradelevel),
                    Upgradeternallevel = Upgradedatas_item.Value.Upgradeternallevel
                };
                Upgradedatas.Add(upgradedata);
            }
        }
    }

}


public class UpgradeData
{
    public int Upgradeternallevel { get; set; } = 1;

    public int Upgradeidx { get; set; } = 0;
    public IReactiveProperty<int> Upgradelevel { get; set; } = new ReactiveProperty<int>(1);


    public int GetUpgradeOrder 
    {
        get 
        {
            return ((Upgradelevel.Value - 1) / 5);
        }
    }


    public int GetUpgradeternalOrder
    {
        get 
        {
            return ((Upgradeternallevel - 1) / 5);
        }
    }


    public int GetInComeLevel
    {
        get
        {
            return ((Upgradelevel.Value) / 6);
        }
    }



   
}
