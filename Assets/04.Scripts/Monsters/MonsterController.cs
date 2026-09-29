using TinyScript;
#if UNITY_EDITOR
using UnityEditor.AssetImporters;
#endif
using UnityEngine;

public class MonsterController : IController  //이벤트 기반으로 바꿀 예정
{
    protected MonsterInfo m_MonsterInfo;
    
    [SerializeField]
    protected int m_ID;


    private StatusScript _status;


    public MonsterInfo GetMosnterInfo()
    {
        
        return m_MonsterInfo;
    }

    public void GetMonsterInfoByID()
    {
        m_MonsterInfo = Managers.Monster.GetMonsterInfo(m_ID);
        if(null == m_MonsterInfo)
        {
            Debug.Log("<color=#ff0000>해당 id:{m_ID}를 가진 몬스터는 없습니다.</color>");
        }
    }
    
       
    
}
