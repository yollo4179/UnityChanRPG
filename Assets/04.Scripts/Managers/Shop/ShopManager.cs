using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO;
public class ShopManager 
{
 
    /*상점 인포*/
    public ShopInfo[] _shopInfo = new ShopInfo[(int)eTRADERS.SHOP_END];
    public ShopInfo[] ShopInfo { get => _shopInfo; set => _shopInfo =value; } //나중에 배열로 만들어서 npc마다 구분;
    /*Debug*/
    private string[] m_ShopInfoPath = new string[(int)eTRADERS.SHOP_END];
    public string GetShopInfoPath(eTRADERS trader)
    {
        return m_ShopInfoPath[(int)trader];
    }   
    public void Init()
    {
        m_ShopInfoPath[(int)eTRADERS.SHOP1] = Application.persistentDataPath + "/ShopInfo.json"; //Partial로 나누든지 마을마다...
        
        for (int i = 0; i<_shopInfo.Length;++i)
        {
            _shopInfo[i] = new ShopInfo();
            _shopInfo[i].Init();
            LoadInfo(i);
        }

        //Todo혹시 다른 상점? -마을마다 다르게 해싱 
        
    }
 
    public void SaveInfo<T>(string _InfoPath, T Object)
    {

        try
        {
            string json = JsonConvert.SerializeObject(Object);
            File.WriteAllText(_InfoPath, json);
            Debug.Log($"<color=#00ff00>저장 완료 : {_InfoPath} 내용{json}</color>");

        }
        catch (Exception e)
        {
            Debug.Log($"<color=#ff0000>{e} </color>");
        }
        finally { }
    }

    public ShopInfo LoadInfo(int shopIndex)//To do마을 키, npc 키 동시에 보내서 구분하기
    {
        try
        {
            var json = File.ReadAllText(m_ShopInfoPath[shopIndex]);
            Debug.Log($"<color=#00ff00>{json}</color>");
            _shopInfo[shopIndex] = JsonConvert.DeserializeObject<ShopInfo>(json);
            return _shopInfo[shopIndex];

        }
        catch (Exception e)
        {
            Debug.Log($"<color=ff0000>{e}</color>");
        }
        finally
        {

        };
        return default(ShopInfo);
    }
}
