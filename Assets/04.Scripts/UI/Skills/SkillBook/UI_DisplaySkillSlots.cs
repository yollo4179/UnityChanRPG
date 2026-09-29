using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.AppUI.UI;
public class UI_DisplaySkillSlots : UI_Scene
{
    enum GameObjects
    {
        Contents_GO,
    } 
    GameObject _content;



    private void Start()
    {
        //1 바인딩 한다. 
        Bind<GameObject>(typeof(GameObjects)); //나머진 컴포넌트 자동화

        GameObject player = GameObject.Find("Player");
        PlayerControllerCom playerController= player.GetComponent<PlayerControllerCom>();


        //var k = _objects[typeof(UnityEngine.GameObject)];//[(int)GameObjects.Contents_GO];
        var enums = Enum.GetValues(typeof(PlayerControllerCom.ePlayerSkillHandle));
        foreach (var handle in enums)
        {
            var playerSkillSO = playerController.GetSkillSOByHandle((int)handle);
            
           var pannel  = Managers.Resource.Instantiate("UI/PopUpUI/Skills/SkillSlot_Prefab", (_objects[typeof(GameObject)][(int)GameObjects.Contents_GO]as GameObject).transform);
           pannel.GetComponent<UI_SkillSlot>().Injectinfo(playerSkillSO);

           
        }
        
    }

}
