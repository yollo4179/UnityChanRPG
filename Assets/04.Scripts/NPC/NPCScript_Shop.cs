
using UnityEngine;

public class NPCScript_Shop : NPCScript
{
    //상점 UI 오픈 관련 스크립트를 OVERRIDE 
    //상점 아이템 리스트를 저장할 변수 추가
    //상점 UI와 상호작용 함수 구현

    Poolable mark_ShopHandler;
    Poolable _npcTextHanadler;


    [SerializeField] string _textNPC = "<size=32>Shop(Tab)</size>\n<size=20>Zone Doe</size>";
   


    protected  void OnTriggerStay(Collider other)
    {
        if (false  == other.CompareTag("Player")) return;

        npcAction = null;
        npcAction +=ExcuteNPCAction;
        if (isPlayerInRange== true) return;  
        isPlayerInRange =true; 
        
      

        mark_ShopHandler = Managers.Pool.LendPoolableTo("Mark_Shop", this.transform);
        mark_ShopHandler.transform.localPosition = new Vector3(0, 2.3f, 0);

        _npcTextHanadler = Managers.Pool.LendPoolableTo("NPC_Text", null);
        UI_NPCWorldText textHandler = _npcTextHanadler.GetComponent<UI_NPCWorldText>();
        textHandler.SetText(_textNPC, Color.yellow, new Vector2(300, 200));
        textHandler.SetPosition(this.transform,  new Vector3(0, 2f, 0));

    }
    protected  void OnTriggerExit(Collider other) 
    { 
        
        if (false  == other.CompareTag("Player")) return;

        npcAction = null;
        isPlayerInRange =false;
        if (mark_ShopHandler != null) Managers.Pool.GetBack(mark_ShopHandler);
        mark_ShopHandler=null;

       if(_npcTextHanadler != null) Managers.Pool.GetBack(_npcTextHanadler);
        _npcTextHanadler = null; 
    }
    // Update is called once per frame
    protected  void Update()
    {
        //특정 키를 눌렀을떄 npc 상호작용
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            npcAction?.Invoke();
            
        }
        
    }
    public override void ExcuteNPCAction()
    {

        UI_Popup handle= Managers.UI.ShowPopupUI<UI_Popup>("ShopPannel_Canvas_Prefab");
       
    }
}
