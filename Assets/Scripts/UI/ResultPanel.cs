using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultPanel : Panel
{
    [SerializeField] private RectTransform content;

    public override void Open()
    {
        base.Open();
        ShowSummary();
    }

    private void ShowSummary()
    {
        var playerInGame = UIManager.Instance.GameplayPanel.playersInLobby.Values;

        foreach (var player in playerInGame)
        {
            player.transform.SetParent(content);
        }
    }

    public void Clear()
    {
        var playerInGame = UIManager.Instance.GameplayPanel.playersInLobby.Values;
        var content = UIManager.Instance.GameplayPanel.content;

        foreach (var player in playerInGame)
        {
            player.transform.SetParent(content);
            player.transform.position = Vector3.zero;
        }

        Close();
    }
}
