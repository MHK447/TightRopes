using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;
using UniRx;
using DG.Tweening;
public class HudTopCurrency : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI MoneyText;

    [SerializeField]
    private GameObject MoneyRoot;

    [SerializeField]
    private TextMeshProUGUI CashText;

    [SerializeField]
    private Button CashBtn;


    private CompositeDisposable disposables = new CompositeDisposable();
    private readonly int[] CurrencyValues = new int[3];
    private Tweener[] Tweeners = new Tweener[3];


    void Awake()
    {
        SetDataHook();
    }


    void OnEnable()
    {
        disposables.Clear();

        GameRoot.Instance.UserData.Money.Subscribe(x =>
        {
            MoneyText.text = ProjectUtility.CalculateMoneyToString(x);
        }).AddTo(disposables);
    }

    void OnDestroy()
    {
        disposables.Clear();
    }

    void Update()
    {
        SetTexts();
    }


    private void SetTexts()
    {
        if (MoneyText) MoneyText.text = ProjectUtility.CalculateMoneyToString((System.Numerics.BigInteger)CurrencyValues[0]);
    }
    public void SyncReward()
    {
        CurrencyValues[0] = (int)GameRoot.Instance.UserData.Money.Value;
        CurrencyValues[1] = (int)GameRoot.Instance.UserData.Cash.Value;
        SetTexts();
    }

    private void SetDataHook()
    {
        if (MoneyText != null)
        {
            MoneyText.text = ProjectUtility.CalculateMoneyToString(GameRoot.Instance.UserData.Money.Value);

            GameRoot.Instance.UserData.Money.Subscribe(x =>
            {
                if (!gameObject.activeInHierarchy)
                {
                    CurrencyValues[0] = (int)x;
                    return;
                }

                if (Tweeners[0] != null)
                {
                    Tweeners[0].Kill();
                    Tweeners[0] = null;
                }

                Tweeners[0] = DOTween.To(() => CurrencyValues[0],
                (int v) => CurrencyValues[0] = v,
                (int)x,
                 0.5f)
      .SetEase(Ease.Linear)
      .SetUpdate(true)
      .OnComplete(() =>
      {
          Tweeners[0] = null;
      });

            }).AddTo(this);
        }
    }
}
