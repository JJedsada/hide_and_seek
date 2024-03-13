using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController 
{
    private GameplayPanel gameplayPanel => UIManager.Instance.GameplayPanel;

    private Dictionary<string, PlayerElement> playersInLobby => gameplayPanel.playersInLobby;

    private Seek[] currentSeek = new Seek[0];

    private PlayerInJar[] playerDeadInRound;

    public bool IsSeek { get; private set; }
    public int currentRound { get; private set; }
    public int seekCount { get; private set; }
    public int playerAlive { get; private set; }
    public int playerCount { get; private set; }

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
        GameManager.Instance.JarManager.SetDefalt();
        GameManager.Instance.ChangeState(StateType.Lobby);
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
        gameplayPanel.EnterGameplay();
        GameManager.Instance.CharacterManager.MainCharater.SetupRole(IsSeek);
    }

    public void SetupHidingState()
    {
        gameplayPanel.SetupRoundDisplay(currentRound);
        gameplayPanel.SetupStateDisplay("Hiding State");
        
        gameplayPanel.ShowHiding(IsSeek);
    }

    public void SetupHuntingState()
    {
        gameplayPanel.SetupStateDisplay("Hunting State");
        gameplayPanel.ShowHunting();
    }

    public void SetupResultState()
    {
        gameplayPanel.SetupStateDisplay("Result State");
        var mainCharacter = GameManager.Instance.CharacterManager.MainCharater;
        mainCharacter.SetMoveAble(false);
        gameplayPanel.ShowPlayerAlive(playerAlive);
    }
    #endregion

    public void SetupGameData(string seeksData)
    {
        Seek[] seeks = JsonConvert.DeserializeObject<Seek[]>(seeksData);
        currentSeek = seeks;
        IsSeek = BeSeek(PhotonNetwork.LocalPlayer.UserId);

        currentRound = 1;
        seekCount = currentSeek.Length;
        playerCount = playersInLobby.Count;
        playerAlive = playerCount - seekCount;

        foreach (var player in GameManager.Instance.CharacterManager.characterModels.Values)
        {
            if (player.playerInfo.UserId == PhotonNetwork.LocalPlayer.UserId)
                continue;
            var seeker = BeSeek(player.playerInfo.UserId);
            player.SetupRole(seeker);
        }
    }

    public void EndRound()  
    {
        if (currentRound >= 5 || playerAlive <= 0)
        {
            ShowGameSummary();        
            return;
        }
        currentRound++;
        UIManager.Instance.GameplayPanel.SetupRoundDisplay(currentRound);
        SendNextState(false);
    }

    private async void ShowGameSummary()
    {
        var gamePanel = UIManager.Instance.GameplayPanel;
        if (playerAlive <= 0)
            gamePanel.SetAnounmentText("Seek Win");
        else
            gamePanel.SetAnounmentText("Hider Win");

        await UniTask.Delay(3000);
        SendNextState(true);
    }

    private void SendNextState(bool isEndGame)
    {
        if (PhotonNetwork.IsMasterClient)
            return;

        if (isEndGame)
            RpcExcute.instance.Rpc_SendWaittingState();
        else
            RpcExcute.instance.Rpc_SendHidingState();
    }

    public bool BeSeek(string userId)
    {
        for (int i = 0; i < currentSeek.Length; i++)
        {
            bool isSeek = currentSeek[i].userId == userId;
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


    #region Receive Event 
    public void EnterWaittingState()
    {
        CharacterManager characterManager = GameManager.Instance.CharacterManager;

        var mainCharacter = characterManager.MainCharater;
        mainCharacter.OutHide();
        mainCharacter.SetupDefault();

        foreach (var character in characterManager.characterModels.Values)
            character.SetupDefault();
    }

    public void SetPlayerHiding(string userId, bool isHide, int jarId)
    {
        var character = GameManager.Instance.CharacterManager.GetCharacterModel(userId);

        character.SetHideInJar(userId, isHide, jarId);
        character.SetActiveNameDisplay(!IsSeek);
    }

    public async UniTask BreakingJar(string userId, int jarId)
    {
        var jar = GameManager.Instance.JarManager.GetJar(jarId);
        var playerDead = jar.Breaking();

        var character = GameManager.Instance.CharacterManager.GetCharacterModel(userId);
        character.Breaking().Forget();
        SendBreakDamage(userId, playerDead);

        await UniTask.Delay(1000);
        jar.SetActive(false);
    }

    private void SendBreakDamage(string userId, Dictionary<string, CharacterController> playerDead)
    {
        if (userId != PhotonNetwork.LocalPlayer.UserId)
            return;

        int index = 0;
        int deadCount = playerDead.Keys.Count;
        PlayerInJar[] listPlayerTakeDamage = new PlayerInJar[deadCount];

        foreach (string userIdInJar in playerDead.Keys)
        {
            PlayerInJar player = new PlayerInJar();
            player.userId = userIdInJar;
            listPlayerTakeDamage[index] = player;
            index++;
        }

        string json = JsonConvert.SerializeObject(listPlayerTakeDamage);
        RpcExcute.instance.Rpc_SendBreakDamage(json);

        if (deadCount <= 0)
            return;

        var mainCharacter = GameManager.Instance.CharacterManager.MainCharater;
        mainCharacter.Score += playerDead.Count;
        RpcExcute.instance.Rpc_SendUpdateScore(mainCharacter.Score);
    }

    public async UniTask TakeDamage(string playerDataTaking)
    {
        playerDeadInRound = JsonConvert.DeserializeObject<PlayerInJar[]>(playerDataTaking);

        int deadCount = playerDeadInRound.Length;

        playerAlive -= deadCount;

        string[] playerName = new string[playerDeadInRound.Length];
        int index = 0;

        foreach (var player in playerDeadInRound)
        {
            var character = GameManager.Instance.CharacterManager.GetCharacterModel(player.userId);      
            character.Dead().Forget();

            playerName[index] = character.playerInfo.NickName;

            index++;
        }
        await UniTask.Delay(700);
        gameplayPanel.SetupKilledAnounment(playerName).Forget();
    }

    public void UpdateScore(string userId, int score)
    {
        UIManager.Instance.GameplayPanel.playersInLobby[userId].SetupScore(score);
        GameManager.Instance.CharacterManager.GetCharacterModel(userId).Score = score;
    }
    #endregion
}
