using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayPanel : Panel
{
    [SerializeField] private TMP_Text roomnameText;
    [SerializeField] private TMP_Text readyText;
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private TMP_Text durationStateText;
    [SerializeField] private TMP_Text anounmentText;
    [Space]
    [SerializeField] private PlayerElement playerElementPrefab;
    [SerializeField] private RectTransform content;
    [Space]
    [SerializeField] private Button startButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button actionButton;
    [Space]
    [SerializeField] private GameObject lobbyStateElement;
    [SerializeField] private GameObject hidingStateElement;

    public Dictionary<string, PlayerElement> playersInLobby { get; private set; } = new Dictionary<string, PlayerElement>();

    public Action onStart;
    public Action<bool> onReady;
    public Action onLeave;
    public Action onAction;

    public override void Initialize()
    {
        leaveButton.onClick.RemoveAllListeners();
        leaveButton.onClick.AddListener(OnLeaveRoom);

        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(OnStartGame);

        readyButton.onClick.RemoveAllListeners();
        readyButton.onClick.AddListener(OnReady);

        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(OnAction);
    }

    public void EnterLobby()
    {
        lobbyStateElement.SetActive(true);
        anounmentText.text = string.Empty;
        SetupRoomName();
        SetupPlayers();
    }

    #region Display
    private void SetupRoomName()
    {
        roomnameText.text = $"Room : {PhotonNetwork.CurrentRoom.Name}";

        if (PhotonNetwork.IsMasterClient)
        {
            startButton.gameObject.SetActive(true);
            readyButton.gameObject.SetActive(false);
        }
        else
        {
            startButton.gameObject.SetActive(false);
            readyButton.gameObject.SetActive(true);
        }
    }

    public void SetupStateDisplay(string stateName)
    {
        stateText.text = $"{stateName}";
    }

    public void SetupStateDuration(float duration)
    {
        durationStateText.text = duration.ToString("F0"); 
    }

    private void SetupPlayers()
    {
        var playersPresence = PhotonNetwork.CurrentRoom.Players;

        foreach (Player person in playersPresence.Values)
        {
            AddPlayerDisplay(person);
        }
    }

    public void AddPlayerDisplay(Player newPlayer)
    {
        var element = Instantiate(playerElementPrefab, content);

        element.Initialize(newPlayer);
        playersInLobby.Add(newPlayer.UserId, element);
    }

    public void RemovePlayerDisplay(Player player)
    {
        if (!playersInLobby.TryGetValue(player.UserId, out var element))
        {
            return;
        }
        Destroy(element.gameObject);
        playersInLobby.Remove(player.UserId);
    }
    #endregion

    #region State Display
    public void ShowRole(bool isSeek)
    {
        lobbyStateElement.SetActive(false);
        string anounText = "You are Hides";
        if (isSeek)
            anounText = "You are Seek"; ;
        anounmentText.text = anounText;
    }

    public void ShowHiding(bool isSeek)
    {
        hidingStateElement.SetActive(isSeek);

        string anounText = isSeek ? "Waiting Player Hiding":"Start Hiding";
        anounmentText.text = anounText;
    }

    public void ShowHunting()
    {
        hidingStateElement.SetActive(false);
        anounmentText.text = "Start Hunting";
    }
    #endregion

    #region Event Action
    private void OnStartGame()
    {
        onStart?.Invoke();
    }

    private void OnReady()
    {
        var localPlayer = PhotonNetwork.LocalPlayer;
        var playerElement = playersInLobby[localPlayer.UserId];
        bool isReady = !playerElement.IsReady;

        string readyTxt = isReady ? "UnReady" : "Ready";
        readyText.text = readyTxt;

        onReady?.Invoke(isReady);
    }

    private void OnLeaveRoom()
    {
        onLeave?.Invoke();

        foreach (var person in playersInLobby.Values)
        {
            Destroy(person.gameObject);
        }
        playersInLobby.Clear();
    }

    private void OnAction()
    {
        onAction?.Invoke();
    }
    #endregion
}
