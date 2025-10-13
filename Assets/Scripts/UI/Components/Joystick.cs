using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BanpoFri;
using UnityEngine.UIElements;

public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] UnityEngine.UI.Image _joystickBack;
    [SerializeField] UnityEngine.UI.Image _joystickHead;

    RectTransform _backRectTransform;
    RectTransform _headRectTransform;

    InputHandler _inputHandler;

    //줌
    float zoomSpeed = 0.01f;
    float _defaultCamera = 10;
    float _minCamera;
    float _maxCamera;

    //조이스틱 전체 반지름, 소수화
    float _radius;
    float _radiusDecimal;

    //이동속도 보정 수치 (for default = 1)
    float _speedCorrection = 50f;
    
    // 조이스틱 입력 세기 배율
    float _inputMultiplier = 0.4f;

    bool _isTouch;
    bool _isDrag;
    Vector3 _vectorMove;

    InGamePlayer _player;

    public bool IsLock  = false;

    private void Start()
    {
        _backRectTransform = _joystickBack.rectTransform;
        _headRectTransform = _joystickHead.rectTransform;

        _radius = _backRectTransform.rect.width * 0.5f;
        _radiusDecimal = 1 / (_radius * _speedCorrection);

        _inputHandler = new InputHandler();
    }

    public void Init()
    {
        _player = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().StageMap.Player;

        ProjectUtility.SetActiveCheck(this.gameObject, true);
        ProjectUtility.SetActiveCheck(_joystickBack.gameObject, false);

    }

    public void ActiveJoystice(bool active)
    {
        _joystickBack.enabled = active;
        _joystickHead.enabled = active;
    }

    // 터치 위치로 해당 오브젝트 이동 후, OnDrag 적용하기 위해 FixedUpdate로 작성
    private void FixedUpdate()
    {
        // 조이스틱 입력이 있고 잠금 상태가 아닐 때 플레이어에게 입력 전달
        if (_isTouch && !IsLock)
        {
            if (_player != null)
            {
                // 입력값을 강하게 만들고 Time.fixedDeltaTime 제거하여 즉시 반응
                var balancevalue = _vectorMove * _inputMultiplier;

                Debug.Log("balancevalue: " + balancevalue + ", _vectorMove: " + _vectorMove);
                // 강화된 입력값 전달
                _player.InputBalance(balancevalue);
            }
        }

// #if UNITY_EDITOR
//         if (_player == null) { return; }

//         // 키보드 입력 처리 (에디터용) - 더 강한 입력값
//         if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
//         {
//             _player.InputBalance(new Vector3(-1f, 0f, -1f));
//         }
//         else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
//         {
//             _player.InputBalance(new Vector3(1f, 0f, -1f));
//         }

//         if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
//         {
//             _player.InputBalance(new Vector3(0f, 1f, -1f));
//         }
//         else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
//         {
//             _player.InputBalance(new Vector3(0f, -1f, -1f));
//         }
// #endif
    }

    void OnTouch(Vector2 vectorTouch)
    {
        Vector2 vec = vectorTouch - (Vector2)_backRectTransform.position;
        vec = Vector2.ClampMagnitude(vec, _radius);
        _headRectTransform.localPosition = vec;
        Vector2 vectorNormal = vec.normalized;

        //조이스틱 중앙과 조이스틱 헤드의 거리 (sqrMagnitude = 연산빠름) + 반지름 크기 비례
        float sqr = (Vector3.zero - _headRectTransform.transform.localPosition).sqrMagnitude * _radiusDecimal;
        //Debug.Log("$ sqr = " + sqr);

        _vectorMove = new Vector2(vectorNormal.x, vectorNormal.y) * sqr;
    }

    public void OnDrag(PointerEventData eventData)
    {
#if !UNITY_EDITOR
        if (Input.touchCount != 1) { return; }
#endif
        OnTouch(eventData.position);
        _isTouch = true;
        _isDrag = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(IsLock) return;

        ProjectUtility.SetActiveCheck(_joystickBack.gameObject, true);

#if !UNITY_EDITOR
        if (Input.touchCount != 1) { return; }
#endif
        _joystickBack.transform.position = Input.mousePosition;
        OnTouch(eventData.position);
        _isTouch = true;
        

        //UIManager.Instance.CloseTimePackageArea();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ProjectUtility.SetActiveCheck(_joystickBack.gameObject, false);

        if (_isTouch && !_isDrag)
        {
            _inputHandler.OnTouch(Input.mousePosition);
        }

        _headRectTransform.localPosition = Vector2.zero;
        _vectorMove = Vector2.zero; // 입력 벡터 리셋
        _isTouch = false;
        _isDrag = false;
    }
}
