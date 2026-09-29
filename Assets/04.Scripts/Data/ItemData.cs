using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using static ItemData;
/*Item Data Fromat*/



public enum eITEMTYPE
{
    NONE,
    CONSUMABLE,
    EQUIPMENT,
    INGREDIENT,
    MONEY,
    QUEST,
    SKILL,
    TYPEEND
}
public enum  eEQUIPMENTTYPE
{
    NONE,
    WEAPON,
    GLOVES,
    ARMOR,
    HEAD,
    SHOES,
    GEM,
    EQUIPMENTTYPEEND
}

public class ItemData
{
   


    public int ItemID;
    [JsonConverter(typeof(StringEnumConverter))]
    public eITEMTYPE Type; //Consumable ? Equipment? ingredient?
    public string ItemName;
    public string SpriteName;
    public string Explanation;
    public int price;
   

    /*ingredient 는 이거 없을거임*/
    /*PrimaryStats*/
    public int ExtraAttack;
    public int ExtraDefense;
    public int ExtraCriChance;
    public int ExtraCriDemage;
    public int ExtraHP;
    public int ExtraMP;
    public eEQUIPMENTTYPE EquipmentType;

    public int Level;
    public int MaxReinforce;
    public eItemRarity Rarity;


    //UseItem -> Item마다 함수 포인터를 따로 지정 (ststic으로 함수포인터 따로 관리 , 구현)

 
}
