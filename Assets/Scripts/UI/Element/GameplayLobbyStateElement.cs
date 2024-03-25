using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.UI;

public class GameplayLobbyStateElement : Panel
{
    private GameplayPanel GameplayPanel => UIManager.Instance.GameplayPanel;

    [SerializeField] private Button buttonStart;
    [SerializeField] private Button buttonReady;

    private Action onStart;
    private Action<bool> onReady;
    private Action<bool> onUpdateReadyDisplay;

    public override void Initialize()
    {
        buttonStart.onClick.RemoveAllListeners();
        buttonStart.onClick.AddListener(OnStartGame);

        buttonReady.onClick.RemoveAllListeners();
        buttonReady.onClick.AddListener(OnReady);
    }

    public void AddListener(Action Onstart, Action<bool> OnReady, Action<bool> OnUpdateReadyDisplay)
    {
        onStart = Onstart;
        onReady = OnReady;
        onUpdateReadyDisplay = OnUpdateReadyDisplay;
    }

    public void SetupButton()
    {
        bool IsMasterClient = PhotonNetwork.IsMasterClient;
        buttonStart.gameObject.SetActive(IsMasterClient);
        buttonReady.gameObject.SetActive(!IsMasterClient);
    }

    private void OnStartGame()
    {
        onStart?.Invoke();
    }

    private void OnReady()
    {
       
        var localPlayer = PhotonNetwork.LocalPlayer;
        var playerElement = GameplayPanel.PlayersInLobby[localPlayer.UserId];
        bool isReady = !playerElement.IsReady;
        Debug.Log("OnReady " + isReady);
        onUpdateReadyDisplay?.Invoke(isReady);
        onReady?.Invoke(isReady);
    }
}
