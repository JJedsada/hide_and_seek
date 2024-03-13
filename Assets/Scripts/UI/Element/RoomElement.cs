using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomElement : MonoBehaviour
{
    [SerializeField] private TMP_Text name;
    [SerializeField] private TMP_Text size;
    [Space]
    [SerializeField] private Button joinButton;

    public Action<string> onJoin;

    private RoomInfo info;

    private void Start()
    {
        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(OnJoinRoom);
    }

    public void Setup(RoomInfo roomInfo)
    {
        info = roomInfo;
        name.text = $"{roomInfo.Name}";
        size.text = $"{roomInfo.PlayerCount}/{roomInfo.MaxPlayers}";
    }

    private void OnJoinRoom()
    {
        if (!info.IsVisible || !info.IsOpen)
        {
            UIManager.ShowPopup("Game is started.");
            return;
        }
        
        if(info.PlayerCount >= info.MaxPlayers)
        {
            UIManager.ShowPopup("Room is full.");
            return;
        }

        onJoin?.Invoke(info.Name);
    }
}
