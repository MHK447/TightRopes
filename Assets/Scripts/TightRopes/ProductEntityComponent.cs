using UnityEngine;
using DG.Tweening;
public class ProductEntityComponent : MonoBehaviour
{
    private Vector3 StartPosition;

    void Awake()
    {
        StartPosition = transform.position;
    }

    public void Init()
    {
        this.transform.position = StartPosition;

        this.transform.localScale = Vector3.one;
        ProjectUtility.SetActiveCheck(this.gameObject , true);
    }


    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            this.transform.DOScale(0, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                ProjectUtility.SetActiveCheck(this.gameObject , false);

                other.GetComponent<InGamePlayer>().AddProductItem();
            });
        }
    }



    public void OnEnable()
    {
        // 360도 무한 회전 트윈 시작
        transform.DORotate(new Vector3(0, 360, 0), 2f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear);
    }

}
