using Newtonsoft.Json;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class MonsterManager
{
    //private static MonsterManager m_Instance =null;

    private  Dictionary<int, MonsterInfo> m_MonsterData;
    //public static MonsterManager GetInstance()
    //{
    //    if (null==m_Instance)
    //    {
    //        m_Instance = new MonsterManager();
    //        m_Instance.LoadMonsterData();
    //    }
    //    if(null == m_Instance.m_MonsterData)
    //    {
    //        m_Instance.LoadMonsterData();
    //    }
    //    return m_Instance;  
    //}
    public void Init()
    {
        LoadMonsterData();
    }
    public MonsterInfo GetMonsterInfo(int ID)
    {
        MonsterInfo MonInfo =null;
        if (true == m_MonsterData.ContainsKey(ID)) MonInfo =m_MonsterData[ID];
        return MonInfo;
    }

    public void LoadMonsterData()
    {
        var Data =Resources.Load("Data/Json/MonstersData") ;
        Debug.Log($"<color=#00ff00>MonsterInfoManager LoadMonsterData Path : {Data} </color>");

        MonsterInfo[] arrMonsterInfo;
        arrMonsterInfo = JsonConvert.DeserializeObject<MonsterInfo[]>(Data.ToString());
        this.m_MonsterData = JsonConvert.DeserializeObject<MonsterInfo[]>(Data.ToString()).ToDictionary(x => x.MonsterID);       
    }

}
