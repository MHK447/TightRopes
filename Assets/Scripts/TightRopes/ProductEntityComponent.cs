using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;
using BanpoFri;
public class ProductEntityComponent : MonoBehaviour
{
    private Vector3 StartPosition;

    private Vector3 StartScale;
    void Awake()
    {
        StartPosition = transform.position;
        StartScale = transform.localScale;
    }

    public void Init()
    {
        this.transform.position = StartPosition;

        this.transform.localScale = StartScale;
        ProjectUtility.SetActiveCheck(this.gameObject, true);
    }


    void OnTriggerEnter(Collider other)
    {
        ProjectUtility.Vibrate();

        if (other.gameObject.tag == "Player")
        {
            this.transform.DOScale(0, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                if (this.gameObject.activeSelf)
                {
                   SoundPlayer.Instance.PlaySound("item_get");
                    ProjectUtility.SetActiveCheck(this.gameObject, false);

                    GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().StageMap.StageClearCheck();
                    GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().StageMap.Player.AddProductItem();
                }
            });
        }
    }



    public void OnEnable()
    {
        // 기존 회전 트윈 정리 및 회전값 초기화
        transform.DOKill();
        transform.rotation = Quaternion.identity;

        // 360도 무한 회전 트윈 시작
        transform.DORotate(new Vector3(0, 360, 0), 2f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear);
    }

    public void OnDisable()
    {
        // 오브젝트가 비활성화될 때 트윈 정리
        transform.DOKill();
    }

}
