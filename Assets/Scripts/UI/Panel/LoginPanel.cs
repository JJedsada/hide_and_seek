using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : Panel
{
    [SerializeField] private TMP_InputField usernameInput;
    [Space]
    [SerializeField] private TMP_Text buttonText;
    [Space]
    [SerializeField] private Button loginButton;

    public Action<string> onLogin;

    public override void Initialize()
    {
        loginButton.onClick.RemoveAllListeners();
        loginButton.onClick.AddListener(OnLogin);
    }

    private void OnLogin()
    {
        onLogin?.Invoke(usernameInput.text);
    }

    public void LoggingIn()
    {
        buttonText.text = "Loading...";
    }
}
