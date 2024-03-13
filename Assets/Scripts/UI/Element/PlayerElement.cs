using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerElement : MonoBehaviour
{
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject deadIcon;
    [SerializeField] private Image readyImage; 
    [SerializeField] private Image role;
    [SerializeField] private GameObject owner;

    public Player playerInfo { get; private set; }

    public bool IsReady { get; private set; } = false;
    public bool IsDead { get; private set; } = false;

    public void Initialize(Player info)
    {
        playerInfo = info;

        EnterLobby();
        UpdateReadyState(IsReady);
    }

    public void EnterLobby()
    {
        SetupDisplay();
        scoreText.text = "0";
        role.gameObject.SetActive(false);
    }

    public void EnterGameplay(bool isSeek)
    {
        role.gameObject.SetActive(true);

        if (isSeek)
            role.color = Color.red;
        else
            role.color = Color.green;
    }

    private void SetupDisplay()
    {
        nicknameText.text = playerInfo.NickName + "";
        owner.SetActive(false);
        if (playerInfo.UserId != PhotonNetwork.LocalPlayer.UserId)
            return;

        owner.SetActive(true);
        nicknameText.text += "(You)";
    }

    public void UpdateReadyState(bool isReady)
    {
        IsReady = isReady;

        Color color = Color.red;

        if (IsReady)
            color = Color.green;

        readyImage.color = color;
    }

    public void SetupScore(int score)
    {
        scoreText.text = $"{score}";
    }

    public void SetupDead(bool isDead)
    {
        deadIcon.SetActive(isDead);
    }
}
