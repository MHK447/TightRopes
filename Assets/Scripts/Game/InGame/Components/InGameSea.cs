using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using BanpoFri;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class InGameSea : MonoBehaviour
{
    [SerializeField]
    private List<Transform> FishSpawnList = new List<Transform>();

    private int SeaIdx = 0;

    [SerializeField]
    private List<GameObject> SeaListPrefab = new List<GameObject>();

    private GameObject CurrentSeaObj;


    private List<GameObject> CurrentSeaList = new List<GameObject>();

    public int GetSeaidx { get { return SeaIdx; } }

    [SerializeField]
    private GameObject FishPrefab;

    [SerializeField]
    private GameObject GradientObj;


    private List<InGameFish> FishList = new List<InGameFish>();

    private int Depth = 0;

    private InGameScrollSea ScrollSea;

    public void Init()
    {
        FishList.Clear();
        CurrentSeaList.Clear();
        foreach (var fishspawn in FishSpawnList)
        {
            var fish = Instantiate(FishPrefab, transform).GetComponent<InGameFish>();

            fish.transform.SetParent(this.transform, false);
            fish.transform.localPosition = fishspawn.localPosition;

            FishList.Add(fish);

        }
    }

    public void Set(int seaidx)
    {
        SeaIdx = seaidx;

        ScrollSea = GameRoot.Instance.InGameSystem.GetInGame<InGameTycoon>().GetScrollSea;

        var td = Tables.Instance.GetTable<SeaInfo>().GetData(seaidx);

        if (td != null)
        {

            for (int i = 0; i < FishList.Count; ++i)
            {
                var randvalue = Random.Range(0, td.inhabit_fish.Count);

                FishList[i].Set(td.inhabit_fish[randvalue]);
            }

            ProjectUtility.SetActiveCheck(GradientObj, td.gradation_on == 1);
        }
    }


    public void SeaCreate(int seaidx)
    {
        var td = Tables.Instance.GetTable<SeaInfo>().GetData(seaidx);

        if (td != null)
        {
            if(CurrentSeaObj != null)
            {
                ProjectUtility.SetActiveCheck(CurrentSeaObj , false);
            }


            var findsea = CurrentSeaList.Find(x => !x.gameObject.activeSelf);

            if (findsea != null)
            {
                ProjectUtility.SetActiveCheck(findsea.gameObject, true);

                CurrentSeaObj = findsea;
            }
            else
            {
                var sea = Instantiate(SeaListPrefab[seaidx - 1], transform);

                CurrentSeaObj = sea;

                CurrentSeaList.Add(sea);
            }
        }
    }

    public InGameFish RandCatchFish()
    {
        // 활성화된 물고기들 중에서 아직 잡히지 않은 물고기들만 필터링
        var availableFishes = FishList.FindAll(x => x.gameObject.activeSelf && !x.IsCaught()).ToList();

        if (availableFishes.Count > 0)
        {
            int randomIndex = Random.Range(0, availableFishes.Count);
            return availableFishes[randomIndex];
        }

        return null;
    }

    public InGameFish GetClosestFish(Vector3 targetPosition)
    {
        // 활성화된 물고기들 중에서 아직 잡히지 않은 물고기들만 필터링
        var availableFishes = FishList.FindAll(x => x.gameObject.activeSelf && !x.IsCaught()).ToList();

        if (availableFishes.Count == 0)
            return null;

        InGameFish closestFish = null;
        float closestDistance = float.MaxValue;

        foreach (var fish in availableFishes)
        {
            float distance = Vector3.Distance(fish.transform.position, targetPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestFish = fish;
            }
        }

        return closestFish;
    }

    public List<InGameFish> GetActiveFishes()
    {
        return FishList.FindAll(x => x.gameObject.activeSelf && !x.IsCaught()).ToList();
    }

    public float GetDistanceToPoint(Vector3 point)
    {
        return Vector3.Distance(transform.position, point);
    }
}
