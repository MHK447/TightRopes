using UnityEngine;
using System;

public class AppTrackingTransparency : MonoBehaviour
{
    public event Action sentTrackingAuthorizationRequest;

    public void Start()
    {
        #if UNITY_IOS
            // ATTManager 인스턴스가 있는지 확인
            if (ATTManager.Instance != null)
            {
                // ATT 상태가 결정되지 않은 경우에만 요청
                if (ATTManager.Instance.GetTrackingAuthorizationStatus() == ATTManager.ATTStatus.NotDetermined)
                {
                    ATTManager.Instance.RequestATT();
                    sentTrackingAuthorizationRequest?.Invoke();
                }
            }
            else
            {
                Debug.LogWarning("ATTManager 인스턴스를 찾을 수 없습니다. ATTManager가 씬에 있는지 확인하세요.");
            }
        #endif
    }
}