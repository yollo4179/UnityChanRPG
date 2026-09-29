using NUnit.Framework;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UI_QuickSlot;
using System.Collections.Generic;
public class UI_IconPopupPairScript : UI_Scene
{


    public UI_Popup[] popUpHandle = new UI_Popup[(int)Buttons.END];
   
    public enum Buttons
    {
        Inven_Button=0,
        Equip_Button=1,
        Quest_Button=2,
        Skill_Button=3,
        Setting_Button=4,
        END
    }
    public void Awake()
    {
        
        string[] pathNamePair = new string[(int)Buttons.END]
        {
            "InventoryPannel_Canvas_Prefab",
            "UI_Equipment_Canvas_Prefab", //Equipment
           "Quest_Canvas_Prefab", //Quest
            "SkillBook_Canvas_Prefab",
            null, //Setting
        };

        Bind<Button>(typeof(Buttons)); //Button을 Object에 담는다.

        for(int i = 0; i < (int)Buttons.END; ++i)
        {
            if (null==pathNamePair[i]) continue;
            int idx = i;
            (_objects[typeof(Button)][i] as Button).onClick
               .AddListener(() => {
                   if (null == Managers.UI.GetOpenUIByName(pathNamePair[idx]))
                   {
                       popUpHandle[idx]= Managers.UI.ShowPopupUI<UI_Popup>(pathNamePair[idx]);
                   }
                   else
                   {
                       Managers.UI.ClosePopupUI(popUpHandle[idx]);
                       popUpHandle[idx] =null;
                   }

               }
               );
        }
            //(_objects[typeof(Button)][(int)Buttons.Inven_Button] as Button).onClick
            //    .AddListener(() =>{
            //        if (null==Managers.UI.GetUIByName("SkillBook_Canvas_Prefab"))
            //        {
            //            popUpHandle[(int)Buttons.Skill_Button]= Managers.UI.ShowPopupUI<UI_Popup>("PopUpUI/Inventory/InventoryPannel_Canvas_Prefab");
            //        }
            //        else
            //        {
            //            Managers.UI.ClosePopupUI(popUpHandle[(int)Buttons.Skill_Button]);
            //            popUpHandle[(int)Buttons.Skill_Button] =null;
            //        }
                    
            //    }
            //    );

            //(_objects[typeof(Button)][(int)Buttons.Skill_Button] as Button).onClick
            //    .AddListener(() => {
            //        if (null==Managers.UI.GetUIByName("SkillBook_Canvas_Prefab"))
            //        {
            //            popUpHandle[(int)Buttons.Skill_Button]= Managers.UI.ShowPopupUI<UI_Popup>("PopUpUI/Skills/SkillBook_Canvas_Prefab");
            //        }
            //        else
            //        {
            //            Managers.UI.ClosePopupUI(popUpHandle[(int)Buttons.Skill_Button]);
            //            popUpHandle[(int)Buttons.Skill_Button] =null;
            //        }
            //    }
            //    );
        

    }
 
}
