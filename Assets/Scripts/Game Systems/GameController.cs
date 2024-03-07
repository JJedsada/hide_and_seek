using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController 
{
    private GameplayPanel gameplayPanel => UIManager.Instance.GameplayPanel;

    private Dictionary<string, PlayerElement> playersInLobby => gameplayPanel.playersInLobby;

    private Seek[] currentSeek = new Seek[0];

    public void Initialize()
    {
        gameplayPanel.onStart = OnStartGame;
        gameplayPanel.onReady = OnReady;
        gameplayPanel.onLeave = OnLeaveRoom;

        GameManager.Instance.Lobby.onPlayerEntryRoom = OnPlayerEntryRoom;
        GameManager.Instance.Lobby.onPlayerLeaveRoom = OnPlayerLeftRoom;
    }

    public void Present()
    {
        gameplayPanel.Open();
        gameplayPanel.EnterLobby();
        GameManager.Instance.ChageState(StateType.Lobby);
    }

    #region Evenet Action  
    private void OnStartGame()
    {
        if (!PhotonNetwork.IsMasterClient || PhotonNetwork.CurrentRoom.PlayerCount <= 1)
            return;

        foreach (var player in playersInLobby.Values)
        {
            if (player.playerInfo.UserId == PhotonNetwork.LocalPlayer.UserId)
                continue;

            if (!player.IsReady)
                return;
        }

        RpcExcute.instance.Rpc_SendGameStarting(FindingSeek());
        PhotonNetwork.CurrentRoom.IsOpen = false;
    }

    private void OnReady(bool isReady)
    {
        if (PhotonNetwork.IsMasterClient)
            return;
        RpcExcute.instance.RPC_SendUpdateReadyState(isReady);
    }

    private void OnLeaveRoom()
    {
        GameManager.Instance.Lobby.LeaveRoom();

        gameplayPanel.Close();

        //Todo : Clear Gameobject and GameLogic

        GameManager.Instance.BrowseController.Present();
    }

    public void UpdateReadyState(string actorNumber, bool isReady)
    {
        if (playersInLobby.TryGetValue(actorNumber, out var playerElement))
        {
            playerElement.UpdateReadyState(isReady);
        }
    }

    public void OnPlayerEntryRoom(Player player)
    {
        SetupPlayer(player);
        gameplayPanel.AddPlayerDisplay(player);
    }

    private async void SetupPlayer(Player player)
    {
       bool hasObject = GameManager.Instance.CharacterManager.viewIds.ContainsKey(player.UserId);

        //await UniTask.WaitUntil(true);

        int viewId = GameManager.Instance.CharacterManager.viewIds[player.UserId];
        GameManager.Instance.CharacterManager.character[viewId].Setup(player);

    }

    public void OnPlayerLeftRoom(Player player)
    {
        gameplayPanel.RemovePlayerDisplay(player);
    }
    #endregion

    #region Display
    public void SetupPrepareState()
    {
        gameplayPanel.SetupStateDisplay("Show Role State");
        gameplayPanel.ShowRole(BeSeek());
    }

    public void SetupHidingState()
    {
        gameplayPanel.SetupStateDisplay("Hiding State");
        gameplayPanel.ShowHiding(BeSeek());
        if (BeSeek())
        {
            GameManager.Instance.CharacterManager.MainCharater.SetMoveAble(false);
            return;
        }
        GameManager.Instance.CharacterManager.MainCharater.SetMoveAble(true);
    }

    public void SetupHuntingDisplay()
    {
        gameplayPanel.SetupStateDisplay("Hunting State");
        gameplayPanel.ShowHunting();
        if (BeSeek())
        {
            GameManager.Instance.CharacterManager.MainCharater.SetMoveAble(true);
            return;
        }
        GameManager.Instance.CharacterManager.MainCharater.SetMoveAble(false);
    }
    #endregion

    public void SetupSeek(string seeksData)
    {
        Seek[] seeks = JsonConvert.DeserializeObject<Seek[]>(seeksData);
        currentSeek = seeks;
    }

    private bool BeSeek()
    {
        for (int i = 0; i < currentSeek.Length; i++)
        {
            bool isSeek = currentSeek[i].userId.ToLower() == PhotonNetwork.LocalPlayer.UserId.ToLower();
            Debug.Log($"{currentSeek[i].userId.ToLower()} : {PhotonNetwork.LocalPlayer.UserId.ToLower()} = {isSeek}");
            if (isSeek)
                return true;
        }
        return false;
    }

    private Seek[] FindingSeek()
    {
        var playerList = PhotonNetwork.CurrentRoom.Players.Values.ToList();
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        int seekCount = GameConfig.GetSeekAmount(playerCount);

        Seek[] seekList = new Seek[seekCount];

        for (int i = 0; i < seekCount; i++)
        {
            int seekIndex = Random.Range(0, playerCount);
            var seek = playerList[seekIndex];

            Seek target = new Seek();
            target.userId = seek.UserId;
            target.nickname = seek.NickName;

            seekList[i] = target;
            playerList.RemoveAt(seekIndex);
        }

        return seekList;
    }
}
