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

    public void Initialized()
    {
        LoginPanel.Initialize();
        BrowsePanel.Initialize();
        GameplayPanel.Initialize();
    }
}
