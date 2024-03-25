using System;
using System.Collections.Generic;
using UnityEngine;

public class GameplaySkillElement : Panel
{
    private SkillType[] hiderSkills = new SkillType[4] { SkillType.Block, SkillType.Kick, SkillType.Revice, SkillType.Spray };
    private SkillType[] finderSkills = new SkillType[3] { SkillType.Scan, SkillType.Starfall, SkillType.Timeless };

    [SerializeField] private List<SkillButtonGUI> buttonSkills;

    public void SetupSkill()
    {
        SkillType[] skill = hiderSkills;
        int skillCount = 3;
        if (GameManager.Instance.GameController.IsSeek)
        {
            skill = finderSkills;
            skillCount = 2;
        }

        for (int i = 0; i < buttonSkills.Count; i++)
        {
            int index = i;
            var button = buttonSkills[i];
            button.Close();
            if (i <= skillCount)
            {
                button.SetupName(skill[index].ToString());
                button.Open();
            }           
        }
    }

    public void OutOfSkill(int index)
    {
        buttonSkills[index].Close();
    }

    public void AddListener(Action<int> onSkill)
    {
        for (int i = 0; i < buttonSkills.Count; i++)
        {
            var button = buttonSkills[i];
            int index = i;
            button.AddListener(()=> { onSkill?.Invoke(index); });
        }
    }
}
