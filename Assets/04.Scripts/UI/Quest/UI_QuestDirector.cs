using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
public class UI_QuestDirector : UI_Base
{
   
   
    private UI_QuestDisplayList _questDisplayList;
    private UI_QuestDisplayContents _questDisplayContents;
    private QuestDisplayRewards _questDisplayRewards;
    private UI_Popup _handle;
    private bool _hasInitialized = false;
    public enum Buttons
    {
        Ready_Button,
        On_Button,
        Com_Button

    }
    public enum GameObjects
    {

        Contents_Panel,
        RewardCellView_Contents,
        List_Panel,
       
    }
   
    public override void Init()
    {

        if (true ==_hasInitialized)
            return;
        _hasInitialized  =true;
        _handle = GetComponentInParent<UI_Popup>();
        Bind<GameObject>(typeof(GameObjects));

        _questDisplayList = Get<GameObject>((int)GameObjects.List_Panel).GetComponentInChildren<UI_QuestDisplayList>();
        _questDisplayContents = Get<GameObject>((int)GameObjects.Contents_Panel).GetComponentInChildren<UI_QuestDisplayContents>();
        _questDisplayRewards = Get<GameObject>((int)GameObjects.RewardCellView_Contents).GetComponentInChildren<QuestDisplayRewards>();

        _questDisplayList.Init();
        _questDisplayContents.Init();

       
    }
    public void SetQuestData (QuestData questData)
    {
        _questDisplayContents.SetQuestData(questData);
        _questDisplayRewards.SetRewards(questData);
    }
   

    public void Awake()
    {
        Init();
    }
    public void OnOpen()
    {
        _handle=Managers.UI.ShowPopupUI<UI_Popup>("Quest_Canvas_Prefab");
    }
    public void OnClose()
    {
        Managers.UI.ClosePopupUI(_handle);
    }
   
}
