using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillButtonGUI : Panel
{
   [SerializeField] private TMP_Text nameText;
   [SerializeField] private Button button;

    public void AddListener(Action onSkill)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => { onSkill?.Invoke(); });
    }

    public void SetupName(string name)
    {
        nameText.text = name;
    }
}
