using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerManager
{
    


    private PlayerInfo m_PlayerInfo;
    public PlayerInfo PlayerInfo { get=>m_PlayerInfo;}

    private PlayerSkillsInfo _playerSkillsInfo;
    private Dictionary<int, PlayerSkillInfo> _dicPlayerSkillInfo;
    private Dictionary<int, int> _dicPlayerSkillIDAndIdxPairs;

    private Dictionary<int, PlayerLevelData> m_DicPlayerLevelData;

    public int LevelPrev { set; get; }
    UI_PlayerStatusBar _statusBar;
    StatusScript _statusScript;
    public void Init()
    {
        

        m_PlayerInfo =new PlayerInfo();
        m_PlayerInfo.LoadJson();
        Debug.Log($"<color=#00ff00>PlayerManager PlayerInfo Found{this.PlayerInfo} </color>");

        m_DicPlayerLevelData = PlayerLevelData.GetPlayerLevelDataFromJson().ToDictionary((x)=>x.Level);
        Debug.Log($"$\"<color=#00ff00>PlayerManager PlayerLevelData Found{m_DicPlayerLevelData} </color>");
       if(0 == m_PlayerInfo.Level)
       {
            PlayerLevelData DataLevelOne =  m_DicPlayerLevelData[1];
            LevelUp(1);
            m_PlayerInfo.Money = 5000;
            m_PlayerInfo.SavePlayerInfo();
       }

        _playerSkillsInfo = new PlayerSkillsInfo();
        _playerSkillsInfo.LoadJson();
        _dicPlayerSkillInfo=new Dictionary<int, PlayerSkillInfo>();
        _dicPlayerSkillIDAndIdxPairs = new Dictionary<int, int>();
        int idx = 0;
        foreach (var skillInfo in _playerSkillsInfo.skillsInfoList)
        {
            _dicPlayerSkillInfo.Add(skillInfo.SkillID, skillInfo);
            _dicPlayerSkillIDAndIdxPairs.Add(skillInfo.SkillID , idx++);
        }
        Subscribe();

        

    }
    public void UpdateSkillInfo(PlayerSkillInfo skillInfo)
    {
        _dicPlayerSkillInfo[skillInfo.SkillID]= skillInfo;
        _playerSkillsInfo.skillsInfoList[ _dicPlayerSkillIDAndIdxPairs[skillInfo.SkillID]]= skillInfo;

    }
    public PlayerSkillInfo GetSkillInfoByHandle(int skillHandle)
    {
        return _dicPlayerSkillInfo[skillHandle];
    }
    public void AddMoney(int Amount)
    {
        m_PlayerInfo.Money = m_PlayerInfo.Money+Amount;
        GameObject invenGO = Managers.UI.GetCachedUIByName("InventoryPannel_Canvas_Prefab"); //인벤 정보(돈) 업데이트
        if (invenGO != null)
        {
            InventoryDirector director = invenGO.GetComponentInChildren<InventoryDirector>();
            if (director != null && director.TextMoney != null)
                director.TextMoney.text = m_PlayerInfo.Money.ToString();
        }

    }
    void LevelUp(int NextLevel)
    {

        if(null == _statusScript)
        {
            GameObject Player = GameObject.FindGameObjectWithTag("Player");
            if (Player != null) 
                _statusScript =Player.GetComponent<StatusScript>();
        }


        //이전 누적 값을 뺀다
        if (null != _statusScript)
        {
            _statusScript.AttackDamage-=m_PlayerInfo.Attack;
            _statusScript.Defense-=m_PlayerInfo.Defense;
            _statusScript.CriDamage-=m_PlayerInfo.CriDemage;
            _statusScript.CriChance-=m_PlayerInfo.CriChance;
            _statusScript.MaxHealth-=m_PlayerInfo.MaxHP;
            _statusScript.MaxMana-=m_PlayerInfo.MaxMP;
        }
        PlayerLevelData NextLevelData = m_DicPlayerLevelData[NextLevel];
        m_PlayerInfo.Level=NextLevel;
        m_PlayerInfo.CriChance=m_PlayerInfo.CriChance+NextLevelData.CriChance;
        m_PlayerInfo.CriDemage=m_PlayerInfo.CriDemage+NextLevelData.CriDemage;
        m_PlayerInfo.Attack=m_PlayerInfo.Attack+NextLevelData.Attack;
        m_PlayerInfo.CurHP=m_PlayerInfo.MaxHP+NextLevelData.MaxHP;
        m_PlayerInfo.MaxHP=m_PlayerInfo.MaxHP+NextLevelData.MaxHP;
        m_PlayerInfo.CurMP=m_PlayerInfo.MaxMP+NextLevelData.MaxMP;
        m_PlayerInfo.MaxMP=m_PlayerInfo.MaxMP+NextLevelData.MaxMP;
        m_PlayerInfo.TotalExp=/*m_PlayerInfo.totalExp+*/NextLevelData.TotalExp;
        m_PlayerInfo.SkillPoint=m_PlayerInfo.SkillPoint+NextLevelData.SkillPoint;
        m_PlayerInfo.Defense=m_PlayerInfo.Defense+NextLevelData.Defense;
        m_PlayerInfo.ExtraAbilityPoints=m_PlayerInfo.ExtraAbilityPoints+NextLevelData.ExtraAbilityPoints;

        //갱신된 누적값을 더한다. 
        if (null != _statusScript)
        {
            _statusScript.AttackDamage  +=m_PlayerInfo.Attack;
            _statusScript.Defense       +=m_PlayerInfo.Defense;
            _statusScript.CriDamage     +=m_PlayerInfo.CriDemage;
            _statusScript.CriChance     +=m_PlayerInfo.CriChance;
            _statusScript.MaxHealth     += m_PlayerInfo.MaxHP;
            _statusScript.MaxMana       += m_PlayerInfo.MaxMP;
            _statusScript.CurHealth  =  _statusScript.MaxHealth;
            _statusScript.CurMana    =  _statusScript.MaxMana;
            _statusScript.Level      =  m_PlayerInfo.Level;
        }

        Managers.Event.Publish<Event_LevelUP>(new Event_LevelUP(m_PlayerInfo.Level)); 
    }
    /*Level Up조건-> Event 기반처리*/
    /*몬스터를 죽이면 몬스터로부터 id를 얻어오고 Exp를 증가시킨다.*/
    void Event_KillMonsterAndGetExp(Event_KillTarget e)
    {
       
        MonsterInfo monsterInfoGO = Managers.Monster.GetMonsterInfo(e.MonsterID);
        Managers.Player.AddEXP(monsterInfoGO.DropExp);
    }

    public void AddEXP(int extraEXP)
    {
        LevelPrev =m_PlayerInfo.Level;
        int remaining = extraEXP;
        while (remaining > 0)
        {
            int needed = Math.Max(1, m_PlayerInfo.TotalExp - m_PlayerInfo.CurExp);
            if (remaining < needed)
            {
                m_PlayerInfo.CurExp += remaining;
                break;
            }
            if (!m_DicPlayerLevelData.ContainsKey(m_PlayerInfo.Level + 1))
            {
                m_PlayerInfo.CurExp = m_PlayerInfo.TotalExp;
                break;
            }
            remaining -= needed;
            LevelUp(m_PlayerInfo.Level + 1);
            m_PlayerInfo.CurExp = 0;
        }
        if (null ==_statusBar)
        {
            GameObject hud = Managers.UI.GetOpenUIByName("HUD_Canvas_Prefab");
            if (hud != null) _statusBar = hud.GetComponentInChildren<UI_PlayerStatusBar>();
        }
        if (_statusBar != null) _statusBar.UpdateSlider(null, Player.ePlayerSlider.EXP);

    }
    protected  void Subscribe()
    {
        Managers.Event.Subscribe<Event_KillTarget>(Event_KillMonsterAndGetExp);
    }
    public void Clear()//Managers에서 Clear할때 정리
    {
        m_PlayerInfo?.SavePlayerInfo();
        _playerSkillsInfo.SavePlayerInfo();
        //UnSubscribe(); 
    }

}
