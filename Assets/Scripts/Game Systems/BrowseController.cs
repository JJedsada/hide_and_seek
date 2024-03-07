using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrowseController 
{
    private BrowsePanel browsePanel => UIManager.Instance.BrowsePanel;

    public void Initialize()
    {
        browsePanel.onCreateRoom = OnCreateRoom;
        browsePanel.onJoinRoom = OnJoinRoom;

        GameManager.Instance.Lobby.onJoinedRoom = OnJoinedRoom;
        GameManager.Instance.Lobby.onRoomListUpdate = OnRoomListUpdate;
    }

    public void Present()
    {
        browsePanel.Open();
    }

    private void OnCreateRoom(string roomNameText)
    {
        string roomname = roomNameText;

        if (string.IsNullOrEmpty(roomname))
            roomname = "let go hide and seek";

        GameManager.Instance.Lobby.CreateRoom(roomname);
    }

    private void OnJoinRoom(string roomname)
    {
        GameManager.Instance.Lobby.JoinRoom(roomname);
    }

    private void OnRoomListUpdate(List<RoomInfo> roomInfos)
    {
        browsePanel.SetupRoomDisplay(roomInfos);
    }

    private void OnJoinedRoom()
    {
        browsePanel.Close();
        GameManager.Instance.GameController.Present();
    }
}
