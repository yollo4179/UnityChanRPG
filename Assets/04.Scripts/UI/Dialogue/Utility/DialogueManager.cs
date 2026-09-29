using System.Collections.Generic;
using UnityEngine;

public class DialogueManager 
{
  
    private Dictionary<string, Dialogues> DialogEventList; 

    private DialogueParser m_DialogueParser;

    public void GetDialoguesByName(string _DialogueKey ,out Dialogues _Dialogues)
    {
        if (DialogEventList.ContainsKey(_DialogueKey))
        {

            _Dialogues = DialogEventList[_DialogueKey]; //해당 다이얼로그 뭉치 반환
            return; 
        }
       
        Debug.Log($"<color=#00ffff> 해당키 ({ _DialogueKey})를 가진 다이얼로그 이벤트는 존재하지 않습니다.</color>");
        _Dialogues = null;
        
       

    }

    public void Init()
    {
        DialogueEffectEvent.Init();
        LoadDialoges();
    }
    private void LoadDialoges()
    {
        m_DialogueParser =new DialogueParser();

        DialogEventList = m_DialogueParser.CSVDialogueParser();//다이얼로그 map 생성 후 방환
        /*To do*/
        /*Parser를 통해서 이벤트 채우기 */
    }

}
