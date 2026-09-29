using UnityEngine;

public class NPCScriptBlackSmith : NPCScript
{
    Poolable mark_ShopHandler;







    protected void OnTriggerStay(Collider other)
    {
        if (false  == other.CompareTag("Player")) return;

        npcAction = null;
        npcAction +=ExcuteNPCAction;
        



       

        if (isPlayerInRange== true) return;
        isPlayerInRange =true;

        mark_ShopHandler = Managers.Pool.LendPoolableTo("Mark_Reinforce", this.transform);
        mark_ShopHandler.transform.localPosition = new Vector3(0, 2.3f, 0);
        ShowupNameMark();

    }
    protected void OnTriggerExit(Collider other)
    {

        if (false  == other.CompareTag("Player")) return;

        npcAction = null;
        isPlayerInRange =false;
       


        if (false  == other.CompareTag("Player")) return;
        isPlayerInRange =false;

        if (mark_ShopHandler != null) Managers.Pool.GetBack(mark_ShopHandler);
        mark_ShopHandler=null;
        if (_nameTextHandler != null) Managers.Pool.GetBack(_nameTextHandler);
        _nameTextHandler = null;
    }
    // Update is called once per frame
    protected void Update()
    {
        //특정 키를 눌렀을떄 npc 상호작용
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            npcAction?.Invoke();

        }

    }
    public override void ExcuteNPCAction()
    {

        UI_Popup handle = Managers.UI.ShowPopupUI<UI_Popup>("Reinforce_Canvas_Prefab");

    }
}
