using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIDialogue : UI_Base
{


    public enum Buttons
    {
        Accept_Button,
        TurnDown_Button,
        Next_Button,
        Finish_Button,
        END
    }
    public enum TMPs
    {
        Name_TMP,
        DialogueContext_TMP,
        AcceptText_TMP,
        TurnDownText_TMP,
        NextText_TMP,
        FinishText_TMP
    }

    public override void Init() { }
    //public override void Init() {
    //    Bind<Button>(typeof(Buttons));
    //    Bind<TextMeshProUGUI>(typeof(TMPs));
    //    m_NowSpeaker.SpeakerName = Get<TextMeshProUGUI>((int)TMPs.Name_TMP);
    //    m_NowSpeaker.DialogueContext =Get<TextMeshProUGUI>((int)TMPs.DialogueContext_TMP);

    //    m_NowSpeaker.ButtonSets = new ButtonSet[(int)Buttons.END];

    //    m_NowSpeaker.ButtonSets[(int)Buttons.Accept_Button].Button=Get<Button>((int)Buttons.Accept_Button); //->이벤트 추가
    //    m_NowSpeaker.ButtonSets[(int)Buttons.TurnDown_Button].Button=Get<Button>((int)Buttons.TurnDown_Button);
    //    m_NowSpeaker.ButtonSets[(int)Buttons.Next_Button].Button=Get<Button>((int)Buttons.Next_Button);
    //    m_NowSpeaker.ButtonSets[(int)Buttons.Finish_Button].Button=Get<Button>((int)Buttons.Finish_Button);

    //    m_NowSpeaker.ButtonSets[(int)Buttons.Accept_Button].ButtonText=Get<TextMeshProUGUI>((int)TMPs.AcceptText_TMP); //->이벤트 추가
    //    m_NowSpeaker.ButtonSets[(int)Buttons.TurnDown_Button].ButtonText=Get<TextMeshProUGUI>((int)TMPs.TurnDownText_TMP);
    //    m_NowSpeaker.ButtonSets[(int)Buttons.Next_Button].ButtonText=Get<TextMeshProUGUI>((int)TMPs.NextText_TMP);
    //    m_NowSpeaker.ButtonSets[(int)Buttons.Finish_Button].ButtonText=Get<TextMeshProUGUI>((int)TMPs.FinishText_TMP);

    //}

    [System.Serializable]
    public class ButtonSet
    {
        [SerializeField]
        public string ButtonKey;
        [SerializeField]
        public TextMeshProUGUI ButtonText;
        [SerializeField]
        public Button Button;
    }


    [System.Serializable]
    public struct Speaker
    {
        [SerializeField]
        public  Image                       DialogueBoxImage;
        [SerializeField]
        public  TextMeshProUGUI             SpeakerName;
        [SerializeField]
        public  TextMeshProUGUI             DialogueContext;

        [SerializeField]
        public ButtonSet[] ButtonSets;

        public Dictionary<string, Tuple<TextMeshProUGUI, Button>> Buttons;

    }


    [System.Serializable]
    public struct DialogueNode
    {
        
        public Dialogue       NowDialogue;

        public int            NowNodeID;
        public string         NowSpeakerName;
        public string[]       NowSpeakerContexts;
        public string[]       NowAnimationNames;

        public int            iNowSpeakerContextsIndex;

    }
    private DialogueNode    m_NowNode;
    [SerializeField]
    private Speaker         m_NowSpeaker;

    private Dialogues       m_Dialogues = null;

    private string          m_DialogueKey;
    private bool            m_isFirstStart = true;
    private bool            m_isAutoStart  = true;

    private DIALOGUE_RESULT m_NowDialogueResult= DIALOGUE_RESULT.DEFAULT;
    bool IsDialogueEnd = false;//나중에 이벤트 매니저 같은거 만들면 등록하기
    float m_fTypingSpeed = 0.1f;

   UI_Popup _handle;

    public void AddAcceptEvent(UnityAction action) //퀘스트 실행 람다 함수 전달 
    {
        
        m_NowSpeaker.Buttons["ACCEPT"].Item2.onClick.AddListener(action);
    }
    private void Awake()
    {
        _handle = transform.root.GetComponent<UI_Popup>();
        /*Button Settings*/
        m_NowSpeaker.Buttons = new Dictionary<string, Tuple<TextMeshProUGUI, Button>>();
        foreach (var ButtonSetGO in m_NowSpeaker.ButtonSets)
        {
            m_NowSpeaker.Buttons.Add(ButtonSetGO.ButtonKey,new Tuple<TextMeshProUGUI,Button>(ButtonSetGO.ButtonText, ButtonSetGO.Button));
            switch (ButtonSetGO.ButtonKey)
            { 
               
                case "NEXT":
                    {
                        ButtonSetGO.Button.onClick.AddListener(() => {
                            ++m_NowNode.iNowSpeakerContextsIndex;
                            MoveOnToNextContext(); });
                        break;
                    }
                case "FINISH":
                    {
                        ButtonSetGO.Button.onClick.AddListener(() => {
                            ClearButtons();
                            //this.gameObject.SetActive(false);
                            Managers.UI.ClosePopupUI(_handle);
                           
                        });
                        break;
                    }

            }

        }
    }
 
    public Dialogues Dialogues
    {
        get => m_Dialogues;
    }
    /* 퀘스트 혹은 외부 이벤트를 통해서 외부에서 키를 부여해 줍니다.*/
    public void SetDialogues(string _DialogueKey)
    {
        Managers.UI.ShowPopupUI<UI_Popup>("DialogueUI_Canvas_Prefab"); /*UI 활성화*/
        /*처음은 무조건 0번째 노드부터*/
        m_NowNode.NowNodeID=0;
        m_isFirstStart =true;
        m_DialogueKey =_DialogueKey;
        IsDialogueEnd = false; 
        Managers.Dialogue.GetDialoguesByName(m_DialogueKey, out m_Dialogues);
        StartDialogue();
    } 
    private LEAFNODE_TYPE SetNextDialogue()
    {
        //여기서 세팅해놓고 
        if(null ==m_Dialogues)
        {
            Debug.Log($"<color=#ff0000> 다이얼로그 목록이 비어 있습니다.</color>");
        }

        m_NowNode.NowNodeID                  = m_NowNode.NowDialogue.NextBranchNodeID[0];
        if(-1 ==m_NowNode.NowNodeID)
        {
            OnClose();
            return LEAFNODE_TYPE.LEAF_DIALOGUE_END;
        }
        SetNowDialogue();
        return LEAFNODE_TYPE.NON_LEAF;
    }
    public void SetNowDialogue()
    {
        m_NowNode.NowDialogue                = m_Dialogues.GetDialogueByNodeID(m_NowNode.NowNodeID);
        m_NowNode.NowSpeakerName             = m_NowNode.NowDialogue.SpeakerName;
        m_NowNode.NowSpeakerContexts         = m_NowNode.NowDialogue.Scripts.ToArray();
        m_NowNode.NowAnimationNames          = m_NowNode.NowDialogue.AnimationName.ToArray();
        m_NowNode.iNowSpeakerContextsIndex   = 0;
        m_NowSpeaker.SpeakerName.text= m_NowNode.NowSpeakerName;
    }
    void SetBranchDialogue()
    {
        /*1. NextNode, NextLabel, NextType을 조사한다.  */
        /*2. NextNode에 대해서 어떤 버튼과 매핑해야 하는가? */
        List<string> ResponseTypeKeys =m_NowNode.NowDialogue.ResponseTypes;
        for (int i = 0; i<ResponseTypeKeys.Count; ++i)
        {

            string key=ResponseTypeKeys[i];
            if ("ACCEPT" !=key)
            m_NowSpeaker.Buttons[key].Item2.onClick.RemoveAllListeners();
            
            m_NowSpeaker.Buttons[key].Item2.gameObject.SetActive(true);
            m_NowSpeaker.Buttons[key].Item1.text = m_NowNode.NowDialogue.Labels[i];
            int v = m_NowNode.NowDialogue.NextBranchNodeID[i];


            m_NowSpeaker.Buttons[key].Item2.onClick.AddListener(() => {
                if ("ACCEPT" ==key)
                    m_NowSpeaker.Buttons[key].Item2.onClick.RemoveAllListeners();

                /*다음에 실행될 노드를 세팅한다.*/
                /*현재 노드로 교체후 코루틴 수행한다.*/
                m_NowNode.NowNodeID = v;/*캡쳐때매 못 묶음*/
                if(-1 ==m_NowNode.NowNodeID)
                {
                    OnClose();
                    return;
                }
                SetNowDialogue();
                MoveOnToNextContext();
                
            });
        }
    }

    public void StartDialogue()
    {
        if(true == m_isFirstStart)
        {
            if (true ==m_isAutoStart)
            {
                SetNowDialogue();
                MoveOnToNextContext();
            }
            m_isFirstStart = false;
        }
        
            /*이벤트로 나머지 처리*/
        

    }

    private void MoveOnToNextContext()
    {
        StopCoroutine("TypingEffect"); /*모두 출력하지 않고 다음 대화로 넘어간다.*/
        ClearButtons();
        /*모든 문자를 출력했다 */
        /* CASE 1: 아직 출력할 CONTEXTS가 남아있다. -> 다음CONTEXT를 출력한다 */
        /* CASE 2 : 마지막 context라면 Branch인지 아닌지 확인한다.*/
        /* CASE 2-1: 분기점이 있다 ->버튼을 TYPE과 매핑한다. 버튼을 누르면 버튼에 값을 세팅하고 세팅한 값에따라서 어떤 노드로 갈지 판정한다.*/
        /* CASE 2-2: 다이얼로그를 모두 돌려보니 종료 코드라면  완료 버튼을 활성화한다.*/
        /* CASE 3: 모든 Context를 출력했다  -> 다음 다이얼로그의 노드를 세팅한다.-> */
        /* CASE 4: 다음 노드는 첫번째 노드만 저장되어 있으므로 첫번째 노드를 따라서 세팅하고 다음 코루틴을 실행한다. */

        /*Next를 눌러야 문장스택이 1씩 증가한다. 마지막 문장에 도달해도 인덱스를 가리킬 뿐 개수를 나타내지 않는다.*/



        if (!AreContextsEnd())
        {
            /*CASE 1*/
            m_NowSpeaker.Buttons["NEXT"].Item2.gameObject.SetActive(true);
            /*마지막 문장도 여기에 걸린다.*/
            /*CASE 2*/
            if (AreContextsAtLastIndex())
            { 
                /*CASE 2-1*/
                if (isNowBranch())
                {
                    m_NowSpeaker.Buttons["NEXT"].Item2.gameObject.SetActive(false);
                    SetBranchDialogue();
                    
                }
                /*CASE 2-2*/
                switch(isLeafNode())
                {
                    case LEAFNODE_TYPE.LEAF_ONLY_ACCEPT:
                        foreach (var ButtonPair in m_NowSpeaker.Buttons) { ButtonPair.Value.Item2.gameObject.SetActive(false); }
                        m_NowSpeaker.Buttons["ACCEPT"].Item2.gameObject.SetActive(true);
                        break;
                    case LEAFNODE_TYPE.LEAF_DIALOGUE_END:
                        foreach (var ButtonPair in m_NowSpeaker.Buttons) { ButtonPair.Value.Item2.gameObject.SetActive(false); }
                        m_NowSpeaker.Buttons["FINISH"].Item2.gameObject.SetActive(true);
                        break;
                    case LEAFNODE_TYPE.NON_LEAF:
                        break;
                }

                //if(1==isLeafNode())
                //{   foreach (var ButtonPair in m_NowSpeaker.Buttons)
                //    { ButtonPair.Value.Item2.gameObject.SetActive(false); }
                //    m_NowSpeaker.Buttons["FINISH"].Item2.gameObject.SetActive(true);
                //}
            }
            StartCoroutine("TypingEffect");
        }
        else
        { 
                /*CASE 3*/
            if(LEAFNODE_TYPE.LEAF_DIALOGUE_END != SetNextDialogue())
            StartCoroutine("TypingEffect");
                /*NextButton 활성화*/
            m_NowSpeaker.Buttons["NEXT"].Item2.gameObject.SetActive(true);


           switch(isLeafNode())
            {
                case LEAFNODE_TYPE.LEAF_ONLY_ACCEPT:
                    foreach (var ButtonPair in m_NowSpeaker.Buttons) { ButtonPair.Value.Item2.gameObject.SetActive(false); }
                    m_NowSpeaker.Buttons["ACCEPT"].Item2.gameObject.SetActive(true);
                    break;
                case LEAFNODE_TYPE.LEAF_DIALOGUE_END:
                    foreach (var ButtonPair in m_NowSpeaker.Buttons) { ButtonPair.Value.Item2.gameObject.SetActive(false); }
                    m_NowSpeaker.Buttons["FINISH"].Item2.gameObject.SetActive(true);
                    break;
                case LEAFNODE_TYPE.NON_LEAF:
                    break;
                    //foreach (var ButtonPair in m_NowSpeaker.Buttons) { ButtonPair.Value.Item2.gameObject.SetActive(false); }
                    //m_NowSpeaker.Buttons["FINISH"].Item2.gameObject.SetActive(true);
                    //break;
                   
            }
                
          
        }
    }
    private IEnumerator TypingEffect()
    {

        DialogueEffectEvent.Invoke(
            m_NowNode.NowDialogue.DialogueEffect[m_NowNode.iNowSpeakerContextsIndex],
            m_NowNode.NowDialogue.EffectAmount[m_NowNode.iNowSpeakerContextsIndex]);
        
        m_NowSpeaker.DialogueContext.text="";
        int NowIdx = 0; 
       /*컨텍스트를 모두 출력한다.*/
        while (NowIdx< m_NowNode.NowSpeakerContexts[m_NowNode.iNowSpeakerContextsIndex].Length)
        {
            yield return new WaitForSeconds(m_fTypingSpeed); 
            ++NowIdx;
            m_NowSpeaker.DialogueContext.text=
                m_NowNode.
                NowSpeakerContexts[m_NowNode.iNowSpeakerContextsIndex].
                Substring(0, NowIdx);
        }
        
        yield return null ;
    }
    private bool isNowBranch() 
    {
        return m_NowNode.NowDialogue.IsBranch;
    }
    private bool AreContextsEnd()
    {

       return m_NowNode.iNowSpeakerContextsIndex >= m_NowNode.NowSpeakerContexts.Length;
    }
    private bool AreContextsAtLastIndex()
    {

        return m_NowNode.iNowSpeakerContextsIndex >= m_NowNode.NowSpeakerContexts.Length - 1;
    }

    private LEAFNODE_TYPE isLeafNode()
    {
        //leaf node is accept
        if(1 == m_NowNode.NowDialogue.NextBranchNodeID.Count && m_NowNode.NowDialogue.NextBranchNodeID[0] ==m_NowNode.NowNodeID) //같은 노드를 지정했을 경우 
         return LEAFNODE_TYPE.LEAF_ONLY_ACCEPT;

        if (0 >= m_NowNode.NowDialogue.NextBranchNodeID.Count) //just Finish
            return LEAFNODE_TYPE.LEAF_DIALOGUE_END;

        return LEAFNODE_TYPE.NON_LEAF;
    }

    public void ClearButtons()
    {
        foreach (var ButtonPair in m_NowSpeaker.Buttons)
        {
            ButtonPair.Value.Item2.gameObject.SetActive(false);
        }
    }

    public void OnClose()
    {
        Managers.UI.ClosePopupUI(_handle);
    }

    public void OnOpen()
    {
        Managers.UI.ShowPopupUI<UI_Popup>("DialogueUI_Canvas_Prefab");
    }
}
