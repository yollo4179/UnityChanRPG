using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using UnityEngine;
using static ItemData;
using System;
using Unity.VisualScripting;
using Newtonsoft.Json.Linq;

public enum eItemRarity
{
    COMMON,
    RARE,
    UNIQUE,
}
public sealed class ItemInfoConverter : JsonConverter<ItemInfo>
{
    public override bool CanWrite =>false; 
    public override ItemInfo ReadJson(JsonReader reader, System.Type objectType, ItemInfo existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jo = JObject.Load(reader);
        var kind = (jo["Type"])?.ToString(); // 대소문자 유연
        ItemInfo target = kind switch
        {
            "EQUIPMENT" => new InstanceItemInfo(),
            "INGREDIENT" => new ItemInfo(),
            "CONSUMABLE" => new ItemInfo(),
            _ => throw new JsonSerializationException($"알 수 없는 kind: {kind}")
        };
        serializer.Populate(jo.CreateReader(), target);
        return target;
    }

    public override void WriteJson(JsonWriter writer, ItemInfo value, JsonSerializer serializer)
    {
        JObject.FromObject(value, serializer).WriteTo(writer);
    }
}
[JsonConverter(typeof(ItemInfoConverter))] public class ItemInfo
{

    public int ID;
    public int Amount;
    [JsonConverter(typeof(StringEnumConverter))]
    public eITEMTYPE Type; //Consumable ? Equipment? ingredient?
    public void SetInfo(int id, eITEMTYPE type, int amount)
    {
        ID =id;
        Amount =amount;
        Type= type; 
    }
    public void UseItem()
    {
        ItemEffectsLibrary.UseItem(this);
    }

}
public class InstanceItemInfo : ItemInfo
{

    public void SetInstanceInfo (string instanceID, int level, eItemRarity rarity,int maxReinforce,bool isEquipped =false)
    {
        InstanceID = instanceID;
        Level = level;
        Rarity = rarity;
        MaxReinforce = maxReinforce;
        IsEquipped=isEquipped;
    }

    public string InstanceID;
    public int Level;
    [JsonConverter(typeof(StringEnumConverter))]
    public eItemRarity Rarity;
    public bool IsEquipped;
    public int MaxReinforce;
    public int CurrentReinforce;
    public int ExtraCriChance;
    public int ExtraCriDamage;
    public int ExtraDefense;
    public int ExtraDamage;
    public int ExtraHealth;
    public int ExtraMana;
}