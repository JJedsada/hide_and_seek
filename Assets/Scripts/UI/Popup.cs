using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Popup : Panel
{
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button confirmButtom;

    public void SetupData(string message)
    {
        descriptionText.text = message;

        confirmButtom.onClick.RemoveAllListeners();
        confirmButtom.onClick.AddListener(Close);

        Open();
    }
}
