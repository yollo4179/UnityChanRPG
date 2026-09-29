using UnityEngine;

using System.IO;
using Newtonsoft.Json;
public class PlayerInfo
{
    public int Level { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int CriChance { get; set; }
    public int CriDemage { get; set; }   // "CriDamage" 오타 가능성 — 확실하지 않음
    public int CurHP { get; set; }
    public int MaxHP { get; set; }
    public int CurMP { get; set; }
    public int MaxMP { get; set; }
    public int SkillPoint { get; set; }  // 세미콜론 누락되어 있어 보였음
    public int ExtraAbilityPoints { get; set; }  // "ExtraAbility" 오타 가능성 — 확실하지 않음
    public int CurExp { get; set; }
    public int TotalExp { get; set; }
    public int Money { get; set; }









    void Copy(PlayerInfo oth)
    {
        this.Level                   =oth.Level;
        this.Attack                  =oth.Attack;
        this.Defense                 =oth.Defense;
        this.CriChance               =oth.CriChance;
        this.CriDemage               =oth.CriDemage;
        this.CurHP                   =oth.CurHP;
        this.MaxHP                   =oth.MaxHP;
        this.CurMP                   =oth.CurMP;
        this.MaxMP                   =oth.MaxMP;
        this.SkillPoint              =oth.SkillPoint;
        this.ExtraAbilityPoints      =oth.ExtraAbilityPoints;
        this.CurExp                  =oth.CurExp;
        this.TotalExp                =oth.TotalExp;
        this.Money                   =oth.Money;
    }
    /*몬스터를 죽이면 그때 ,*/
    /*플레이어 매니저에서 관리*/


    public void LoadJson()
    {
        string path = Path.Combine(Application.persistentDataPath, "UserInfo.json");

        if (!File.Exists(path))
        {
            Debug.Log($"PlayerInfo save file not found. Creating a new save at {path}");
            SavePlayerInfo();
            return;
        }

        try
        {
            string json = File.ReadAllText(path);
            PlayerInfo playerInfo = JsonConvert.DeserializeObject<PlayerInfo>(json);

            if (playerInfo == null)
            {
                Debug.LogError($"PlayerInfo save file is empty or invalid: {path}");
                return;
            }

            Copy(playerInfo);
        }
        catch (JsonException exception)
        {
            Debug.LogError($"Failed to parse PlayerInfo save file at {path}: {exception.Message}");
        }

    }

    public void SavePlayerInfo()
    {
        string path = Path.Combine(Application.persistentDataPath, "UserInfo.json");
        string json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(path, json);
        Debug.Log("PlayerInfo saved to " + path);

    }
    
}
