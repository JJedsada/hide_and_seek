using Cysharp.Threading.Tasks;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameplayPanel : Panel
{
    [SerializeField] private Sprite punchIcon;
    [SerializeField] private Sprite hideIcon;
    [SerializeField] private Sprite defualtIcon;
    [Space]
    [SerializeField] private TMP_Text roomnameText;
    [SerializeField] private TMP_Text readyText;
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private TMP_Text durationStateText;
    [SerializeField] private TMP_Text anounmentText;
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text killAnounmentText;
    [SerializeField] private TMP_Text actionCountText;
    [Space]
    [SerializeField] private Image actionImage;
    [SerializeField] private GameObject countAction;
    [Space]
    [SerializeField] private PlayerElement playerElementPrefab;
    public RectTransform content;
    [Space]
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button actionButton;
    [Space]
    [SerializeField] private GameplayLobbyStateElement lobbyStateElement;
    public GameplaySkillElement skillElement;
    [SerializeField] private GameObject hidingStateElement;
    public FixedJoystick joystick;

    public Dictionary<string, PlayerElement> PlayersInLobby { get; private set; } = new Dictionary<string, PlayerElement>();

    private Action onLeave;
    private Action onAction;
    private Action<int> onSkill;

    public override void Initialize()
    {
        leaveButton.onClick.RemoveAllListeners();
        leaveButton.onClick.AddListener(OnLeaveRoom);
        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(OnAction);

        lobbyStateElement.Initialize();
    }

    public void AddListener(Action OnStart, Action<bool> OnReady, Action OnLeave, Action OnAction, Action<int> OnSkill)
    {
        onLeave = OnLeave;
        onAction = OnAction;
        onSkill = OnSkill;

        lobbyStateElement.AddListener(OnStart, OnReady, SetupReadyDisplay);
        skillElement.AddListener(OnSkill);
    }

    public void EnterLobby()
    {
        lobbyStateElement.Open(); 
        skillElement.Close();
        anounmentText.text = string.Empty;
        actionImage.sprite = defualtIcon; 
        countAction.gameObject.SetActive(false);
        SetupRoomName();
        SetupPlayers();
    }

    public void EnterGameplay()
    {
        skillElement.SetupSkill();
        foreach (var element in PlayersInLobby.Values)
        {
            bool seeker = GameManager.Instance.GameController.BeSeek(element.playerInfo.UserId);
            element.EnterGameplay(seeker);
        }
    }

    public void ShowSkill(bool isShow)
    {
        skillElement.Close();
        if (isShow)
            skillElement.Open();
    }

    #region Display
    private void SetupRoomName()
    {
        roomnameText.text = $"Room : {PhotonNetwork.CurrentRoom.Name}";
        lobbyStateElement.SetupButton();
    }

    public void SetupRoundDisplay(int currentRound)
    {
        roundText.text = $"Round : {currentRound}/5";
    }

    public void SetupStateDisplay(string stateName)
    {
        stateText.text = $"{stateName}";
    }

    public void SetupStateDuration(float duration)
    {
        durationStateText.text = duration.ToString("F0"); 
    }

    public void SetupInteractButton(bool isInteract)
    {
        if (actionButton.interactable == isInteract)
            return;

        actionButton.interactable = isInteract;
    }

    public void SetupPunchAction(int actionCount)
    {
        actionImage.sprite = punchIcon;
        SetupPunchCount(actionCount);
        countAction.gameObject.SetActive(true);
    }

    public void SetupPunchCount(int actionCount)
    {
        actionCountText.text = actionCount.ToString();
    }

    public void SetupHideAction()
    {
        actionImage.sprite = hideIcon;
        countAction.gameObject.SetActive(false);
    }

    public async UniTask SetupKilledAnounment(params string[] playername)
    {
        string text = string.Empty;

        for (int i = 0; i < playername.Length; i++)
        {
            text += $"{playername[i]} was killed.\n";
        }
        killAnounmentText.text = text;
        await UniTask.Delay(2000);
        killAnounmentText.text = string.Empty; 
    }

    private void SetupPlayers()
    {
        var playersPresence = PhotonNetwork.CurrentRoom.Players;

        foreach (Player person in playersPresence.Values)
        {
            AddPlayerDisplay(person);
        }
    }

    public PlayerElement GetElement(string userId)
    {
        return PlayersInLobby[userId];
    }

    public void AddPlayerDisplay(Player newPlayer)
    {
        if (PlayersInLobby.ContainsKey(newPlayer.UserId))
            return;

        var element = Instantiate(playerElementPrefab, content);

        element.Initialize(newPlayer);
        PlayersInLobby.Add(newPlayer.UserId, element);
    }

    public void RemovePlayerDisplay(Player player)
    {
        if (!PlayersInLobby.TryGetValue(player.UserId, out var element))
        {
            return;
        }
        Destroy(element.gameObject);
        PlayersInLobby.Remove(player.UserId);
    }
    #endregion

    #region State Display
    public void ShowRole(bool isSeek)
    {
        lobbyStateElement.Close();
        string anounText = "You are Hider";
        if (isSeek)
            anounText = "You are Finder"; ;
        anounmentText.text = anounText;
    }

    public void ShowHiding(bool isSeek)
    {
        hidingStateElement.SetActive(isSeek);

        if (isSeek)
        {
            anounmentText.text = "Waiting Player Hiding";
            return;
        }

        string hidingText = $"Round {GameManager.Instance.GameController.currentRound}/5\nStart Hiding";
        SetAnounmentText(hidingText);
    }

    public void ShowHunting()
    {
        hidingStateElement.SetActive(false);
        string text = "Start Hunting";
        SetAnounmentText(text);
    }

    public void ShowPlayerAlive(int playerAlive)
    {
        string text = $"Player in alive {playerAlive}";
        SetAnounmentText(text);
    }

    public async void SetAnounmentText(string text)
    {
        anounmentText.text = text;
        await UniTask.Delay(3000);
        anounmentText.text = string.Empty;
    }

    public void DefualtDisplay()
    {
        roundText.text = string.Empty;
        durationStateText.text = string.Empty;
        stateText.text = string.Empty;
        anounmentText.text = string.Empty;

        ElementClearData();
    }

    private void ElementClearData()
    {
        foreach (var element in PlayersInLobby.Values)
        {
            element.EnterLobby();
        }
    }
    #endregion

    #region Event Action
    public void SetupReadyDisplay(bool isReady)
    {
        string readyTxt = isReady ? "UnReady" : "Ready";
        readyText.text = readyTxt;
    }

    private void OnLeaveRoom()
    {
        onLeave?.Invoke();

        foreach (var person in PlayersInLobby.Values)
        {
            Destroy(person.gameObject);
        }
        PlayersInLobby.Clear();
    }

    private void OnAction()
    {
        onAction?.Invoke();
    }
    #endregion
}
