using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

[UIPath("UI/Popup/PopupStageClear")]
public class PopupStageClear : UIBase
{
    [SerializeField]
    private TextMeshProUGUI StageClearDescText;

    [SerializeField]
    private TextMeshProUGUI StageClearDesc2Text;

    [SerializeField]
    private TextMeshProUGUI RewardAdValueText;

    [SerializeField]
    private TextMeshProUGUI RewardBaseValueText;

    [SerializeField]
    private Button AdRewardBtn;

    [SerializeField]
    private Button BaseRewardBtn;

    private int RewardValue = 0;
    private int currentDisplayRewardValue = 0;
    private int currentDisplayAdRewardValue = 0;
    private System.Action OnNextStageCallback;

    [Header("Animation Settings")]
    [SerializeField] private float textAppearDelay = 0.3f;
    [SerializeField] private float textAppearDuration = 0.5f;
    [SerializeField] private float countUpDuration = 1.5f;

    protected override void Awake()
    {
        base.Awake();

        AdRewardBtn.onClick.AddListener(OnAdRewardBtnClick);
        BaseRewardBtn.onClick.AddListener(OnBaseRewardBtnClick);

        InitializeUI();
    }

    private void InitializeUI()
    {
        // 초기 상태에서 모든 텍스트를 숨김
        StageClearDescText.alpha = 0f;
        StageClearDesc2Text.alpha = 0f;
        RewardAdValueText.alpha = 0f;
        RewardBaseValueText.alpha = 0f;

        // 버튼들도 초기에 숨김
        AdRewardBtn.gameObject.SetActive(false);
        BaseRewardBtn.gameObject.SetActive(false);
    }

    public void Set(int rewardvalue, System.Action onNextStageCallback = null)
    {
        RewardValue = rewardvalue;
        currentDisplayRewardValue = 0;
        currentDisplayAdRewardValue = 0;
        OnNextStageCallback = onNextStageCallback;

        // 초기 텍스트 설정
        RewardAdValueText.text = ProjectUtility.CalculateMoneyToString(0);
        RewardBaseValueText.text = ProjectUtility.CalculateMoneyToString(0);

        // 애니메이션 시작
        StartCoroutine(PlayStageClearAnimation());
    }

    public void OnAdRewardBtnClick()
    {
        GameRoot.Instance.GetAdManager.ShowRewardedAd(() =>
        {
            GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.Money, RewardValue * 2);
            ProceedToNextStage();
        });
    }

    public void OnBaseRewardBtnClick()
    {
        GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.Money, RewardValue);
        ProceedToNextStage();
    }

    private void ProceedToNextStage()
    {
        Hide();

        GameRoot.Instance.UISystem.OpenUI<PageStage>(popup => popup.Interaction(GameRoot.Instance.UserData.Stageidx.Value + 1, () =>
        {
            // 다음 스테이지로 넘어가기
            if (OnNextStageCallback != null)
            {
                OnNextStageCallback.Invoke();
            }
        }));

    }

    private IEnumerator PlayStageClearAnimation()
    {
        // 1단계: 스테이지 클리어 텍스트 등장 (극적인 스케일 효과)
        yield return new WaitForSeconds(0.2f);

        StageClearDescText.transform.localScale = Vector3.zero;
        StageClearDescText.DOFade(1f, textAppearDuration);
        StageClearDescText.transform.DOScale(Vector3.one, textAppearDuration)
            .SetEase(Ease.OutBack);

        yield return new WaitForSeconds(textAppearDelay);

        // 2단계: 두 번째 설명 텍스트 등장
        StageClearDesc2Text.transform.localScale = Vector3.zero;
        StageClearDesc2Text.DOFade(1f, textAppearDuration);
        StageClearDesc2Text.transform.DOScale(Vector3.one, textAppearDuration)
            .SetEase(Ease.OutBounce);

        yield return new WaitForSeconds(textAppearDelay);

        // 3단계: 리워드 텍스트들 등장과 동시에 카운트업 시작
        RewardBaseValueText.transform.localScale = Vector3.zero;
        RewardAdValueText.transform.localScale = Vector3.zero;

        // 베이스 리워드 텍스트 등장
        RewardBaseValueText.DOFade(1f, textAppearDuration);
        RewardBaseValueText.transform.DOScale(Vector3.one, textAppearDuration)
            .SetEase(Ease.OutBack);

        // 광고 리워드 텍스트 등장 (약간의 딜레이)
        yield return new WaitForSeconds(0.1f);
        RewardAdValueText.DOFade(1f, textAppearDuration);
        RewardAdValueText.transform.DOScale(Vector3.one, textAppearDuration)
            .SetEase(Ease.OutBack);

        // 4단계: 리워드 값 카운트업 애니메이션
        yield return new WaitForSeconds(0.2f);
        StartRewardCountUp();

        // 5단계: 버튼들 등장
        yield return new WaitForSeconds(countUpDuration + 0.3f);
        ShowButtons();
    }

    private void StartRewardCountUp()
    {
        // 베이스 리워드 카운트업
        DOTween.To(() => currentDisplayRewardValue, x =>
        {
            currentDisplayRewardValue = x;
            RewardBaseValueText.text = ProjectUtility.CalculateMoneyToString(currentDisplayRewardValue);
        }, RewardValue, countUpDuration)
        .SetEase(Ease.OutQuart);

        // 광고 리워드 카운트업 (약간의 딜레이)
        DOTween.To(() => currentDisplayAdRewardValue, x =>
        {
            currentDisplayAdRewardValue = x;
            RewardAdValueText.text = ProjectUtility.CalculateMoneyToString(currentDisplayAdRewardValue);
        }, RewardValue * 2, countUpDuration)
        .SetEase(Ease.OutQuart)
        .SetDelay(0.2f);
    }

    private void ShowButtons()
    {
        AdRewardBtn.gameObject.SetActive(true);
        BaseRewardBtn.gameObject.SetActive(true);

        // 버튼들 등장 애니메이션
        AdRewardBtn.transform.localScale = Vector3.zero;
        BaseRewardBtn.transform.localScale = Vector3.zero;

        AdRewardBtn.transform.DOScale(Vector3.one, 0.4f)
            .SetEase(Ease.OutBack)
            .SetDelay(0.1f);

        BaseRewardBtn.transform.DOScale(Vector3.one, 0.4f)
            .SetEase(Ease.OutBack)
            .SetDelay(0.2f);
    }

}
