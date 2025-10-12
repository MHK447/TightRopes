using UnityEngine;
using System;

public class AppTrackingTransparency : MonoBehaviour
{
    public event Action sentTrackingAuthorizationRequest;

    public void Start()
    {
        #if UNITY_IOS
            if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() ==
                ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                ATTrackingStatusBinding.RequestAuthorizationTracking();
                sentTrackingAuthorizationRequest?.Invoke();
            }
        #endif
    }
}