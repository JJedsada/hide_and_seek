using Photon.Pun;
using Photon.Pun.UtilityScripts;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterManager 
{
    [SerializeField] private CharacterController PlayerPrefab;
    [Space]
    [SerializeField] private Transform spawnPoint;

    public CharacterController MainCharater;

    public Dictionary<int, CharacterController> characterModels {get; private set;} = new Dictionary<int, CharacterController>();

    public void SpawnPlayer()
    {
        PlayerPrefab.photonView.ViewID = PhotonNetwork.LocalPlayer.GetPlayerNumber();
        PhotonNetwork.RegisterPhotonView(PlayerPrefab.photonView);
        var localPlayer = PhotonNetwork.Instantiate(PlayerPrefab.name, spawnPoint.position, Quaternion.identity);
        MainCharater = localPlayer.GetComponent<CharacterController>();
        MainCharater.SetupPlayerData(PhotonNetwork.LocalPlayer);
    }

    public void AddCharacter(int viewId, CharacterController characterObject)
    {
        this.characterModels.Add(viewId, characterObject);

        if(characterObject.playerInfo == default || characterObject.playerInfo == null)
        {
            string userId = GameManager.Instance.Lobby.playerDataViewIds[viewId];
            var playerData = GameManager.Instance.Lobby.playerDataInMatch[userId];
            characterObject.SetupPlayerData(playerData);
        }  
    }

    public CharacterController GetCharacterModel(string userId)
    {
        foreach(var model in characterModels.Values)
        {
            if(model.playerInfo.UserId == userId)
                return model;
        }

        return null;
    }

    public void ClearCharacterModel()
    {
        characterModels.Clear();
    }
}
