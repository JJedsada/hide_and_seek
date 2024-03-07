using UnityEngine;

public class LoginController 
{
    private LoginPanel loginPanel => UIManager.Instance.LoginPanel;

    public void Initialize()
    {
        loginPanel.onLogin = OnLogin;
    }

    public void Present()
    {
        loginPanel.Open();
    }

    private void OnLogin(string usernameText)
    {
        string username = usernameText;

        if (string.IsNullOrEmpty(username))
        {
            Debug.LogError("username cant be empty.");
            return;
        }
        loginPanel.LoggingIn();
        GameManager.Instance.Server.OnConnectedServer = OnConnectedServer;
        GameManager.Instance.Server.Connect(username);   
    }

    private void OnConnectedServer()
    {
        loginPanel.Close();
        GameManager.Instance.BrowseController.Present();
    }
}
