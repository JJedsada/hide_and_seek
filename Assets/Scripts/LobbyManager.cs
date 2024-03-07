using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public Action onJoinedRoom;
    public Action onLeavedRoom;

    public Action<Player> onPlayerEntryRoom;
    public Action<Player> onPlayerLeaveRoom;

    public Action<List<RoomInfo>> onRoomListUpdate;



    public void CreateRoom(string name)
    {
        PhotonNetwork.CreateRoom(name, new RoomOptions() { PublishUserId = true, MaxPlayers = 4 });
    }

    public void JoinRoom(string name)
    {
        PhotonNetwork.JoinRoom(name);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        onJoinedRoom?.Invoke();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        onLeavedRoom?.Invoke();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        onRoomListUpdate?.Invoke(roomList);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnPlayerEnteredRoom(Player player)
    {
        base.OnPlayerEnteredRoom(player);
        onPlayerEntryRoom?.Invoke(player); 
    }

    public override void OnPlayerLeftRoom(Player player)
    {
        base.OnPlayerLeftRoom(player);
        onPlayerLeaveRoom?.Invoke(player);
    }
}
