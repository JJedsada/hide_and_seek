using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BrowsePanel : Panel
{
    [SerializeField] private TMP_InputField roomnameInput;
    [SerializeField] private Button createButton;
    [Space]
    [SerializeField] private RoomElement roomElementPrefab;
    [SerializeField] private RectTransform listingContent;

    private List<RoomElement> onlineRooms = new List<RoomElement>();

    public Action<string> onCreateRoom;
    public Action<string> onJoinRoom;

    public override void Initialize()
    {
        createButton.onClick.RemoveAllListeners();
        createButton.onClick.AddListener(OnCreateRoom);
    }

    public override void Open()
    {
        base.Open();
        roomnameInput.text = string.Empty;
    }

    private void OnCreateRoom()
    {
        onCreateRoom?.Invoke(roomnameInput.text);
    }

    public void SetupRoomDisplay(List<RoomInfo> roomInfos)
    {
        foreach (var onlined in onlineRooms)
        {
            Destroy(onlined.gameObject);
        }
        onlineRooms.Clear();

        for (int i = 0; i < roomInfos.Count; i++)
        {
            var roomInfo = roomInfos[i];

            if (roomInfo.RemovedFromList)
                continue;

            var room = Instantiate(roomElementPrefab, listingContent);
            room.onJoin = onJoinRoom;
            room.Setup(roomInfos[i]);
            onlineRooms.Add(room);
        }
    }
}
