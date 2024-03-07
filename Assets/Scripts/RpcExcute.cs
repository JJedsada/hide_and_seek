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

    public void RPC_SendPlayerInfo(int phottonViewId)
    {
        base.photonView.RPC("RPC_ReceivePlayerInfo", RpcTarget.OthersBuffered, PhotonNetwork.LocalPlayer, phottonViewId);
    }

    public void RPC_SendUpdateReadyState(bool isReady)
    {
        base.photonView.RPC("RPC_ReceiveUpdateReadyState", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer, isReady);
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

    }

    #region Receive
    [PunRPC]
    private void RPC_ReceivePlayerInfo(Player player, int viewId)
    {
        Debug.Log($"Add {player.UserId}");
        GameManager.Instance.CharacterManager.viewIds.Add(player.UserId, viewId);
    }

    [PunRPC]
    private void RPC_ReceiveUpdateReadyState(Player player, bool isReady)
    {
        GameManager.Instance.GameController.UpdateReadyState(player.UserId, isReady);       
    }

    [PunRPC]
    private void Rpc_ReceiveGameStarting(string seekList)
    {
        GameManager.Instance.GameController.SetupSeek(seekList);
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
    #endregion
}
