using Photon.Pun;
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
    public StateController GameStateController = new();

    private void Start()
    {
        GameStateController.Initialize();
        UIManager.Instance.Initialized();

        LoginController.Initialize();
        BrowseController.Initialize();
        GameController.Initialize();

        LoginController.Present();
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        GameStateController.Update(deltaTime);
    }

    public void ChageState(StateType state)
    {
        switch (state)
        {
            case StateType.Lobby:
                GameStateController.ChangeState(new LobbyState());
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
        RpcExcute.instance.RPC_SendPlayerInfo(viewId);
        if (PhotonNetwork.IsMasterClient)
        {
            RpcExcute.instance.RPC_SendUpdateReadyState(true);
        }
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

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
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
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
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
}

public class HuntingState : State
{
    private CountDown stateCountDown = new CountDown();

    public override void EnterState()
    {
        base.EnterState();
        ShowHuntingDisplay();
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        stateCountDown.Update(deltaTime);
    }

    public void ShowHuntingDisplay()
    {
        GameManager.Instance.GameController.SetupHuntingDisplay();

        stateCountDown.Start(GameConfig.HuntingDuration);

        stateCountDown.onUpdate = OnUpdate;
        stateCountDown.onComplete = OnComplete;

        void OnUpdate(float currentDuration)
        {
            UIManager.Instance.GameplayPanel.SetupStateDuration(currentDuration);
        }

        void OnComplete()
        {
            if (PhotonNetwork.IsMasterClient)
                RpcExcute.instance.Rpc_SendResult();
        }
    }
}

public class ResultState : State
{

}
