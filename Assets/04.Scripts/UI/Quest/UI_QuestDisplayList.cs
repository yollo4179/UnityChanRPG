using Quest;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.UI;

namespace Quest
{
    public enum eQuestButtonType
    {
        READY,
        ON,
        COMPLETED,
    }
}
public class UI_QuestDisplayList : UI_Base
{

   
    private List<Poolable> _handles;
    private Button _button_Ready;
    private Button _button_On;
    private Button _button_Completed;

    private QuestData _nowFocus; 
    private GameObject _parentGO;
    private Quest.eQuestButtonType _nowButtonType;

    private UI_QuestDirector _questDirector;
    public enum Buttons
    {
        Ready_Button,
        On_Button,
        Com_Button

    }
    public enum GameObjects
    {
        QuestsList_GO
    }
    bool _hasInitialized = false;
    public override void Init()
    {
        if (true ==_hasInitialized)
            return;
        _hasInitialized  =true;
        _handles = new List<Poolable>();

        Bind<Button>(typeof(Buttons));
        Bind<GameObject>(typeof(GameObjects));
        _button_Ready        =         Get<Button>((int)Buttons.Ready_Button);
        _button_On           =         Get<Button>((int)Buttons.On_Button);
        _button_Completed    =         Get<Button>((int)Buttons.Com_Button);

        _parentGO =  Get<GameObject>((int)GameObjects.QuestsList_GO);

        _questDirector =GetComponentInParent<UI_QuestDirector>();

        _button_Ready.onClick.AddListener(
            () => {
                _nowButtonType = eQuestButtonType.READY;
                SelectButton();

            });
        _button_On.onClick.AddListener(
           () => {
               _nowButtonType = eQuestButtonType.ON;
               SelectButton();
           });
        _button_Completed.onClick.AddListener(
           () => {
               _nowButtonType = eQuestButtonType.COMPLETED;
               SelectButton();
           });
        SelectButton();

        _questDirector.SetQuestData(_nowFocus);

        Managers.Event.Subscribe<Event_TaskUpdated>(UpdateDisplayList);
        Managers.Event.Subscribe<Event_QuestAtivated>(UpdateDisplayList);
        Managers.Event.Subscribe<Event_QuestCompleted>(UpdateDisplayList);
    }
    public void UpdateDisplayList(Event_TaskUpdated evt)
    {
        SelectButton();
    }
    public void UpdateDisplayList(Event_QuestAtivated evt) {
        SelectButton();
    }
    public void UpdateDisplayList(Event_QuestCompleted evt)
    {
        SelectButton();
    }
    public void SelectButton()
    {
        _button_Ready.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        _button_On.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        _button_Completed.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        switch (_nowButtonType)
        {
            case eQuestButtonType.READY:
                _button_Ready.GetComponentInChildren<TextMeshProUGUI>().color = Color.yellow;
                break;
            case eQuestButtonType.ON:
                _button_On.GetComponentInChildren<TextMeshProUGUI>().color = Color.yellow;
                break;
            case eQuestButtonType.COMPLETED:
                _button_Completed.GetComponentInChildren<TextMeshProUGUI>().color = Color.yellow;
                break;
        }

        var listQuests = DisplayQuests();
        if (null == listQuests || 0>=listQuests.Count)
        {
            _nowFocus =null; _questDirector.SetQuestData(_nowFocus);
            return;
        }
        _nowFocus = Managers.Quest.GetQuestData(listQuests[0]._runtimeProcess.QuestCODE);
        _questDirector.SetQuestData(_nowFocus);
    }
    public List<QuestRuntimeProcess> DisplayQuests()
    {
        ClearQuestList();
        var questDataList = Managers.Quest.GetQuestDataList();// 조건을 검사하고 진행 할수 있는 퀘스트들을 나열한다.(카탈로그에 등록되어있는)
        var runningQuestList = Managers.Quest.GetQuestRuntimeProcessList(); //진행중인퀘스트와 진행 완료된 퀘스트들을 나열하기 위해 사용한다.


        List<QuestRuntimeProcess> listQuests = null;


        switch (_nowButtonType)
        {
            case eQuestButtonType.READY:

                listQuests = Managers.Quest.GetReadyQuests();
                break;
            case eQuestButtonType.ON:
                listQuests = Managers.Quest.GetOnQuests();
                break;
            case eQuestButtonType.COMPLETED:
                listQuests = Managers.Quest.GetCompletedQuests();
                break;
        }
        if (null == listQuests) return null;

        foreach (var quest in listQuests)
        {
            Poolable pool = null;
            _handles.Add(pool = Managers.Pool.LendPoolableTo("QusetTitle_Button", _parentGO.transform));
            QuestData data = Managers.Quest.GetQuestData(quest._runtimeProcess.QuestCODE);
            QuestListButton nowSlot = _handles[_handles.Count-1].GetComponent<QuestListButton>();
            nowSlot.SetQuestData(data, _nowButtonType);
            Button btn =  nowSlot.GetTitleButton();
            btn.onClick.AddListener(() => {
                _nowFocus = data;
                _questDirector.SetQuestData(_nowFocus);
            }
            );
            //todo 
            //정보 채우기 
            //pool.GetComponent<>
        }
        return listQuests;

    }
    public void ClearQuestList()
    {
        foreach(var quest in _handles)
        {
            Managers.Pool.GetBack(quest);
        }
        _handles.Clear();
    }




}
