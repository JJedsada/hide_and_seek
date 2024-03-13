using Photon.Pun;
using System.Globalization;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public ServerManager Server;
    public LobbyManager Lobby;

    public LoginController LoginController = new();
    public BrowseController BrowseController = new();
    public GameController GameController = new();

    public CharacterManager CharacterManager = new();
    public JarManager JarManager = new JarManager();
    public StateController GameStateController = new();

    private void Start()
    {
        GameStateController.Initialize();
        UIManager.Instance.Initialized();

        LoginController.Initialize();
        BrowseController.Initialize();
        GameController.Initialize();

        JarManager.Initialize();

        LoginController.Present();
    }

    private void Update()
    {
        GameStateController.Update();
    }

    public void ChangeState(StateType state)
    {
        switch (state)
        {
            case StateType.Lobby:
                GameStateController.ChangeState(new LobbyState());
                break;
            case StateType.Waitting:
                GameStateController.ChangeState(new WaittingState());
                break;
            case StateType.Prepare:
                GameStateController.ChangeState(new PrepareState());
                break;
            case StateType.Hiding:
                GameStateController.ChangeState(new HidingState());
                break;
            case StateType.Hunting:
                GameStateController.ChangeState(new HuntingState());
                break;
            case StateType.Result:
                GameStateController.ChangeState(new ResultState());
                break;
        }
    }
}

public class LobbyState: State
{
    public override void EnterState()
    {
        base.EnterState();

        GameManager.Instance.CharacterManager.SpawnPlayer();
        int viewId = GameManager.Instance.CharacterManager.MainCharater.photonView.ViewID;
        RpcExcute.instance.Rpc_SendPlayerInfo(viewId);
        if (PhotonNetwork.IsMasterClient)
        {
            RpcExcute.instance.Rpc_SendUpdateReadyState(true);
        }

        GameManager.Instance.ChangeState(StateType.Waitting);
    }
}

public class WaittingState : State
{
    public override void EnterState()
    {
        base.EnterState();

        var GameplayPanel = UIManager.Instance.GameplayPanel;
        GameplayPanel.DefualtDisplay();
        GameplayPanel.EnterLobby();
        GameManager.Instance.JarManager.SetDefalt();
        GameManager.Instance.GameController.EnterWaittingState();

        if (PhotonNetwork.IsMasterClient)
            return;

        RpcExcute.instance.Rpc_SendUpdateReadyState(false);
        GameplayPanel.SetupReadyDisplay(false);
    }
}

public class PrepareState : State
{
    private CountDown stateCountDown = new CountDown();

    public override void EnterState()
    {
        base.EnterState();
        ShowRoleYourSelf();
    }

    public override void Update()
    {
        base.Update();

        float deltaTime = Time.deltaTime;
        stateCountDown.Update(deltaTime);
    }

    public void ShowRoleYourSelf()
    {
        GameManager.Instance.GameController.SetupPrepareState();

        stateCountDown.Start(GameConfig.PrepareDuration);

        stateCountDown.onUpdate = OnUpdate;
        stateCountDown.onComplete = OnComplete;

        void OnUpdate(float currentDuration)
        {
            UIManager.Instance.GameplayPanel.SetupStateDuration(currentDuration);
        }

        void OnComplete()
        {
            if (PhotonNetwork.IsMasterClient)
                RpcExcute.instance.Rpc_SendHidingState();
        }
    }
}

public class HidingState : State
{
    private CountDown stateCountDown = new CountDown();
    
    public override void EnterState()
    {
        base.EnterState();
        ShowHidingDisplay();
        SetupMainCharacterState();
    }

    public override void Update()
    {
        base.Update();
        float deltaTime = Time.deltaTime;
        stateCountDown.Update(deltaTime);
    }

    public void ShowHidingDisplay()
    {
        GameManager.Instance.GameController.SetupHidingState();

        stateCountDown.Start(GameConfig.HidingDuration);

        stateCountDown.onUpdate = OnUpdate;
        stateCountDown.onComplete = OnComplete;

        void OnUpdate(float currentDuration)
        {
            UIManager.Instance.GameplayPanel.SetupStateDuration(currentDuration);
        }

        void OnComplete()
        {
            if (PhotonNetwork.IsMasterClient)
                RpcExcute.instance.Rpc_SendHuntingState();
        }       
    }

    private void SetupMainCharacterState()
    {
        var mainCharacter = GameManager.Instance.CharacterManager.MainCharater;
        mainCharacter.SetupHidingState();

        if (GameManager.Instance.GameController.IsSeek)
        {
            mainCharacter.SetMoveAble(false);
            return;
        }
        mainCharacter.SetHideModel(true);
        mainCharacter.SetMoveAble(true);
        mainCharacter.OutHide();
    }
}

public class ResultState : State
{
    private CountDown stateCountDown = new CountDown();

    public override void EnterState()
    {
        base.EnterState();
        SetupResultState();
        UpdateScore();
    }

    public override void Update()
    {
        base.Update();
        float deltaTime = Time.deltaTime;
        stateCountDown.Update(deltaTime);
    }

    public void SetupResultState()
    {
        GameManager.Instance.GameController.SetupResultState();
 
        stateCountDown.Start(GameConfig.ShowResultDuration);

        stateCountDown.onUpdate = OnUpdate;
        stateCountDown.onComplete = OnComplete;

        void OnUpdate(float currentDuration)
        {
            UIManager.Instance.GameplayPanel.SetupStateDuration(currentDuration);
        }

        void OnComplete()
        {
            GameManager.Instance.GameController.EndRound();
        }
    }

    private void UpdateScore()
    {
        var character = GameManager.Instance.CharacterManager.GetCharacterModel(PhotonNetwork.LocalPlayer.UserId);

        if (!character.isSeek && !character.IsDead)
        {
            character.Score += 1;
            RpcExcute.instance.Rpc_SendUpdateScore(character.Score);
        }
    }
}
