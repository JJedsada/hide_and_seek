using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController 
{
    private GameplayPanel gameplayPanel => UIManager.Instance.GameplayPanel;

    public SkillController skillController = new();

    private Dictionary<string, PlayerElement> playersInLobby => gameplayPanel.PlayersInLobby;

    private Seek[] currentSeek = new Seek[0];

    private PlayerInJar[] playerDeadInRound;

    public bool IsSeek { get; private set; }
    public int currentRound { get; private set; }
    public int seekCount { get; private set; }
    public int playerAlive { get; private set; }
    public int playerCount { get; private set; }
    public bool IsTimeless { get; set; }
    public bool IsStarfall { get; set; }

    private List<string> reviveList = new List<string>();
    private int starfallToJarId;

    public void Initialize()
    {
        gameplayPanel.AddListener(OnStartGame, OnReady, OnLeaveRoom, OnAction, OnSkill);
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

    private void OnSkill(int skillIndex)
    {
        GameManager.Instance.CharacterManager.MainCharater.SkillAction(skillIndex);
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

        bool isOpen = IsSeek ? false : true;
        gameplayPanel.ShowSkill(isOpen);

        for (int i = 0; i < currentSeek.Length; i++)
        {
            if (IsSeek)
                return;
            string seekId = currentSeek[i].userId;
            var model = GameManager.Instance.CharacterManager.GetCharacterModel(seekId);      
            model.ShowSeekView(true);
        }
    }

    public void ExitHidingState()
    {
        for (int i = 0; i < currentSeek.Length; i++)
        {
            var model = GameManager.Instance.CharacterManager.GetCharacterModel(currentSeek[i].userId);
            model.ShowSeekView(false);
        }
    }

    public void SetupHuntingState()
    {
        gameplayPanel.SetupStateDisplay("Hunting State");
        gameplayPanel.ShowHunting();

        bool isOpen = IsSeek ? true : false;
        gameplayPanel.ShowSkill(isOpen);
    }

    public void SetupResultState()
    {
        reviveList.Clear();
        gameplayPanel.SetupStateDisplay("");
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
        UIManager.Instance.resultPanel.Open();
        await UniTask.Delay(3000);
        UIManager.Instance.resultPanel.Clear();
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
            int seekIndex = UnityEngine.Random.Range(0, playerCount);
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

    public void StarfallBreakingJar(int jarId)
    {
        var jar = GameManager.Instance.JarManager.GetJar(jarId);
        jar.Breaking();
        jar.SetActive(false);
        GameManager.Instance.JarManager.Starfall(starfallToJarId).Forget();
    }

    private void SendBreakDamage(string userId, Dictionary<string, CharacterController> playerDead)
    {
        if (userId != PhotonNetwork.LocalPlayer.UserId)
            return;

        List<PlayerInJar> listPlayerTakeDamage = new List<PlayerInJar>();
        List<PlayerInJar> listPlayerRevive = new List<PlayerInJar>();

        foreach (string userIdInJar in playerDead.Keys)
        {
            if (IsRevive(userIdInJar))
            {
                PlayerInJar revive = new PlayerInJar();
                revive.userId = userIdInJar;
                listPlayerRevive.Add(revive);
                continue;
            }       

            PlayerInJar player = new PlayerInJar();
            player.userId = userIdInJar;
            listPlayerTakeDamage.Add(player);
        }

        string json = JsonConvert.SerializeObject(listPlayerTakeDamage);
        string reviveJson = JsonConvert.SerializeObject(listPlayerRevive);
        RpcExcute.instance.Rpc_SendBreakDamage(json);
        RpcExcute.instance.Rpc_SendRevive(reviveJson);

        if (listPlayerTakeDamage.Count <= 0)
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

    public async UniTask Revived(string playerDataTaking)
    {
        var reviveInRound = JsonConvert.DeserializeObject<PlayerInJar[]>(playerDataTaking);
        foreach (var player in reviveInRound)
        {
            var character = GameManager.Instance.CharacterManager.GetCharacterModel(player.userId);
            character.Revive().Forget();
        }

        await UniTask.Delay(700);
    }

    public void UpdateScore(string userId, int score)
    {
        UIManager.Instance.GameplayPanel.PlayersInLobby[userId].SetupScore(score);
        GameManager.Instance.CharacterManager.GetCharacterModel(userId).Score = score;
    }

    public void PlayerActiveSkill(string skillJson)
    {
        SkillReqeust skillReqeust = JsonConvert.DeserializeObject<SkillReqeust>(skillJson);
        switch (skillReqeust.skillId)
        {
            case SkillType.Block:
                break;

            case SkillType.Kick:
                break;

            case SkillType.Revice:
                Revive(skillReqeust);
                break;

            case SkillType.Spray: 
                Spray(skillReqeust);
                break;

            case SkillType.Scan:
                Scan(skillReqeust);
                break;
            case SkillType.Starfall:
                Starfall(skillReqeust);
                break;
            case SkillType.Timeless:
                Timeless();
                break;
        }
    }
    #endregion
  
    private bool IsRevive(string userId)
    {
        for (int i = 0; reviveList.Count > 0; i++)
        {
            if (reviveList[i] != userId)
                continue;

            return true;
        }
        return false;
    }

    private void Revive(SkillReqeust skillReqeust)
    {
        reviveList.Add(skillReqeust.playerId);
    }

    private void Spray(SkillReqeust skillReqeust)
    {
        if (IsSeek)
        {
            GameManager.Instance.JarManager.GetJar(skillReqeust.jarId).Spray();
            return;
        }

        if (PhotonNetwork.LocalPlayer.UserId != skillReqeust.playerId)
            return;

        GameManager.Instance.JarManager.GetJar(skillReqeust.jarId).Spray();
    }

    private void Scan(SkillReqeust skillReqeust)
    {
        UIManager.Instance.GameplayPanel.SetAnounmentText("Finder use Scan skills.");
        if (IsSeek)
            GameManager.Instance.JarManager.ScanAll();
    }

    private void Starfall(SkillReqeust skillReqeust)
    {
        IsStarfall = true;
        starfallToJarId = skillReqeust.jarId;
        UIManager.Instance.GameplayPanel.SetAnounmentText("Finder use Starfall skills.");
    }

    public void ActiveStarfall()
    {
        if (!IsStarfall)
            return;

        IsStarfall = false;
        RpcExcute.instance.Rpc_SendStarfall(starfallToJarId);
        UIManager.Instance.GameplayPanel.SetAnounmentText("Starfall.");
    }

    private void Timeless()
    {
        IsTimeless = true;
        UIManager.Instance.GameplayPanel.SetAnounmentText("Finder use timeless skills.");
    }
}
