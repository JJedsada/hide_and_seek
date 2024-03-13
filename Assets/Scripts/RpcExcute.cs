using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public class RpcExcute : MonoBehaviourPunCallbacks
{
    public static RpcExcute instance;

    private void Awake()
    {
        instance = this;
    }

    public void Rpc_SendPlayerInfo(int phottonViewId)
    {
        base.photonView.RPC("Rpc_ReceivePlayerInfo", RpcTarget.OthersBuffered, PhotonNetwork.LocalPlayer, phottonViewId);
    }

    public void Rpc_SendUpdateReadyState(bool isReady)
    {
        base.photonView.RPC("Rpc_ReceiveUpdateReadyState", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer, isReady);
    }

    public void Rpc_SendWaittingState()
    {
        base.photonView.RPC("Rpc_ReceiveWaittingState", RpcTarget.All);
    }

    public void Rpc_SendGameStarting(Seek[] seekList)
    {
        string json = JsonConvert.SerializeObject(seekList);
        base.photonView.RPC("Rpc_ReceiveGameStarting", RpcTarget.All, json);
    }

    public void Rpc_SendHidingState()
    {
        base.photonView.RPC("Rpc_ReceiveHidingState", RpcTarget.All);
    }

    public void Rpc_SendHuntingState()
    {
        base.photonView.RPC("Rpc_ReceiveHuntingState", RpcTarget.All);
    }

    public void Rpc_SendResult()
    {
        base.photonView.RPC("Rpc_ReceiveResultState", RpcTarget.All);
    }

    public void Rpc_SendHideInJar(bool isHide, int jarId)
    {
        base.photonView.RPC("Rpc_ReceiveHideInJar", RpcTarget.All, PhotonNetwork.LocalPlayer, isHide, jarId);
    }

    public void Rpc_SendBreakJar(int jarId)
    {
        base.photonView.RPC("Rpc_ReceiveBreakJar", RpcTarget.All, PhotonNetwork.LocalPlayer, jarId);
    }

    public void Rpc_SendBreakDamage(string playerData)
    {
        base.photonView.RPC("Rpc_ReceiveBreakDamage", RpcTarget.All, playerData);
    }

    public void Rpc_SendUpdateScore(int score)
    {
        base.photonView.RPC("Rpc_ReceiveUpdateScore", RpcTarget.All, PhotonNetwork.LocalPlayer, score);
    }

    #region Receive
    [PunRPC]
    private void Rpc_ReceivePlayerInfo(Player player, int viewId)
    {
        GameManager.Instance.Lobby.ReceiveViewId(player.UserId, viewId);
    }

    [PunRPC]
    private void Rpc_ReceiveUpdateReadyState(Player player, bool isReady)
    {
        GameManager.Instance.GameController.UpdateReadyState(player.UserId, isReady);       
    }

    [PunRPC]
    private void Rpc_ReceiveGameStarting(string seekList)
    {
        GameManager.Instance.GameController.SetupGameData(seekList);
        GameManager.Instance.ChangeState(StateType.Prepare);
    }

    [PunRPC]
    private void Rpc_ReceiveHidingState()
    {
        GameManager.Instance.ChangeState(StateType.Hiding);
    }

    [PunRPC]
    private void Rpc_ReceiveHuntingState()
    {
        GameManager.Instance.ChangeState(StateType.Hunting);
    }

    [PunRPC]
    private void Rpc_ReceiveResultState()
    {
        GameManager.Instance.ChangeState(StateType.Result);
    }

    [PunRPC]
    private void Rpc_ReceiveHideInJar(Player player, bool isHide, int jarId)
    {
        GameManager.Instance.GameController.SetPlayerHiding(player.UserId, isHide, jarId);
    }

    [PunRPC]
    private void Rpc_ReceiveBreakJar(Player player, int jarId)
    {
        GameManager.Instance.GameController.BreakingJar(player.UserId, jarId).Forget();
    }

    [PunRPC]
    private void Rpc_ReceiveBreakDamage(string playerDataTaking)
    {   
        GameManager.Instance.GameController.TakeDamage(playerDataTaking).Forget();
    }

    [PunRPC]
    private void Rpc_ReceiveWaittingState()
    {
        GameManager.Instance.ChangeState(StateType.Waitting); 
    }

    [PunRPC]
    private void Rpc_ReceiveUpdateScore(Player player, int score)
    {
        GameManager.Instance.GameController.UpdateScore(player.UserId, score);
    }

    #endregion
}
