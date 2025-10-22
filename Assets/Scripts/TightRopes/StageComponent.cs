using UnityEngine;
using UnityEngine.UI;
using BanpoFri;
using TMPro;
using DG.Tweening;

public class StageComponent : MonoBehaviour
{
    [SerializeField]
    private Image StageImg;

    [SerializeField]
    private Image StageLockImg;

    [SerializeField]
    private Image StageColorImg;

    [SerializeField]
    private TextMeshProUGUI StageNameText;


    [SerializeField]
    private GameObject LockObj;

    [SerializeField]
    private GameObject NoneLockObj;


    private int StageIdx = 0;

    public int GetStageIdx{ get { return StageIdx; } }

    public void Set(int stageidx)
    {
        var stageinfotd = Tables.Instance.GetTable<StageInfo>().GetData(stageidx);

        if (stageinfotd != null)
        {
            StageIdx = stageidx;


            StageImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Map, $"MapIcon_{stageidx:00}");
            StageImg.color = stageidx <= GameRoot.Instance.UserData.Stageidx.Value ? Color.white : Color.black;

            StageNameText.text = Tables.Instance.GetTable<Localize>().GetString(stageinfotd.name);
            StageNameText.fontSharedMaterial = Config.Instance.TextMaterialList[stageidx - 1];
            StageColorImg.color = Config.Instance.GetImageColor(stageinfotd.image_color);
            ProjectUtility.SetActiveCheck(LockObj, stageidx > GameRoot.Instance.UserData.Stageidx.Value);
            ProjectUtility.SetActiveCheck(NoneLockObj, stageidx <= GameRoot.Instance.UserData.Stageidx.Value);
        }
    }

    public void UnLockAction()
    {
        // 스케일 애니메이션 시퀀스 생성
        Sequence unlockSequence = DOTween.Sequence();
        
        // 1. 스케일을 1.2배로 키우기 (0.3초)
        unlockSequence.Append(transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack));
        
        // 2. 원래 크기로 돌아가기 (0.2초)
        unlockSequence.Append(transform.DOScale(1f, 0.2f).SetEase(Ease.InBack));
        
        // 3. 애니메이션 완료 후 UI 상태 변경
        unlockSequence.OnComplete(() =>
        {
            // Lock 오브젝트 비활성화
            ProjectUtility.SetActiveCheck(LockObj, false);
            
            // NoneLock 오브젝트 활성화
            ProjectUtility.SetActiveCheck(NoneLockObj, true);
            
            // 스테이지 이미지 색상을 흑백에서 컬러로 변경
            StageImg.DOColor(Color.white, 0.5f).SetEase(Ease.OutQuad);
        });
        
        // 애니메이션 시작
        unlockSequence.Play();
    }

}
