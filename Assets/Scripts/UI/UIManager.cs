using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public LoginPanel LoginPanel;
    public BrowsePanel BrowsePanel;
    public GameplayPanel GameplayPanel;
    [SerializeField] private Popup popup;
    [SerializeField] private GameObject loadingTaskPanel;

    public void Initialized()
    {
        LoginPanel.Initialize();
        BrowsePanel.Initialize();
        GameplayPanel.Initialize();
    }

    public static void OpenLoading()
    {
        Instance.loadingTaskPanel.SetActive(true);
    }

    public static void CloseLoading()
    {
        Instance.loadingTaskPanel.SetActive(false);
    }

    public static void ShowPopup(string message)
    {
        Instance.popup.SetupData(message);
    }
}
