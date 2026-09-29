using NUnit.Framework;
using UnityEngine;

using System.Collections.Generic;
public class NPCScript_Quest_REN : NPCScript_Quest
{


    public override void Awake()
    {
        //1. 진행할 수있는 퀘스트인지 확인하기
        //2. 퀘스트가 진행중인지 확인하기 
        //3. 진행 중인 task인지 확인하기
        //4-1. 진행중이라면, 태스크 확인하기 npc 비교하고 다이얼로그 실행하기 
        //4-2  완료됐다면 다음 태스크 확인, 
        base.Awake();
        CheckTalkable();


    }

    protected void OnTriggerStay(Collider other)
    {
        if (false  == other.CompareTag("Player")) return;


        CheckTalkable();
        npcAction = null;
        npcAction +=ExcuteNPCAction;
        



        if (isPlayerInRange== true) return;
        isPlayerInRange =true;
        ShowupNameMark();

    }
    protected  void OnTriggerExit(Collider other)
    {
       
        if (false  == other.CompareTag("Player")) return;
        isPlayerInRange =false;

        if (_nameTextHandler != null) Managers.Pool.GetBack(_nameTextHandler);
        _nameTextHandler = null;
    }

    // Update is called once per frame
    protected  void Update()
    {
        if (isPlayerInRange&&_isTalkable&&Input.GetKeyDown(KeyCode.Tab))
        {
            ExecuteDialogue();
        }
    }
    public override void ExcuteNPCAction()
    {
       ExecuteDialogue();
       CheckTalkable();
    }
}
