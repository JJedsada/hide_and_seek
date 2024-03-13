using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public Dictionary<string, Player> playerDataInMatch = new Dictionary<string, Player>();
    public Dictionary<int, string> playerDataViewIds = new Dictionary<int, string>();

    public Action onJoinedRoom;
    public Action onLeavedRoom;

    public Action<Player> onPlayerEntryRoom;
    public Action<Player> onPlayerLeaveRoom;

    public Action<List<RoomInfo>> onRoomListUpdate;

    public void ReceiveViewId(string userId, int viewId)
    {
        playerDataViewIds.Add(viewId, userId);
    }

    #region Create Room
    public void CreateRoom(string name)
    {
        playerDataViewIds.Clear();
        playerDataInMatch.Clear();

        PhotonNetwork.CreateRoom(name, new RoomOptions() { PublishUserId = true, MaxPlayers = 20 });
        UIManager.OpenLoading();
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom(); 
        UIManager.CloseLoading();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message); 
        UIManager.CloseLoading();
        UIManager.ShowPopup(message);
    }
    #endregion

    #region Join Room
    public void JoinRoom(string name)
    {
        UIManager.OpenLoading();
        PhotonNetwork.JoinRoom(name);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        onJoinedRoom?.Invoke();

        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            playerDataInMatch.Add(player.UserId, player);
        }
        UIManager.CloseLoading();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        UIManager.CloseLoading();
        UIManager.ShowPopup(message);
    }
    #endregion

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        onLeavedRoom?.Invoke();

        playerDataViewIds.Clear();
        playerDataInMatch.Clear();
        GameManager.Instance.CharacterManager.ClearCharacterModel();
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
        playerDataInMatch.Add(player.UserId, player);
    }

    public override void OnPlayerLeftRoom(Player player)
    {
        base.OnPlayerLeftRoom(player);
        onPlayerLeaveRoom?.Invoke(player);
        playerDataInMatch.Remove(player.UserId);
    }
}
