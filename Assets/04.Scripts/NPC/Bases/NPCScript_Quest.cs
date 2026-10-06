using System.Collections.Generic;
using UnityEngine;

public class NPCScript_Quest : NPCScript
{
    protected bool _isTalkable = false;
    protected bool _isTalking = false;
    //[SerializeField] public List<Dialogues> _dialogues;
    // [SerializeField] public string _dialogueKey;

  
    Poolable _questMarkHandler;
    [SerializeField] public List<QuestData> _listQuestData;

    public virtual void Awake()
    {
        Managers.Event.Subscribe<Event_TaskUpdated>(CheckTalkable);  
        Managers.Event.Subscribe<Event_QuestAtivated>(CheckTalkable);
    }
    private void OnDestroy()
    {
        EventManager eventManager = Managers.EventIfExists;
        if (eventManager == null) return;
        eventManager.UnSubscribe<Event_TaskUpdated>(CheckTalkable);
        eventManager.UnSubscribe<Event_QuestAtivated>(CheckTalkable);
    }
    private void CheckTalkable(Event_QuestAtivated evt)
    {
        CheckTalkable();
    }
    public void CheckTalkable(Event_TaskUpdated evt)
    {
        if (true ==_isTalkable) return;
        foreach (QuestData data in _listQuestData)
        {
            if (null ==data) continue;
            if (false == data.CheckPreRequisites()) continue;

            QUEST_STATE questStata = Managers.Quest.QuestState(data);
            if (QUEST_STATE.QUEST_ACCEPTED != questStata) continue;

            //진행중인 퀘스트

            foreach (var task in data.tasks) //todo 한캐릭터가 여러 개를 동시에 수행하게 하려면, 말을 걸면 퀘스트 창이 뜨게 만들고 클릭해서 크ㅔ스트 진행( 버튼에 할당해서 for문 안 블록 수행) 
            {
                if (null ==task) continue;
                if (task.NPCID !=NPC_ID) continue; //내가 받은 TASK의 NPC ID 와 NPC ID가 같을 때만 다이얼로그를 수행한다.

                eQuestTaskState taskState = Managers.Quest.TaskState(data, task);
                if (eQuestTaskState.ACCEPTED != taskState) continue;

                _isTalkable = true;
                ShowupQuestionMark();
            }
        }
    }

    protected void CheckTalkable()
    {
        if (true ==_isTalkable) return;
        foreach (QuestData data in _listQuestData)
        {
            if (null ==data) continue;
            if (false == data.CheckPreRequisites()) continue;

            QUEST_STATE questStata = Managers.Quest.QuestState(data);
            if (QUEST_STATE.QUEST_ACCEPTED != questStata) continue;

            //진행중인 퀘스트

            foreach (var task in data.tasks) //todo 한캐릭터가 여러 개를 동시에 수행하게 하려면, 말을 걸면 퀘스트 창이 뜨게 만들고 클릭해서 크ㅔ스트 진행( 버튼에 할당해서 for문 안 블록 수행) 
            {
                if (null ==task) continue;
                if (task.NPCID !=NPC_ID) continue; //내가 받은 TASK의 NPC ID 와 NPC ID가 같을 때만 다이얼로그를 수행한다.

                eQuestTaskState taskState = Managers.Quest.TaskState(data, task);
                if (eQuestTaskState.ACCEPTED != taskState) continue;

                _isTalkable = true;
                ShowupQuestionMark();
            }
        }
    }

    public void ShowupQuestionMark()
    {

        _questMarkHandler = Managers.Pool.LendPoolableTo("Mark_Quest", this.transform);
        _questMarkHandler.transform.localPosition = new Vector3(0, 2.3f, 0);
        _questMarkHandler.transform.rotation = Quaternion.identity;


    }
    public void CloseQuestionMark()
    {
        Managers.Pool.GetBack(_questMarkHandler);
    }
   


    protected override void ExecuteDialogue()//일단 대화걸 수있는 것이 있다면 대화 수행
    {
        foreach (QuestData data in _listQuestData)
        {
            if (null ==data) continue;
            if (false == data.CheckPreRequisites()) continue;

            QUEST_STATE questStata = Managers.Quest.QuestState(data);
            if (QUEST_STATE.QUEST_ACCEPTED != questStata) continue;

            //진행중인 퀘스트

            foreach (var task in data.tasks) //todo 한캐릭터가 여러 개를 동시에 수행하게 하려면, 말을 걸면 퀘스트 창이 뜨게 만들고 클릭해서 크ㅔ스트 진행( 버튼에 할당해서 for문 안 블록 수행) 
            {
                if (null ==task) continue;
                if (task.NPCID !=NPC_ID) continue; //내가 받은 TASK의 NPC ID 와 NPC ID가 같을 때만 다이얼로그를 수행한다.

                eQuestTaskState taskState = Managers.Quest.TaskState(data, task);
                if (eQuestTaskState.ACCEPTED != taskState) continue;




               
                UI_Popup popUp = Managers.UI.ShowPopupUI<UI_Popup>("DialogueUI_Canvas_Prefab");
                UIDialogue uiDialogue = popUp.gameObject.GetComponentInChildren<UIDialogue>();
                if (!uiDialogue.SetDialogues(task.DialogKey)) return;
                uiDialogue.AddAcceptEvent(
                    () =>
                    {
                        Event_TalkToNPC evt = new Event_TalkToNPC(NPC_ID, data.QuestCODE, task.SubTaskCODE);
                        Managers.Event.Publish<Event_TalkToNPC>(evt);

                        _isTalkable =false;
                        if (_questMarkHandler != null) CloseQuestionMark();
                        CheckTalkable();
                    }
                );// 성공 버튼 클릭 시 수행할 이벤트를 콜백으로 등록
               


                return;

            }


        }
    }
}
