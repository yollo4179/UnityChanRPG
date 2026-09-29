using System;
using UnityEngine;

using System.Collections.Generic;
public class NPCScript : MonoBehaviour
{
    [SerializeField] public int NPC_ID;
    
    protected  Action npcAction;
    protected bool isPlayerInRange = false;
    [SerializeField,TextArea(10,5)] public string   _name;
    [SerializeField] public Poolable _nameTextHandler;
    

    public void ShowupNameMark()
    {
        _nameTextHandler = Managers.Pool.LendPoolableTo("NPC_Text", null);
        UI_NPCWorldText textHandler = _nameTextHandler.GetComponent<UI_NPCWorldText>();
        textHandler.SetText(_name, Color.yellow, new Vector2(300, 200));
        textHandler.SetPosition(this.transform, new Vector3(0, 2f, 0));
    }

    public virtual void ExcuteNPCAction() 
    {
        
    } 


    protected virtual void ExecuteDialogue()
    {
        Debug.Log("Executing dialogue with NPC.");
    }
    protected virtual void StartQuest()
    {
        Debug.Log("Starting quest from NPC.");
    }
    protected virtual void GiveItem()
    {
        Debug.Log("NPC is giving an item.");
    }
    protected virtual void ShowEmote()
    {
        Debug.Log("NPC is showing an emote.");
    }

    protected virtual void PopupUI()
    {
        Debug.Log("Displaying NPC popup UI.");
    }
}
