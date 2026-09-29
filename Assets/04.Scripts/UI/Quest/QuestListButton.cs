using Quest;
using TMPro;
using UnityEngine;
using UnityEngine.UI; 
public class QuestListButton : UI_Base
{
   public enum Buttons
   {
        QusetTitle_Button,
        QuestAccept_Button
    }
    public enum TMPs
    {
        QusetTitle_TMP,
        QuestState_TMP,
    }

    Quest.eQuestButtonType _nowButtonType; 
   private QuestData _questData;

    Button _questTitleButton;
    Button _questAcceptButton;
    TextMeshProUGUI _textQuestTitle;
    TextMeshProUGUI _textQuestState;
    [SerializeField] string[] _buttonTexts = { "수락", "진행 중", "완료" };

    UI_QuestDisplayList _listPanel = null;
    UI_QuestDirector _questDirector;
    bool _hasInitialized = false;
    public Button GetTitleButton() { return _questTitleButton; }
    public override void Init()
    {
        if (true== _hasInitialized) return;
        _hasInitialized  = true; 
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(TMPs));
        _questTitleButton = Get<Button>((int)Buttons.QusetTitle_Button);
        _questAcceptButton = Get<Button>((int)Buttons.QuestAccept_Button);
        _textQuestTitle = Get<TextMeshProUGUI>((int)TMPs.QusetTitle_TMP);
        _textQuestState = Get<TextMeshProUGUI>((int)TMPs.QuestState_TMP);

        _listPanel = GetComponentInParent<UI_QuestDisplayList>();
        _questDirector =GetComponentInParent<UI_QuestDirector>();
    }
    public void SetQuestData(QuestData questData , Quest.eQuestButtonType buttonType )
    {
        Init();
        _listPanel = GetComponentInParent<UI_QuestDisplayList>();
        _questDirector =GetComponentInParent<UI_QuestDirector>();
        _questData =questData;
        _nowButtonType = buttonType;

        _textQuestTitle.text = questData.QuestTitle;
        _textQuestState.text = _buttonTexts[(int)buttonType];

        _questTitleButton.onClick.RemoveAllListeners();
        _questTitleButton.onClick.AddListener(
            ()=> {
                _questDirector.SetQuestData(_questData);
            });
        _questAcceptButton.onClick.RemoveAllListeners();


        switch (buttonType)
        {
            case eQuestButtonType.READY:
                _questAcceptButton.interactable=true;
                _questAcceptButton.enabled = true;
                _questAcceptButton.onClick.AddListener(() => {
                    Managers.Quest.ActivateQuest(questData); //AcceptQuest로 바꾼다.
                    _listPanel?.DisplayQuests();// 리프레시 
                }
                );
                break;
            case eQuestButtonType.ON:
                _questAcceptButton.interactable=false;
                _questAcceptButton.enabled = false; 
                break;
            case eQuestButtonType.COMPLETED:
                _questAcceptButton.interactable=false;
                _questAcceptButton.enabled = false;
                break;
        }

    }


}
