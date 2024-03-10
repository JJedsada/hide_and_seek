using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
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
        base.photonView.RPC("Rpc_ReceiveHuntingState", RpcTarget.All);
    }

    public void Rpc_SendHideInJar(bool isHide, int hideId)
    {
        base.photonView.RPC("Rpc_ReceiveHideState", RpcTarget.All, PhotonNetwork.LocalPlayer, isHide, hideId);
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
        GameManager.Instance.ChageState(StateType.Prepare);
    }

    [PunRPC]
    private void Rpc_ReceiveHidingState()
    {
        GameManager.Instance.ChageState(StateType.Hiding);
    }

    [PunRPC]
    private void Rpc_ReceiveHuntingState()
    {
        GameManager.Instance.ChageState(StateType.Hunting);
    }

    [PunRPC]
    private void Rpc_ReceiveResultState()
    {
        GameManager.Instance.ChageState(StateType.Result);
    }

    [PunRPC]
    private void Rpc_ReceiveHideState(Player player, bool isHide, int hideId)
    {
        GameManager.Instance.GameController.SetPlayerHiding(player.UserId, isHide, hideId);
    }
    #endregion
}
