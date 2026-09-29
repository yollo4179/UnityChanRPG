using UnityEngine;
using System.Collections.Generic;

public enum LEAFNODE_TYPE
{
    NON_LEAF,
    LEAF_DIALOGUE_END,
    LEAF_ONLY_ACCEPT,
    END

}

enum DIALOGUE_RESULT
{
    DEFAULT,
    ACCEPT,
    TURN_DOWN,
    NEXT,
    SELECT01,
    SELECT02,
    SELECT03,
    SELECT04,
    SELECT05,
    DIALOGUE_END,

}
[System.Serializable]
public class Dialogue
{

   public  Dialogue()
    {
        Scripts= new List<string>();
        AnimationName = new List<string>();
        IsBranch =false;
        NextBranchNodeID = new List<int>();
        Labels = new List<string>();
        ResponseTypes = new List<string>();
        DialogueEffect = new List<eDialogueEffect>();
        EffectAmount = new List<int>();
    }
    [Tooltip("노드 ID")]
    public int NodeID;
    [Tooltip("화자의 이름")]
    public string SpeakerName;


    /*재생 완료 후 브랜치 선택에따라서 재귀*/
    [Tooltip("대사, 끊어말할 수있다.")]  
    public List<string> Scripts;
    [Tooltip("대사 진행 시 재생할 애니메이션")]
    public List<string> AnimationName;
    [Tooltip("분기를 나눌 대사의 시점")]
    public bool IsBranch;
    [Tooltip("다음 브렌치의 인덱스")]
    public List<int> NextBranchNodeID;

    [Tooltip("버튼 라벨")]
    public List<string> Labels;

    public List<string> ResponseTypes;

    public List<eDialogueEffect> DialogueEffect;
    public List<int> EffectAmount;
    /*인덱스 카운팅*/
}
