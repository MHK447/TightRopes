using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using BanpoFri;
using UniRx;
using System.Linq;
using UnityEngine.UI;

public class InGameBase : InGameMode
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

    public override void Load()
    {
        base.Load();
        CallStartGame();
    }


    public void CallStartGame()
    {
        GameRoot.Instance.StartCoroutine(StartGame());
    }




    public IEnumerator StartGame()
    {
        SetState(InGameState.NoneInit);
        Player.Init();
        SetState(InGameState.WaitPlay);
        yield return new WaitUntil(() => CurState == InGameState.WaitPlay);
    }

    public void SetState(InGameState state)
    {
        CurState = state;
    }



    protected override void LoadUI()
    {
        base.LoadUI();
        GameRoot.Instance.InGameSystem.InitPopups();
        GameRoot.Instance.UISystem.OpenUI<HudTotal>();
        GameRoot.Instance.UISystem.OpenUI<PopupInGameLobby>(popup => popup.Init());
    }



    public override void UnLoad()
    {
        base.UnLoad();
    }

    public void StartPlaying()
    {
        if (CurState == InGameState.WaitPlay)
        {
            SetState(InGameState.Playing);

            GameRoot.Instance.UISystem.GetUI<HudTotal>()?.Hide();
            GameRoot.Instance.UISystem.GetUI<PopupInGameLobby>()?.Hide();
            GameRoot.Instance.UISystem.OpenUI<PopupInGame>();
            Player.PlayGame();

            GameRoot.Instance.UserData.RaceData.Init();
        }
    }




    protected override void Update()
    {
        base.Update();
        HandleInput();
    }

    private void HandleInput()
    {
        if (CurState != InGameState.WaitPlay)
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


            foreach(var result in results)
            {
                if(result.gameObject.GetComponent<Button>() != null)
                {
                    return true;
                }
            }

            return false;
        }

        return false;
    }
}
