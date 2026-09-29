using UnityEngine;
using System.Collections.Generic;
public class QuestMiniDirector : UI_Base
{
    public enum GameObjects
    {
        QuestMini_Panel //나 자신

    }
    private bool _hasInitialized = false;
    GameObject _parent = null;
    List<Poolable> questHandles; 
    public override void Init()
    {
        if (true ==_hasInitialized) return;
        _hasInitialized = true;

        questHandles = new List<Poolable>();
        Bind<GameObject>(typeof(GameObjects));
        _parent = Get<GameObject>((int)GameObjects.QuestMini_Panel);

        Managers.Event.Subscribe<Event_QuestAtivated>(UpdateAccepedQuests);
        Managers.Event.Subscribe<Event_QuestCompleted>(UpdateAccepedQuests);

        UpdateDisplay();
    }
    public void Start()
    {
        Init();
    }
    //Quest를 Accept할때마다 호출한다. 
    public void UpdateAccepedQuests(Event_QuestAtivated evt)
    {
        UpdateDisplay();
    }
    public void UpdateAccepedQuests(Event_QuestCompleted evt)
    {
        UpdateDisplay();
    }
    public void UpdateDisplay()
    {
        ClearHandle();
        List<QuestRuntimeProcess> list = Managers.Quest.GetOnQuests();
        foreach (QuestRuntimeProcess process in list)
        {
            questHandles.Add(Managers.Pool.LendPoolableTo("QuestMini_TMP", _parent.transform));
            QuestMiniPart questMini = questHandles[questHandles.Count -1].GetComponent<QuestMiniPart>();
            questMini.Init();
            QuestData questData = Managers.Quest.GetQuestData(process._runtimeProcess.QuestCODE);
            questMini.SetQuestData(questData);
        }
    }
    public void ClearHandle()
    {
        foreach (var handle in questHandles)
        {
            Managers.Pool.GetBack(handle);
            handle.GetComponent<QuestMiniPart>().Unsubscribe();
        }
        questHandles.Clear();
    }


}
