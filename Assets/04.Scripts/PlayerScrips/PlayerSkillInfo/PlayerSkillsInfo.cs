using UnityEngine;

using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
public class PlayerSkillsInfo 
{
    public PlayerSkillInfo[] skillsInfoList;

    public void LoadJson()
    {
        string path = Path.Combine(Application.persistentDataPath, "UserSkillInfo.json");

        if (!File.Exists(path))
        {
            CreateDefaultSkillInfo();
            SavePlayerInfo();
            return;
        }

        string json = File.ReadAllText(path);
        PlayerSkillsInfo playerInfo = JsonConvert.DeserializeObject<PlayerSkillsInfo>(json);

        if (playerInfo == null || playerInfo.skillsInfoList == null)
        {
            Debug.LogError($"UserSkillInfo is empty or invalid: {path}");
            CreateDefaultSkillInfo();
            return;
        }

        Copy(playerInfo);

    }
    void Copy(PlayerSkillsInfo oth)
    {
        this.skillsInfoList =oth.skillsInfoList;

    }

    public void SavePlayerInfo()
    {
        string path = Path.Combine(Application.persistentDataPath, "UserSkillInfo.json");
        string json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(path, json);
        Debug.Log("UserSkillInfo saved to " + path);

    }

    void CreateDefaultSkillInfo()
    {
        SkillSO[] skillAssets = Resources.LoadAll<SkillSO>("Data/ScriptableObjects/SkillData");
        List<PlayerSkillInfo> defaultSkills = new List<PlayerSkillInfo>();

        foreach (SkillSO skill in skillAssets)
        {
            if (skill.handle < 6999 || skill.handle > 7006)
                continue;

            int numberOfHits = 0;
            if (skill.EventSequence.eventClips != null)
            {
                foreach (EventClipInfo clip in skill.EventSequence.eventClips)
                {
                    if (clip.events == null)
                        continue;

                    foreach (AnimEventDesc animationEvent in clip.events)
                    {
                        if (animationEvent.eventName == eAnimEvent.COLLIDER)
                            numberOfHits++;
                    }
                }
            }

            defaultSkills.Add(new PlayerSkillInfo
            {
                SkillID = skill.handle,
                SkillName = skill.SkillName,
                Level = Mathf.Max(1, skill.SkillLevel),
                BaseDamage = skill.Damage,
                ExtraDamage = skill.DamageIncreasePercentPoint,
                NumberOfHit = Mathf.Max(1, numberOfHits)
            });
        }

        defaultSkills.Sort((left, right) => left.SkillID.CompareTo(right.SkillID));
        skillsInfoList = defaultSkills.ToArray();
        Debug.Log($"Created default skill save data for {skillsInfoList.Length} player skills.");
    }
}
