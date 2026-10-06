using UnityEngine;
using System.Collections.Generic;
using System.Data;
[System.Serializable]
public class Dialogues
{

    [SerializeField] private  string _dialogeEventKey;
    public string DialogeEventKey { get => _dialogeEventKey; } 
    public Dialogues()
    {
        Init();
    }
    public void AddDialogue(Dialogue _Dialogue)
    {
        m_DialogueList.Add(_Dialogue);
    }
    [Tooltip("Dialogue")]
    public List<Dialogue> m_DialogueList;
    public List<Dialogue> DialogueList { get => m_DialogueList; }
    public Dialogue GetDialogueByNodeID(int NodeID) 
    {
        if (!NodeIDToIndex.TryGetValue(NodeID, out int index))
        {
            return null;
        }
        return m_DialogueList[index];
    } 
    public Dictionary<int, int> NodeIDToIndex;
    public int GetNowDicSize()
    {
        return m_DialogueList.Count;
    }
    void Init() {
        m_DialogueList = new List<Dialogue>();
        NodeIDToIndex= new Dictionary<int, int>();
    }
   
}
