using UnityEngine;
using BanpoFri;
using System.Collections;
using System;
using DG.Tweening;

public class RopeComponent : MonoBehaviour
{
    [SerializeField]
    private GameObject RopeObj;

    [SerializeField]
    private Material RopeMat;

    [SerializeField]
    private Renderer ropeRenderer;

    // Intensity 애니메이션 관련 변수들
    private bool isAnimating = false;
    private float targetIntensity = 0f;
    private float animationDuration = 2f;

    // Emission 설정
    private Color whiteEmission = Color.white;
    private float minIntensity = -10f;
    private float maxIntensity = 3f;

    // 액션 시스템
    public Action OnUpgradeEffectComplete;

    private InGameStage StageMap;

    public void Init()
    {

        // Renderer 컴포넌트 자동 할당
        if (ropeRenderer == null && RopeObj != null)
        {
            ropeRenderer = RopeObj.GetComponent<Renderer>();
        }

        // 초기 intensity를 -10으로 설정

        StageMap = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().StageMap;
        
        var level = GameRoot.Instance.UserData.Upgradedatas[(int)UpgradeSystem.UpgradeType.RopeUpgrade].GetUpgradeternalOrder;

        RopeMat = Config.Instance.GetRopeUpgradeMat(level);

        ropeRenderer.material = RopeMat;

        if (ropeRenderer != null && ropeRenderer.material != null)
        {
            SetMaterialIntensity(minIntensity);
        }

        SetRopeScale();

    }


    public void SetRopeScale()
    {
        float basescale = 0.25f;
        float level = GameRoot.Instance.UserData.Upgradedatas[(int)UpgradeSystem.UpgradeType.RopeUpgrade].GetUpgradeOrder * 0.1f;
        
        Vector3 targetScale = new Vector3(basescale + level, RopeObj.transform.localScale.y, basescale + level);
        
        // 기존 트윈이 있다면 중단
        RopeObj.transform.DOKill();
        
        // 부드러운 스케일 트윈 애니메이션 (0.3초 동안 Ease.OutBack 효과)
        RopeObj.transform.DOScale(targetScale, 0.3f).SetEase(Ease.OutBack);
    }


    public void SetRopeDirection(System.Action endaction = null)
    {
        // 이미 애니메이션 중이면 중단
        if (isAnimating)
        {
            StopAllCoroutines();
            isAnimating = false;
        }

        var level = GameRoot.Instance.UserData.Upgradedatas[(int)UpgradeSystem.UpgradeType.RopeUpgrade].GetUpgradeOrder;
        Material newMaterial = Config.Instance.GetRopeUpgradeMat(level);


        OnUpgradeEffectComplete = endaction;


        // 업그레이드 효과 애니메이션 시작
        StartCoroutine(UpgradeEffectCoroutine(newMaterial));
    }

    /// <summary>
    /// 머티리얼의 intensity 값을 설정합니다
    /// </summary>
    private void SetMaterialIntensity(float intensity)
    {
        if (ropeRenderer != null && ropeRenderer.material != null)
        {
            // 화이트 Emission 컬러에 intensity 적용
            if (ropeRenderer.material.HasProperty("_EmissionColor"))
            {
                // 화이트 컬러에 intensity 값을 곱해서 설정
                Color emissionColor = whiteEmission * Mathf.Pow(2f, intensity);
                ropeRenderer.material.SetColor("_EmissionColor", emissionColor);
            }
            // 다른 intensity 프로퍼티들도 시도
            else if (ropeRenderer.material.HasProperty("_Intensity"))
            {
                ropeRenderer.material.SetFloat("_Intensity", intensity);
            }
            else if (ropeRenderer.material.HasProperty("_EmissionIntensity"))
            {
                ropeRenderer.material.SetFloat("_EmissionIntensity", intensity);
            }
            else if (ropeRenderer.material.HasProperty("_GlowIntensity"))
            {
                ropeRenderer.material.SetFloat("_GlowIntensity", intensity);
            }
        }
    }

    /// <summary>
    /// 업그레이드 효과 애니메이션 코루틴
    /// </summary>
    private IEnumerator UpgradeEffectCoroutine(Material newMaterial)
    {
        isAnimating = true;

        // 1단계: intensity를 -10에서 3으로 증가 (화이트 emission 연출)
        yield return StartCoroutine(AnimateIntensity(minIntensity, maxIntensity, animationDuration * 0.4f));


        SetRopeScale();
        
        // 2단계: 머티리얼 변경
        if (ropeRenderer != null)
        {
            RopeMat = newMaterial;
            ropeRenderer.material = RopeMat;

            // 머티리얼 변경 후 잠시 최대 intensity 유지
            SetMaterialIntensity(maxIntensity);
        }

        // 잠시 대기 (변경된 머티리얼을 보여주기 위해)
        yield return new WaitForSeconds(0.2f);


        // 3단계: intensity를 3에서 -10으로 감소
        yield return StartCoroutine(AnimateIntensity(maxIntensity, minIntensity, animationDuration * 0.4f));

        isAnimating = false;

        // 액션 호출 (업그레이드 효과 완료)
        OnUpgradeEffectComplete?.Invoke();
    }

    /// <summary>
    /// Intensity 애니메이션 코루틴
    /// </summary>
    private IEnumerator AnimateIntensity(float startIntensity, float endIntensity, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Ease-in-out 곡선 적용
            t = t * t * (3f - 2f * t);

            float currentIntensity = Mathf.Lerp(startIntensity, endIntensity, t);
            SetMaterialIntensity(currentIntensity);

            yield return null;
        }

        // 최종 값 설정
        SetMaterialIntensity(endIntensity);
    }

    /// <summary>
    /// 현재 애니메이션 중인지 확인
    /// </summary>
    public bool IsAnimating()
    {
        return isAnimating;
    }

    /// <summary>
    /// 애니메이션 지속 시간 설정
    /// </summary>
    public void SetAnimationDuration(float duration)
    {
        animationDuration = duration;
    }

    /// <summary>
    /// Intensity 범위 설정
    /// </summary>
    public void SetIntensityRange(float min, float max)
    {
        minIntensity = min;
        maxIntensity = max;
    }

    /// <summary>
    /// Emission 컬러 설정
    /// </summary>
    public void SetEmissionColor(Color color)
    {
        whiteEmission = color;
    }
}
