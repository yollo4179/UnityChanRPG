using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Runtime.ExceptionServices;

using UnityEngine.UIElements;
enum DIALOGUE_NODE{
    EVENT_NAME,
    SPEAKER_NAME,
    NODE_ID,
    IS_BRANCH,
    ANIMATION_NAME,
    SCRIPT,
    RESULT,
    DIALOGUE_EFFECT,
    DIALOGUE_EFFECT_AMOUNT,
    DIALOGUE_NODE_END
}
enum DIALOGUE_EDGE
{
    EVENT_NAME,
    FROM_NODEID,
    TO_NODEID,
    BUTTON_LABEL,
    MY_RESPONSE,
    DIALOGUE_EDGE_END,
}

class EdgeData
{
    public string ButtonLabel;
    public string myResponse;
}

public class DialogueParser
{
 
    public Dictionary<string, Dialogues> CSVDialogueParser(string _NodeFileName= "DialogueNodes", string _EdgeFileName= "DialogueEdges") 
    {

        /*엣지부터 정리합니다.*/
        /*인접 리스트로 처리합니다. From Node -> Multiple [To node] and Value____*/
        Dictionary<string, Dictionary<int, List<Tuple<int, EdgeData>>>> EdgeDataDictionary = new Dictionary<string, Dictionary<int, List<Tuple<int, EdgeData> >>>();
        TextAsset EdgeAsset = Resources.Load<TextAsset>("Data/CSV/" + _EdgeFileName);
        TextAsset NodeAsset = Resources.Load<TextAsset>("Data/CSV/" + _NodeFileName);
        Dictionary<string, Dialogues> DailogueDictionary = new Dictionary<string, Dialogues>();
        if (NodeAsset == null || EdgeAsset == null)
        {
            Debug.LogError("Dialogue CSV files not found.");
            return DailogueDictionary;
        }
        var Lines = EdgeAsset.text.Replace("\r\n", "\n").Replace("\r", "\n");
        var EdgeRows = Lines.Split('\n', StringSplitOptions.RemoveEmptyEntries);

       
        
        
        string EventName = "";
        string NowEventName = "";
        for (int i = 1; i<EdgeRows.Length;++i)
        {
            string[] EdgeRowElements = EdgeRows[i].Split(new char[] { ',' });

            EventName = EdgeRowElements[(int)DIALOGUE_EDGE.EVENT_NAME];

            if("END"!=EventName.ToString() && ""!=EventName.ToString())
            {
                NowEventName = EventName;
                EdgeDataDictionary.Add(EventName,new Dictionary<int, List<Tuple<int, EdgeData>>>());
            }

            
            
            int ToNodeID = int.Parse(EdgeRowElements[(int)DIALOGUE_EDGE.TO_NODEID].Trim());
            int FromNodeID = int.Parse(EdgeRowElements[(int)DIALOGUE_EDGE.FROM_NODEID].Trim());
            
            EdgeData NowEdgeData = new EdgeData();
            if(!EdgeDataDictionary[NowEventName].ContainsKey(FromNodeID))
                EdgeDataDictionary[NowEventName][FromNodeID] =new List<Tuple<int, EdgeData>>();
            
            NowEdgeData.ButtonLabel = EdgeRowElements[(int)DIALOGUE_EDGE.BUTTON_LABEL].Trim();
            NowEdgeData.myResponse = EdgeRowElements[(int)DIALOGUE_EDGE.MY_RESPONSE].Trim();
            
            EdgeDataDictionary[NowEventName][FromNodeID].Add(new Tuple<int,EdgeData> (ToNodeID, NowEdgeData ) );
        }

    
        Debug.Log($"<color=cyan> 엣지 파싱 완료{EdgeDataDictionary}</color>");
        
            
        Debug.Log($"<color=cyan> 엣지 정렬 완료{EdgeDataDictionary}</color>");

        /*노드 처리합니다*/
        Lines = NodeAsset.text.Replace("\r\n", "\n").Replace("\r", "\n");
        string[] NodeRows = Lines.Split(new char[] { '\n' },StringSplitOptions.RemoveEmptyEntries);
        /*csv 첫 행은 설명 */
        NowEventName="";
        EventName="";
        Dialogue NowDialogue=null;
        
        for (int i = 1; i<NodeRows.Length;)
        {
            string[] NodeElements = NodeRows[i].Split(new char[] { ',' });

            /*Set One Dialogue Event*/
            EventName = NodeElements[(int)DIALOGUE_NODE.EVENT_NAME].ToString();
            if (""!=EventName && "END"!=EventName)
            {
             
                NowEventName =EventName;
                DailogueDictionary.Add(NowEventName, new Dialogues());
                Debug.Log($"<color=#00ffff>{NowEventName}다이얼로그 이벤트 추가</color>");
            }

            /*화자의 이름이 비어있지 않다. (다이얼로그 시작)*/
            /*하나의 노드에 모든 모든 스크립트 들을 넣는다.*/
            int NowNodeID = -1;
            string strNodeID = NodeElements[(int)DIALOGUE_NODE.NODE_ID].ToString().Trim();
            string SpeakerName = NodeElements[(int)DIALOGUE_NODE.SPEAKER_NAME].ToString().Trim();
            
            /*노드 생성 Index ==NodeID*/
            if (""!=strNodeID)
            {
                NowDialogue = new Dialogue();

                if ("" !=SpeakerName)
                    NowDialogue.SpeakerName=SpeakerName;
                NowDialogue.NodeID =NowNodeID=  int.Parse(strNodeID);
               
                /*넣기 전이니까 그냥 사이즈 넣는게 맞음*/
                DailogueDictionary[NowEventName].NodeIDToIndex.Add(NowNodeID, DailogueDictionary[NowEventName].GetNowDicSize()  );
                Debug.Log($"<color=#00ffff>{NowEventName}의 {SpeakerName} 대사 노드 생성</color>");
            }
            /*Dialogue 노드의 List를 채운다.*/
            bool isBranch = false;
            do
            {
                //순서 상관 없음 이넴으로 순서 정해놓음
                bool isNumber = int.TryParse(NodeElements[(int)DIALOGUE_NODE.DIALOGUE_EFFECT_AMOUNT].ToString().Trim(), out int val);
                if (false == isNumber) val =0;
                NowDialogue.EffectAmount.Add(val);

                bool isEffect = Enum.TryParse(typeof(eDialogueEffect),NodeElements[(int)DIALOGUE_NODE.DIALOGUE_EFFECT].ToString().Trim(), out var dialogueEffect);
                if (false ==isEffect) dialogueEffect = eDialogueEffect.DEFAULT;
                else
                {
                    int a = 0;
                }
                NowDialogue.DialogueEffect.Add((eDialogueEffect)dialogueEffect);

                NowDialogue.Scripts.Add(NodeElements[(int)DIALOGUE_NODE.SCRIPT].ToString().Replace("'", ",").Replace("&","\n"));
                NowDialogue.AnimationName.Add(NodeElements[(int)DIALOGUE_NODE.ANIMATION_NAME].ToString().Trim());
                {
                    /*엣지 연결 준비 : 엣지인가? */
                    isBranch = NodeElements[(int)DIALOGUE_NODE.IS_BRANCH].ToString().Trim() == "TRUE";
                }
                if (++i <NodeRows.Length)
                {
                    NodeElements = NodeRows[i].Split(new char[] { ',' });
                }
                else {break;}

            } while (NodeElements[(int)DIALOGUE_NODE.NODE_ID].ToString()=="");

            /*Edge로 INDEX 연결하기*/
            
            NowDialogue.IsBranch =isBranch;

            List<Tuple<int, EdgeData>> EdgeData;
            EdgeDataDictionary[NowEventName].TryGetValue(NowNodeID,out EdgeData);
            if (null ==EdgeData)
            {
                /*LeafNode*/
                ////NowDialogue.NextBranchNodeID.Add(-1);
            }
            else
            {
                /* 인접 리스트에서 연결된 자식 있는지 확인*/
                foreach (var Edge in EdgeData)
                {
                    NowDialogue.NextBranchNodeID.Add(Edge.Item1);
                    NowDialogue.Labels.Add(Edge.Item2.ButtonLabel);
                    if("" != Edge.Item2.myResponse)
                        NowDialogue.ResponseTypes.Add(Edge.Item2.myResponse);

                }
            }
            DailogueDictionary[NowEventName].AddDialogue(NowDialogue);


        }
        return DailogueDictionary; 
    }

}
