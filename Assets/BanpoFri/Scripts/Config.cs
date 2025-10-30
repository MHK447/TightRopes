using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using System.Linq;
using BanpoFri;


    public enum Language
    {
        en,
        ko,
        es,
        ja,
        ptbr,
        th,
        tw,
        vi,
        bg,
        cn,
        cs,
        da,
        nl,
        et,
        fi,
        fr,
        de,
        el,
        hu,
        id,
        it,
        lv,
        lt,
        no,
        pl,
        pt,
        ro,
        ru,
        sk,
        sl,
        sv,
        tr,
        ua

    }


[System.Serializable]
public class FontDefine
{
    public Language country;
    public Font font;
}

[System.Serializable]
public class Config : BanpoFri.SingletonScriptableObject<Config>, BanpoFri.ILoader
{

    public enum LandCondination
    {
        Great,
        Basic,
        Sad,
    }

    public enum InGameUpgradeIdx
    {
        ATTACK,
        ATTACKSPEED,
        ATTACKRANGE,
        ATTACKREGEN,
        HP,
        HPREGEN,
        CRITICALPERCENT,
        CRITICALMULTIPLE,
    }

    public enum LABUpgradeIdx
    {
        ATTACK,
        ATTACKREGEN,
        ATTACKRANGE,
        HP,
        HPREGEN,
        CRITICALPERCENT,
        CRITICALDAMAGE,
    }

    public enum RecordKeys
    {
        StagePlayTime,
        EventStagePlayTime,
        Init,
        FirstDayPlayTime,
        FirstDayLogTime,
        M_Rev_05,
        ABTest,
        ShopDailyPurchaseCnt,
        TryTowerClear,
        UseADTicketCnt,

        AdCycleCount,
    }

    public enum WeaponType
    {
        Base = 1,
        TrackEnemy,
    }

    public enum CurrencyID
    {
        Money = 1,
        Cash = 2,
        EnergyMoney = 3,
        GachaCoin = 4,
        StarCoin = 5,
    }


    public enum RewardType
    {
        Currency = 1,
        Food = 101,
    }

    public enum RecordCountKeys
    {
        FirstEcpm,
        Init,
        StartStage,
        Navi_Start,
        FreeGemCount,
        AdGemCount,
        AdCycleCount,
        TutorialStageCount,
        FirstSwayAdd,
        AdWatchCount,
        BuyInAppCountTotal,
    }





    [System.Serializable]
    public class ColorDefine
    {
        public string key_string;
        public Color color;
    }

    [HideInInspector]
    [SerializeField]
    private List<ColorDefine> _textColorDefines = new List<ColorDefine>();
    [HideInInspector]
    [SerializeField]
    private List<ColorDefine> _eventTextColorDefines = new List<ColorDefine>();
    private Dictionary<string, Color> _textColorDefinesDic = new Dictionary<string, Color>();

    [SerializeField]
    private List<Material> RopeUpgradeMat = new List<Material>();

    [SerializeField]
    private Font mainfont;
    [SerializeField]
    private List<FontDefine> _fontDefines = new List<FontDefine>();



    public List<ColorDefine> TextColorDefines
    {
        get
        {
            return _textColorDefines;
        }
    }
    public List<ColorDefine> EventTextColorDefines
    {
        get
        {
            return _eventTextColorDefines;
        }
    }

    [HideInInspector]
    [SerializeField]
    private List<ColorDefine> _imageColorDefines = new List<ColorDefine>();
    [HideInInspector]
    [SerializeField]
    private List<ColorDefine> _eventImgaeColorDefines = new List<ColorDefine>();
    private Dictionary<string, Color> _imageColorDefinesDic = new Dictionary<string, Color>();
    public List<ColorDefine> ImageColorDefines
    {
        get
        {
            return _imageColorDefines;
        }
    }
    public List<ColorDefine> EventImageColorDefines
    {
        get
        {
            return _eventImgaeColorDefines;
        }
    }

    public Material SkeletonGraphicMat;
    public Material DisableSpriteMat;
    public Material EnableSpriteMat;
    public Material ImgAddtiveMat;

    public List<Material> TextMaterialList = new List<Material>();




    public Color GetTextColor(string key)
    {
        if (_textColorDefinesDic.ContainsKey(key))
            return _textColorDefinesDic[key];

        return Color.white;
    }


    public Material GetRopeUpgradeMat(int index)
    {
        if (RopeUpgradeMat == null || RopeUpgradeMat.Count == 0)
            return null;

        // 배열 크기를 넘어가면 % 연산으로 순환
        int safeIndex = index % RopeUpgradeMat.Count;
        return RopeUpgradeMat[safeIndex];
    }

    public Color GetImageColor(string key)
    {
        if (_imageColorDefinesDic.ContainsKey(key))
            return _imageColorDefinesDic[key];

        return Color.white;
    }


    public Color GetUnitGradeColor(int grade)
    {
        switch (grade)
        {
            case 1:
                return GetImageColor("Unit_Grade_1");
            case 2:
                return GetImageColor("Unit_Grade_2");
            case 3:
                return GetImageColor("Unit_Grade_3");
        }

        return Color.white;
    }

    public void UpdateFallbackOrder(Language CurLangauge)
    {
        if (CurLangauge != Language.ja &&
            CurLangauge != Language.tw) return;

        // foreach (var name in mainfont.fontNames)
        //     Debug.Log("before :" + name);

        var list = mainfont.fontNames.ToList();

        var font = _fontDefines.Where(x => x.country == CurLangauge).FirstOrDefault();
        if (font != null)
        {
            list.Remove(font.font.fontNames[0]);
            list.Insert(1, font.font.fontNames[0]);

            var new_array = new string[list.Count];

            for (int i = 0; i < mainfont.fontNames.Length; i++)
            {
                new_array[i] = list[i];
            }

            mainfont.fontNames = new_array;

            new_array = null;

            // foreach (var name in mainfont.fontNames)
            //     Debug.Log("after :" + name);

        }
    }



    public void Load()
    {
        _textColorDefinesDic.Clear();
        foreach (var cd in _textColorDefines)
        {
            _textColorDefinesDic.Add(cd.key_string, cd.color);
        }
        foreach (var cd in _eventTextColorDefines)
        {
            _textColorDefinesDic.Add(cd.key_string, cd.color);
        }
        _imageColorDefinesDic.Clear();
        foreach (var cd in _imageColorDefines)
        {
            _imageColorDefinesDic.Add(cd.key_string, cd.color);
        }
        foreach (var cd in _eventImgaeColorDefines)
        {
            _imageColorDefinesDic.Add(cd.key_string, cd.color);
        }
    }
}
