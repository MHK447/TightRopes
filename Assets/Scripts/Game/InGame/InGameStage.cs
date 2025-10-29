using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine;
using UniRx;
using DG.Tweening;
using System.Linq;
using BanpoFri;
using UnityEngine.UI;
using System.Collections;
public class InGameStage : MonoBehaviour
{
    public enum InGameState
    {
        NoneInit,
        WaitPlay,
        Playing,
        Direction,
    }

    public InGameState CurState { get; private set; } = InGameState.NoneInit;

    public InGamePlayer Player;

    public Transform StartTr;

    public Transform EndTr;

    [SerializeField]
    private GameObject HighScoreObj;

    [HideInInspector]
    public float DeadYPos = 190f;

    [SerializeField]
    private GameObject ClearEffectObj;

    public RopeComponent RopeComponent;

    [SerializeField]
    private List<ProductEntityComponent> ProductEntityList = new List<ProductEntityComponent>();

    [HideInInspector]
    public bool IsTutorialScreen = false;

    public int ProductEntityCount { get { return ProductEntityList.Count; } }

    public void StartPlaying()
    {
        ProjectUtility.SetActiveCheck(ClearEffectObj, false);
        if (CurState == InGameState.WaitPlay)
        {
            SetState(InGameState.Playing);

            GameRoot.Instance.UISystem.GetUI<HudTotal>()?.Hide();
            GameRoot.Instance.UISystem.GetUI<PopupInGameLobby>()?.Hide();
            GameRoot.Instance.UISystem.OpenUI<PopupInGame>(popup => popup.Init());
            Player.PlayGame();
            HighScoreInit();
            RopeComponent.Init();

            GameRoot.Instance.UserData.RaceData.DataClear();

            foreach (var product in ProductEntityList)
            {
                product.Init();
            }
        }
    }

    public void ReadyPlayingGame()
    {

        if (!GameRoot.Instance.TutorialSystem.IsClearTuto(TutorialSystem.Tuto_1) && GameRoot.Instance.UserData.Stageidx.Value == 2)
        {
            GameRoot.Instance.TutorialSystem.StartTutorial(TutorialSystem.Tuto_1, true);
        }

        GameRoot.Instance.UserData.RaceData.DataClear();
        ProjectUtility.SetActiveCheck(ClearEffectObj, false);
        ActiveHighScoreObj(false);
        SetState(InGameState.WaitPlay);
        GameRoot.Instance.UISystem.OpenUI<PopupInGameLobby>(popup => popup.Init());
        GameRoot.Instance.UISystem.OpenUI<HudTotal>();
        GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.Hide();
        Player.ReadyPlayr();
        GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().GetMainCam.SetFocus(true);


    }


    private void GameStartCheck()
    {


        bool inputDetected = false;

        // 모바일 터치 입력 처리
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                if (!IsPointerOverUI(touch.position))
                {
                    inputDetected = true;
                }
            }
        }
        // PC 마우스 입력 처리
        else if (Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverUI(Input.mousePosition))
            {
                inputDetected = true;
            }
        }

        if (inputDetected && !Player.IsDead && !Player.IsDeadWait && !GameRoot.Instance.TutorialSystem.IsActive())
        {
            StartPlaying();
        }
    }

    private bool IsPointerOverUI(Vector2 screenPosition)
    {

        if (CurState != InGameState.WaitPlay)
            return false;

        // UI 위에 있는지 체크 (EventSystem 사용)
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            var eventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
            {
                position = screenPosition
            };

            var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
            UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventData, results);


            foreach (var result in results)
            {
                if (result.gameObject.layer == LayerMask.NameToLayer("UI"))
                {
                    return true;
                }

                if (result.gameObject.GetComponent<Button>() != null)
                {
                    return true;
                }
            }

            return false;
        }

        return false;
    }


    public void RetryGame()
    {
        GameRoot.Instance.UISystem.OpenUI<HudTotal>();

        SetState(InGameState.WaitPlay);

        int incomevalue = (int)(GameRoot.Instance.UserData.Incomemultivalue * 100);

        System.Numerics.BigInteger rewardvalue = (System.Numerics.BigInteger)Mathf.Round(GameRoot.Instance.UserData.RaceData.RaceDistanceProperty.Value) * incomevalue;

        if (rewardvalue < 0)
        {
            rewardvalue = 0;
        }
        rewardvalue = rewardvalue / 100;

        GameRoot.Instance.UISystem.OpenUI<PageStageClearReward>(popup => popup.Set(Mathf.RoundToInt(GameRoot.Instance.UserData.Highscorevalue), rewardvalue));

        // ProjectUtility.SetRewardAndEffect((int)Config.RewardType.Currency, (int)Config.CurrencyID.Money, rewardvalue, () =>
        // {
        //     GameRoot.Instance.WaitTimeAndCallback(1f, () =>
        //     {
        //         GameRoot.Instance.UISystem.OpenUI<PageFade>(popup => popup.Set(() =>
        //         {
        //             ReadyPlayingGame();
        //         }));
        //     });
        // });
    }

    public void StageClearEnd()
    {
        ProjectUtility.SetActiveCheck(ClearEffectObj, true);
        SetState(InGameState.WaitPlay);
        Player.StageClearEnd();

        SoundPlayer.Instance.PlaySound("explosion");

        GameRoot.Instance.UISystem.GetUI<PageScreenTouch>()?.Hide();

        GameRoot.Instance.WaitTimeAndCallback(2f, () =>
        {
            GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.Hide();


            int incomevalue = (int)(GameRoot.Instance.UserData.Incomemultivalue * 100);

            System.Numerics.BigInteger rewardvalue = (System.Numerics.BigInteger)Mathf.Round(GameRoot.Instance.UserData.RaceData.RaceDistanceProperty.Value) * incomevalue;

            if (rewardvalue < 0)
            {
                rewardvalue = 0;
            }
            rewardvalue = rewardvalue / 100;

            rewardvalue *= 5;

            GameRoot.Instance.UISystem.OpenUI<PopupStageClear>(popup => popup.Set(rewardvalue, () => NextStage(GameRoot.Instance.UserData.Stageidx.Value + 1)));
        });
    }



    public void StageClearCheck()
    {
        if (ProductEntityList.Count(x => x.gameObject.activeSelf) == 0)
        {
            StageClearEnd();
        }
    }

    public void NextStage(int stageidx)
    {
        GameRoot.Instance.UserData.ResetRecordCount(Config.RecordCountKeys.AdCycleCount, 0);
        GameRoot.Instance.UserData.Highscorevalue = 0;
        // 스테이지 인덱스 증가
        GameRoot.Instance.UserData.Stageidx.Value = stageidx;

        foreach (var upgrade in GameRoot.Instance.UserData.Upgradedatas)
        {
            upgrade.Upgradelevel.Value = 1;
        }

        // 로딩 화면 표시
        GameRoot.Instance.Loading.Show();

        // 현재 스테이지 정리 후 새 스테이지 로드
        GameRoot.Instance.WaitTimeAndCallback(0.5f, () =>
        {
            // 현재 스테이지 오브젝트 파괴
            if (this.gameObject != null)
            {
                Destroy(this.gameObject);
            }

            // 새 스테이지 로드
            GameRoot.Instance.StartCoroutine(GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().WaitStageLoad());
        });



        GameRoot.Instance.UserData.Save();
    }


    void Update()
    {
        GameStartCheck();
    }

    public void HighScoreInit()
    {
        var scorevalue = GameRoot.Instance.UserData.Highscorevalue;

        ActiveHighScoreObj(scorevalue > 0);

        // EndTr이 StartTr보다 앞에 있는지(z값이 더 큰지) 뒤에 있는지(z값이 더 작은지) 판단
        bool isEndTrAhead = EndTr.position.z > StartTr.position.z;

        float zpos;
        if (isEndTrAhead)
        {
            // EndTr이 앞에 있으면 StartTr에서 scorevalue만큼 앞으로 (+ 방향)
            zpos = StartTr.position.z + scorevalue;
        }
        else
        {
            // EndTr이 뒤에 있으면 StartTr에서 scorevalue만큼 뒤로 (- 방향)
            zpos = StartTr.position.z - scorevalue;
        }

        HighScoreObj.transform.position = new Vector3(HighScoreObj.transform.position.x, HighScoreObj.transform.position.y, zpos);

        Debug.Log($"HighScore Position: EndTr이 {(isEndTrAhead ? "앞" : "뒤")}에 있음, StartTr.z: {StartTr.position.z}, EndTr.z: {EndTr.position.z}, 계산된 zpos: {zpos}");
    }

    public void ActiveHighScoreObj(bool value)
    {
        ProjectUtility.SetActiveCheck(HighScoreObj, value);
    }


    public void SetState(InGameState state)
    {
        CurState = state;
    }


    public void CallStartGame()
    {
        GameRoot.Instance.StartCoroutine(StartGame());
    }




    public IEnumerator StartGame()
    {
        Player.Init();
        RopeComponent.Init();
        ReadyPlayingGame();
        SetState(InGameState.WaitPlay);
        ProjectUtility.SetActiveCheck(ClearEffectObj, false);
        yield return new WaitUntil(() => CurState == InGameState.WaitPlay);
    }


}
