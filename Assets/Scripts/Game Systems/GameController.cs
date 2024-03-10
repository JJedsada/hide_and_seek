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

    private string[] playerDeadInRound;

    private bool IsSeek;

    private int currentRound;
    private int seekCount;
    private int playerAlive;
    private int playerCount;

    public void Initialize()
    {
        gameplayPanel.onStart = OnStartGame;
        gameplayPanel.onReady = OnReady;
        gameplayPanel.onLeave = OnLeaveRoom;
        gameplayPanel.onAction = OnAction;

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
        RpcExcute.instance.Rpc_SendUpdateReadyState(isReady);
    }

    private void OnLeaveRoom()
    {
        GameManager.Instance.Lobby.LeaveRoom();

        gameplayPanel.Close();

        //Todo : Clear Gameobject and GameLogic

        GameManager.Instance.BrowseController.Present();
    }

    private void OnAction()
    {
        GameManager.Instance.CharacterManager.MainCharater.TriggerAction();
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
        gameplayPanel.AddPlayerDisplay(player);
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
        gameplayPanel.ShowRole(IsSeek);
        GameManager.Instance.CharacterManager.MainCharater.SetupRole(IsSeek);
    }

    public void SetupHidingState()
    {
        gameplayPanel.SetupStateDisplay("Hiding State");
        
        gameplayPanel.ShowHiding(IsSeek);

        var mainCharacter = GameManager.Instance.CharacterManager.MainCharater;
        mainCharacter.SetupHidingState();
        if (IsSeek)
        {
            mainCharacter.SetMoveAble(false);
            return;
        }
        mainCharacter.SetMoveAble(true);
    }

    public void SetupHuntingState()
    {
        gameplayPanel.SetupStateDisplay("Hunting State");
        gameplayPanel.ShowHunting();

        var mainCharacter = GameManager.Instance.CharacterManager.MainCharater;
        mainCharacter.SetupHuntingState();

        if (IsSeek)
        {
            mainCharacter.SetMoveAble(true); 
            return;
        }
        mainCharacter.SetMoveAble(false);
    }

    public void SetupResultState()
    {
        var mainCharacter = GameManager.Instance.CharacterManager.MainCharater;
        mainCharacter.SetupHuntingState();
        mainCharacter.SetMoveAble(false);

        EndRound();
    }

    #endregion

    public void SetupGameData(string seeksData)
    {
        Seek[] seeks = JsonConvert.DeserializeObject<Seek[]>(seeksData);
        currentSeek = seeks;
        IsSeek = BeSeek();

        currentRound = 1;
        seekCount = currentSeek.Length;
        playerCount = playersInLobby.Count;
        playerAlive = playerCount - seekCount;
    }

    public void EndRound()  
    {
        if (currentRound >= 5)
            return;

        currentRound++;

        if (playerAlive <= 0)
        {
            //Todo : show end game
        }

        if (PhotonNetwork.IsMasterClient)
            return;

        if (playerAlive <= 0)
            RpcExcute.instance.Rpc_SendHidingState();
        else
            RpcExcute.instance.Rpc_SendHidingState();

    }


    private bool BeSeek()
    {
        for (int i = 0; i < currentSeek.Length; i++)
        {
            bool isSeek = currentSeek[i].userId == PhotonNetwork.LocalPlayer.UserId;
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

    public void SetPlayerHiding(string userId, bool isHide, int jarId)
    {
        var character = GameManager.Instance.CharacterManager.GetCharacterModel(userId);

        character.SetHidigModel(isHide, jarId);
    }
}
