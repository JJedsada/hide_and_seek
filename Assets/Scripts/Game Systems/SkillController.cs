using Newtonsoft.Json;
using Photon.Pun;
using System.Collections.Generic;

public class SkillController 
{
    private SkillType[] hiderSkills = new SkillType[4] { SkillType.Block, SkillType.Kick, SkillType.Revice, SkillType.Spray };
    private SkillType[] finderSkills = new SkillType[3] { SkillType.Scan, SkillType.Starfall, SkillType.Timeless };

    private Dictionary<SkillType, int> skillData = new Dictionary<SkillType, int>();

    private bool isFinder;
    private bool isUsedStarfall;

    private CharacterController character;

    public void Initailize(CharacterController character)
    {
        this.character = character;
    }

    public void SetupData(bool isFinder)
    {
        this.isFinder = isFinder;

        skillData.Clear();

        SkillType[] skill = hiderSkills;
        if (isFinder)
            skill = finderSkills;

        for (int i = 0; i < skill.Length; i++)
            skillData.Add(skill[i], 1);
    }

    public void ActionSkill(int index)
    {
        SkillType skillId = hiderSkills[index];
        if (isFinder)
            skillId = finderSkills[index];

        SkillAction(skillId, index);
    }

    private void SkillAction(SkillType skillId, int index)
    {
        if (!skillData.ContainsKey(skillId) || skillData[skillId] <= 0)
            return;

        skillData[skillId] -= 1;

        if (skillData[skillId] <= 0)
            UIManager.Instance.GameplayPanel.skillElement.OutOfSkill(index);

        SkillReqeust reqeust = new SkillReqeust();
        reqeust.skillId = skillId;
        reqeust.playerId = PhotonNetwork.LocalPlayer.UserId;
        switch (skillId)
        {
            case SkillType.Spray:
                var charaterSpray = character.TrySpray();
                if (!charaterSpray.isInteract)
                    return;
                reqeust.jarId = charaterSpray.jarId;
                break;

            case SkillType.Starfall:
                isUsedStarfall= true;               
                return;
        }
        string json = JsonConvert.SerializeObject(reqeust);
        RpcExcute.instance.Rpc_SendActiveSkill(json);
    }

    public void StarfallActive()
    {
        if (!isUsedStarfall)
            return;

        isUsedStarfall = false;

        SkillReqeust reqeust = new SkillReqeust();
        reqeust.skillId = SkillType.Starfall;
        reqeust.playerId = PhotonNetwork.LocalPlayer.UserId;
        reqeust.jarId = GetJarIdByStarfall();
        string json = JsonConvert.SerializeObject(reqeust);
        RpcExcute.instance.Rpc_SendActiveSkill(json);
    }

    private int GetJarIdByStarfall()
    {
        var jarAlive = GameManager.Instance.JarManager.JarAlive();

        int index = UnityEngine.Random.Range(0, jarAlive.Count);

        return jarAlive[index].JarId;
    }
}

public class SkillReqeust
{
    public SkillType skillId;
    public string playerId;
    public int jarId;
}

public enum SkillType
{
    Block,
    Kick,
    Revice,
    Spray,
    Scan,
    Starfall,
    Timeless
}
