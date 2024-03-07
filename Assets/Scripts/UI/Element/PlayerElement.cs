using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerElement : MonoBehaviour
{
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private Image readyImage;

    public Player playerInfo { get; private set; }

    public bool IsReady { get; private set; } = false;
    public bool IsDead { get; private set; } = false;

    public void Initialize(Player info)
    {
        playerInfo = info;

        SetupDisplay();
        UpdateReadyState(IsReady);
    }

    private void SetupDisplay()
    {
        nicknameText.text = playerInfo.NickName;
    }

    public void UpdateReadyState(bool isReady)
    {
        IsReady = isReady;

        Color color = Color.red;

        if (IsReady)
            color = Color.green;

        readyImage.color = color;
    }
}
