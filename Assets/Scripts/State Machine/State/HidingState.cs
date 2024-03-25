using Photon.Pun;
using UnityEngine;

public class HidingState : State
{
    private CountDown stateCountDown = new CountDown();

    public override void EnterState()
    {
        base.EnterState();
        ShowHidingDisplay();
        SetupMainCharacterState();
    }
    public override void ExitState()
    {
        var mainCharacter = GameManager.Instance.CharacterManager.MainCharater;
        mainCharacter.ClearPlayerAction();

        GameManager.Instance.GameController.ExitHidingState();
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

        float duration = GameConfig.HidingDuration;

        if (GameManager.Instance.GameController.IsTimeless)
        {
            duration = GameConfig.TimelessDuration;
            GameManager.Instance.GameController.IsTimeless = false;
        }

        stateCountDown.Start(duration);

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
            mainCharacter.SetMoveAble(true);
            return;
        }
        mainCharacter.SetHideModel(true);
        mainCharacter.SetMoveAble(true);
        mainCharacter.OutHide();
    }
}