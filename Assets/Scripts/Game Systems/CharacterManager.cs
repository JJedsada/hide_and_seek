using Photon.Pun;
using Photon.Pun.UtilityScripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterManager 
{
    [SerializeField] private CharacterController PlayerPrefab;
    [Space]
    [SerializeField] private Transform spawnPoint;

    public CharacterController MainCharater;

    public Dictionary<int, CharacterController> character = new Dictionary<int, CharacterController>();
    public Dictionary<string, int> viewIds = new Dictionary<string, int>();

    public void SpawnPlayer()
    {
        PlayerPrefab.photonView.ViewID = PhotonNetwork.LocalPlayer.GetPlayerNumber();
        PhotonNetwork.RegisterPhotonView(PlayerPrefab.photonView);
        var localPlayer = PhotonNetwork.Instantiate(PlayerPrefab.name, spawnPoint.position, Quaternion.identity);
        MainCharater = localPlayer.GetComponent<CharacterController>();
        MainCharater.Setup(PhotonNetwork.LocalPlayer);
    }
}
