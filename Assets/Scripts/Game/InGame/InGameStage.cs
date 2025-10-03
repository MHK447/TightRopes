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
    }

    public InGameState CurState { get; private set; } = InGameState.NoneInit;

    public InGamePlayer Player;

    public Transform StartTr;

    [SerializeField]
    private GameObject HighScoreObj;

    [HideInInspector]
    public float DeadYPos = 190f;



    public void StartPlaying()
    {
        if (CurState == InGameState.WaitPlay)
        {
            SetState(InGameState.Playing);

            GameRoot.Instance.UISystem.GetUI<HudTotal>()?.Hide();
            GameRoot.Instance.UISystem.GetUI<PopupInGameLobby>()?.Hide();
            GameRoot.Instance.UISystem.OpenUI<PopupInGame>();
            Player.PlayGame();
            HighScoreInit();
            
            GameRoot.Instance.UserData.RaceData.DataClear();
        }
    }

    public void ReadyPlayingGame()
    {
        GameRoot.Instance.UserData.RaceData.DataClear();

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
        if (CurState == InGameState.Playing)
            return;

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

        if (inputDetected)
        {
            StartPlaying();
        }
    }

    private bool IsPointerOverUI(Vector2 screenPosition)
    {
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
                if (result.gameObject.GetComponent<Button>() != null)
                {
                    return true;
                }
            }

            return false;
        }

        return false;
    }


    public void EndGame()
    {
        if (CurState != InGameState.Playing) return;

        GameRoot.Instance.UISystem.OpenUI<HudTotal>();

        SetState(InGameState.WaitPlay);

        ProjectUtility.SetRewardAndEffect((int)Config.RewardType.Currency, (int)Config.CurrencyID.Money, 100, () =>
        {
            GameRoot.Instance.WaitTimeAndCallback(1f, () =>
            {
                GameRoot.Instance.UISystem.OpenUI<PageFade>(popup => popup.Set(() =>
                {
                    ReadyPlayingGame();
                }));
            });
        });
    }


    void Update()
    {
        GameStartCheck();
    }

    public void HighScoreInit()
    {
        var scorevalue = GameRoot.Instance.UserData.Highscorevalue;

        ActiveHighScoreObj(scorevalue > 0);

        var zpos = StartTr.position.z - scorevalue;

        HighScoreObj.transform.position = new Vector3(HighScoreObj.transform.position.x, HighScoreObj.transform.position.y, zpos);
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
        SetState(InGameState.NoneInit);
        Player.Init();
        ReadyPlayingGame();
        SetState(InGameState.WaitPlay);
        yield return new WaitUntil(() => CurState == InGameState.WaitPlay);
    }


}
