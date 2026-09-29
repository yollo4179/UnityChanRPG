using System.Threading;
using Unity.AppUI.UI;
using UnityEngine;

public class SceneGamePlay : SceneBase
{
    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.GamePlayScene;
        // Managers.UI.ShowScene<UI_Inven>();

        //프리팹 생성 등 ...
        InitUIs();
        InitSceneEffects();
        InitMarkUps();

        
    }
    protected override void InitSceneEffects()
    {
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Wolf/Wolf_GreenHit"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Wolf/Wolf_HealingCircle"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Wolf/Wolf_StoneSlash"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Monster/Minotaur/Slash_Row_Red"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Monster/Minotaur/Sparks_explode_red"));

        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Monster/Alien/Lightning_Hit_Blue"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Monster/Alien/Lightning_aura"));


        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Monster/Smaug/SmaugBiteEffect"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Monster/Smaug/SmaugBreath"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Monster/Smaug/SmaugMeteor"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Monster/Smaug/SmaugTornado"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/Effects/Monster/Smaug/Sparks_explode_yellow"));


    }
    protected void InitMarkUps()
    {
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/PopupMarks/Mark_Completed"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/PopupMarks/Mark_Quest"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/PopupMarks/Mark_Reinforce"));
        Managers.Pool.CreatePool(Managers.Resource.Load<GameObject>("Prefabs/PopupMarks/Mark_Shop"));
    }
    protected override void InitUIs() {

        

        //일단 생성하고,
        var nowWorldUI = Managers.UI.ShowWorldUI<UI_World>("WorldUI/UI_SSTargetHPBars(Canvas)");//인스턴스화 하고 받아온다.
        nowWorldUI.Init();
        /*데미지 폰트 캔버스 (hud)*/
        nowWorldUI = Managers.UI.ShowWorldUI<UI_World>("WorldUI/UI_SSDamageFonts_Canvas");
        nowWorldUI.Init();




        /*Skills and QuickSlots*/
        var nowScemeUI = Managers.UI.ShowSceneUI<UI_Scene>("SceneUI/HUD_Canvas_Prefab");

        nowScemeUI = Managers.UI.ShowSceneUI<UI_Scene>("SceneUI/Draggable_Canvas");
        
        {
           
            /*For Icon Popup*/
            var nowPopup = Managers.Resource.Instantiate("UI/PopUpUI/Skills/SkillBook_Canvas_Prefab",null,true,1);
            Managers.Resource.Destroy(nowPopup);
            Managers.UI.RegisterCachedUI("SkillBook_Canvas_Prefab", nowPopup);

            {
                nowPopup = Managers.Resource.Instantiate("UI/PopUpUI/ItemDetail/ItemDetailPannel_Canvas_Prefab", null, true, 1);
                Managers.Resource.Destroy(nowPopup);
                Managers.UI.RegisterCachedUI("ItemDetailPannel_Canvas_Prefab", nowPopup);

                nowPopup = Managers.Resource.Instantiate("UI/PopUpUI/Trade/ConfirmTrade_Canvas_Prefab", null, true, 1);
                Managers.Resource.Destroy(nowPopup);
                Managers.UI.RegisterCachedUI("ConfirmTrade_Canvas_Prefab", nowPopup);
            }
            nowPopup = Managers.Resource.Instantiate("UI/PopUpUI/Equipment/UI_Equipment_Canvas_Prefab", null, true, 1);
            Managers.Resource.Destroy(nowPopup);
            Managers.UI.RegisterCachedUI("UI_Equipment_Canvas_Prefab", nowPopup);

            {
                
                nowPopup = Managers.Resource.Instantiate("UI/PopUpUI/Inventory/InvenGridCellView", null, false);

                nowPopup = Managers.Resource.Instantiate("UI/PopUpUI/Inventory/InventoryPannel_Canvas_Prefab", null, true, 1);
                Managers.Resource.Destroy(nowPopup);
                Managers.UI.RegisterCachedUI("InventoryPannel_Canvas_Prefab", nowPopup);
            }

                {//Shop
                //CellView
                nowPopup = Managers.Resource.Instantiate("UI/PopUpUI/Shop/ShopGridCellView",null,false);//풀링이니까 sCENEE하위에

                nowPopup = Managers.Resource.Instantiate("UI/PopUpUI/Shop/ShopPannel_Canvas_Prefab", null, true, 1);
                Managers.Resource.Destroy(nowPopup);
                Managers.UI.RegisterCachedUI("ShopPannel_Canvas_Prefab", nowPopup);
            }

            

           

            nowPopup = Managers.Resource.Instantiate("UI/PopUpUI/Dialogues/DialogueUI_Canvas_Prefab", null, true, 1);
            Managers.Resource.Destroy(nowPopup);
            Managers.UI.RegisterCachedUI("DialogueUI_Canvas_Prefab", nowPopup);


            /*QUEST*/
            nowPopup = Managers.Resource.Instantiate("UI/PopUpUI/Quest/TaskProcess_TMP", null, false);
            nowPopup = Managers.Resource.Instantiate("UI/PopUpUI/Quest/QusetTitle_Button", null, false);
            nowPopup = Managers.Resource.Instantiate("UI/PopUpUI/Quest/QuestItem_Slot", null, false);
            nowPopup = Managers.Resource.Instantiate("UI/PopUpUI/Quest/Quest_Canvas_Prefab", null, true, 1);
            Managers.Resource.Destroy(nowPopup);
            Managers.UI.RegisterCachedUI("Quest_Canvas_Prefab", nowPopup);
            /*Quest Mini*/
            nowPopup = Managers.Resource.Instantiate("UI/SceneUI/QuestMini/TaskMini_TMP", null, false);
            nowPopup = Managers.Resource.Instantiate("UI/SceneUI/QuestMini/QuestMini_TMP", null, false);

            /*Reinforce*/
            nowPopup = Managers.Resource.Instantiate("UI/PopUpUI/Reinforce/Reinforce_Canvas_Prefab", null, true, 1);
            Managers.Resource.Destroy(nowPopup);
            Managers.UI.RegisterCachedUI("Reinforce_Canvas_Prefab", nowPopup);
        }
       
       


        //nowPopupUI = Managers.UI.ShowPopupUI<UI_Popup>("PopUpUI/Dialogues/DialogueUI_Canvas_Prefab");






    }
    public override void Clear() { }
}
