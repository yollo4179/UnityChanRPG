using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Linq.Expressions;




public enum eKEY_CODE
{
   
    JUMP,
    DASH,
    BASE_ATTACK,
    OPEN_INVEN,
    OPEN_SHOP_DEBUG,
    SLOT1,
    SLOT2,
    SLOT3,
    SLOT4,
    SLOT5,
    SLOT6,
    KEYCODE_END,
    
}


public class PlayerControllerCom : IController
{

    public enum PLAYERSTATE
    {
        STATE_DUMMY,
        MOVEMENT,
        JUMP,
        DASH,
        BASE_ATTACK,
        HIT,
        SKILL_MELEE,
        SKILL_RANGED,
        SKILL_BUFF,
        STATE_END,
    }
    public enum ePlayerSkillHandle
    {
        RUSH_ATTACK=7000,
        HOLLY_SWORDS=7001,
        UPPER_SLASH=7002,
        SPIRAL_SLASH=7003,
        ROLLING_ATTACK=7004,
        BUFF=7005,
        DEVIDEEARTH =7006,
    }

    [System.Serializable]public class SkillSet
    {
        [SerializeField] public SkillSO skillSO;
        [SerializeField] public PLAYERSTATE playerState;
        [SerializeField] public bool isState;

    }

    [SerializeField] private InventoryDirector m_InventoryDirector;
    [SerializeField] private ShopDirector m_ShopDirector;
    [SerializeField] private UIDialogue m_DebugUIDialogue;

    [SerializeField] private SkillSet[] _skillSets;
    Dictionary<eKEY_CODE, SkillSet> _dicSlotSkillPair;//소비템도 skillSet으로 관리
    HitBox[] _hitBoxes;
    public HitBox[] GetHitBoxList() { return _hitBoxes; }

    private PlayerMovementCom m_PlayerMovementCom;
    private PlayerAnimatorCom m_PlayerAnimatorCom;

    
    private KeyCode[] m_KeyCode;
    

    void KeyMapping ()
    {
        m_KeyCode = new KeyCode[(int)eKEY_CODE.KEYCODE_END];
        m_KeyCode[(int)eKEY_CODE.JUMP] = KeyCode.Space;
        m_KeyCode[(int)eKEY_CODE.DASH] = KeyCode.LeftShift;
        m_KeyCode[(int)eKEY_CODE.BASE_ATTACK] = KeyCode.Mouse0;

        m_KeyCode[(int)eKEY_CODE.OPEN_INVEN] = KeyCode.I;
        m_KeyCode[(int)eKEY_CODE.OPEN_SHOP_DEBUG] = KeyCode.O;

        m_KeyCode[(int)eKEY_CODE.SLOT1]=KeyCode.Alpha1;
        m_KeyCode[(int)eKEY_CODE.SLOT2]=KeyCode.Alpha2;
        m_KeyCode[(int)eKEY_CODE.SLOT3]=KeyCode.Alpha3;
        m_KeyCode[(int)eKEY_CODE.SLOT4]=KeyCode.Alpha4;
        m_KeyCode[(int)eKEY_CODE.SLOT5]=KeyCode.Alpha5;
        m_KeyCode[(int)eKEY_CODE.SLOT6]=KeyCode.Alpha6;

    }
   public  KeyCode GetKeyCode(eKEY_CODE _Key) { return m_KeyCode[(int)_Key]; }
    /*인풋과 플레이어의 움직임을 담당하자 */
    
    ///*d입력값*/
    //private KeyCode JumpKey = KeyCode.Space;
    //private KeyCode DashKey = KeyCode.LeftShift;
  

    public void Awake()
    {

        

        _dicSkillDescs=new Dictionary<int, SkillSO> ();
        _hitBoxes = GetComponentsInChildren<HitBox> ();

        KeyMapping();
        m_MovementCom = m_PlayerMovementCom= GetComponent<PlayerMovementCom>();
        m_AnimatorCom= m_PlayerAnimatorCom = GetComponentInChildren<PlayerAnimatorCom>();


        CreateStateList((int)PLAYERSTATE.STATE_END);
        /*State 초기화*/
        setState((int)PLAYERSTATE.MOVEMENT, new PlayerMovementState());
        InitializeState(m_StateList[(int)PLAYERSTATE.MOVEMENT]);

        setState((int)PLAYERSTATE.JUMP, new PlayerJumpState());
        InitializeState(m_StateList[(int)PLAYERSTATE.JUMP]);

        setState((int)PLAYERSTATE.DASH, new PlayerDashState());
        InitializeState(m_StateList[(int)PLAYERSTATE.DASH]);

        setState((int)PLAYERSTATE.BASE_ATTACK, new PlayerBaseAttackState());
        InitializeState(m_StateList[(int)PLAYERSTATE.BASE_ATTACK]);
        /*ForSkill*/
        setState((int)PLAYERSTATE.SKILL_MELEE, new PlayerMeleeSkillState());
        InitializeState(m_StateList[(int)PLAYERSTATE.SKILL_MELEE]);

        setState((int)PLAYERSTATE.SKILL_RANGED, new PlayerRangedSkillState());
        InitializeState(m_StateList[(int)PLAYERSTATE.SKILL_RANGED]);

        setState((int)PLAYERSTATE.SKILL_BUFF, new PlayerBuffSkillState());
        InitializeState(m_StateList[(int)PLAYERSTATE.SKILL_BUFF]);
        


        foreach (var skillSet   in _skillSets)
        {
            //상태가 존재할 때만 SkillSO부여
            if(true ==skillSet.isState)
                m_StateList[(int)skillSet.playerState]?.SetSkillSO(skillSet.skillSO);
            //상태가 존재하지 않을때도 부여
            if (false ==skillSet.isState)
                _dicSkillDescs.Add(skillSet.skillSO.handle, skillSet.skillSO);
            
        }


        ChangeState((int)PLAYERSTATE.MOVEMENT);
        /*State 초기화*/

        StatusScript script= GetComponent<StatusScript>();
        script.LoadScriptInfo();
        foreach (var hitBox in _hitBoxes)
            hitBox?.InjectScript(script).SetOwner(this.gameObject);

        //TODO_나중에 데이터 받아서 Equipment에따라 생성되자 마자 장착
        Managers.Equipment.EquipBaseWeapon();

    }

    // Update is called once per frame
    public void Update()
    {

        UpdateState();

        //UpdateMovement();
        // CheckJumpKey();
        // CheckDashKey();
        UIKeyInput();
        UpdateSlotInput();
    }

    public void UIKeyInput()
    {
        //if(m_InventoryDirector&&Input.GetKeyDown(m_KeyCode[(int)eKEY_CODE.OPEN_INVEN]) )
        //{
        //    bool isOpen = m_InventoryDirector.gameObject.activeSelf;
        //    if (isOpen) m_InventoryDirector.ClearMySelf();
        //    m_InventoryDirector.gameObject.SetActive(!isOpen);
        //}

        //if (m_ShopDirector &&Input.GetKeyDown(m_KeyCode[(int)eKEY_CODE.OPEN_SHOP_DEBUG]))
        //{
        //    bool isOpen = m_ShopDirector.gameObject.activeSelf;
        //    if (isOpen) m_ShopDirector.ClearMySelf();
        //    m_ShopDirector.gameObject.SetActive(!isOpen);
           
        //}

        //if (Input.GetKeyDown(KeyCode.F1))
        //{
            
        //    //m_DebugUIDialogue.SetDialogues("미사키의 부탁");
        //}

        //if (Input.GetKeyDown(KeyCode.F2))
        //{
        //    Managers.Event.Publish<Event_KillTarget>(new Event_KillTarget(1000));
        //}
    }
    public void UpdateSlotInput()
    {
        int itemSlot = 0;
        if (Input.GetKeyDown(GetKeyCode(eKEY_CODE.SLOT1)))
            itemSlot =1;
        else if (Input.GetKeyDown(GetKeyCode(eKEY_CODE.SLOT2)))
            itemSlot =2;
        else if (Input.GetKeyDown(GetKeyCode(eKEY_CODE.SLOT3)))
            itemSlot =3;
        else if (Input.GetKeyDown(GetKeyCode(eKEY_CODE.SLOT4)))
            itemSlot =4;
        else if (Input.GetKeyDown(GetKeyCode(eKEY_CODE.SLOT5)))
            itemSlot =5;
        else if (Input.GetKeyDown(GetKeyCode(eKEY_CODE.SLOT6)))
            itemSlot=6;

        if(0>=itemSlot) return;//아무것도 안눌림
        ItemInfo itemInfo = Managers.UI.GetCachedUIByName("HUD_Canvas_Prefab").GetComponentInChildren<UI_DisplayQuickSlots>().GetItemInfoBySlotNo(itemSlot);
        itemInfo?.UseItem();
        
    }
}

