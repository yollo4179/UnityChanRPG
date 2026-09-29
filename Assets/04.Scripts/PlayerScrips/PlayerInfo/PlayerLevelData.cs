using UnityEngine;
using System.IO;
using Newtonsoft.Json;
public class PlayerLevelData
{

     public int Level{ get; set; }
     public int Attack { get ; set; }
     public int Defense { get ; set; }
    public int CriChance { get; set ; }
    public int CriDemage { get ; set; }
    public int MaxHP { get ; set ; }
    public int MaxMP { get; set; }
    public int SkillPoint { get ; set ; }
    public int ExtraAbilityPoints { get; set; }
    public int TotalExp { get; set; }

    public static  PlayerLevelData[] GetPlayerLevelDataFromJson()
    {
        string overridePath = Path.Combine(Application.persistentDataPath, "UserLevelData.json");
        string json;

        if (File.Exists(overridePath))
        {
            json = File.ReadAllText(overridePath);
            Debug.Log($"<color=#00ff00>PlayerLevelData override loaded: {overridePath}</color>");
        }
        else
        {
            TextAsset defaultData = Resources.Load<TextAsset>("Data/Json/PlayerLevelData");
            if (defaultData == null)
            {
                throw new FileNotFoundException(
                    "Default player level data is missing from Resources/Data/Json/PlayerLevelData.json.");
            }

            json = defaultData.text;
            Debug.Log("<color=#00ff00>Default PlayerLevelData loaded from Resources.</color>");
        }

        PlayerLevelData[] levelData = JsonConvert.DeserializeObject<PlayerLevelData[]>(json);
        if (levelData == null || levelData.Length == 0)
        {
            throw new InvalidDataException("PlayerLevelData JSON does not contain any level data.");
        }

        return levelData;
    }
}
