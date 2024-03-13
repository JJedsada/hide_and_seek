using Newtonsoft.Json;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class HuntingState : State
{
    private GameController gameController => GameManager.Instance.GameController;

    private CountDown stateCountDown = new CountDown();

    public override void EnterState()
    {
        base.EnterState();
        StartCountdownState();
        SetupHuntingDisplay();
        SetupMainCharacterState();
        SetupHiderDeadByWithOutHide();
    }

    public override void Update()
    {
        base.Update();

        float deltaTime = Time.deltaTime;
        stateCountDown.Update(deltaTime);
    }

    private void StartCountdownState()
    {
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

    public void SetupHuntingDisplay()
    {
        GameManager.Instance.GameController.SetupHuntingState();
    }

    private void SetupMainCharacterState()
    {
        var mainCharacter = GameManager.Instance.CharacterManager.MainCharater;
        int breakCount = GameConfig.BreakJarCount(gameController.currentRound, gameController.playerCount);
        mainCharacter.SetupHuntingState(breakCount);

        mainCharacter.SetMoveAble(false);

        if (gameController.IsSeek)
            mainCharacter.SetMoveAble(true);
    }

    private void SetupHiderDeadByWithOutHide()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        List<PlayerInJar> playerNotHiding = new List<PlayerInJar>();
        foreach (var character in GameManager.Instance.CharacterManager.characterModels.Values)
        {
            if (character.isHiding)
                continue;

            if(character.isSeek)
                continue;

            PlayerInJar player = new PlayerInJar();
            player.userId = character.playerInfo.UserId;
            playerNotHiding.Add(player);
        }

        string json = JsonConvert.SerializeObject(playerNotHiding.ToArray());
        RpcExcute.instance.Rpc_SendBreakDamage(json);
    }
}