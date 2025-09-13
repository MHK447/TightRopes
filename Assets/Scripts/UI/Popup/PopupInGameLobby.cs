using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using System.Collections.Generic;


public class PopupInGameLobby : UIBase
{
    [SerializeField]
    private List<LobbyUpgradeComponent> LobbyUpgradeComponents = new List<LobbyUpgradeComponent>();




    public void Init()
    {

        for(int i = 0; i < LobbyUpgradeComponents.Count; i++)
        {
            LobbyUpgradeComponents[i].Set(i);
        }
    }

}
