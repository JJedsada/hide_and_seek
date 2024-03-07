using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerManager : MonoBehaviourPunCallbacks
{
    public Action OnConnectedServer;

    public void Connect(string username)
    {
        PhotonNetwork.NickName = username;

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        OnConnectedServer?.Invoke();

        Debug.Log("Connected");
    }
}
