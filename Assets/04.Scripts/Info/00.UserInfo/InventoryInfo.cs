using UnityEngine;
using System.Collections.Generic; 
public class InventoryInfo
{

    public List<ItemInfo> InfoList; 
    
    //신규 유저일 경우 한번 호출  
    public void Init ()
    {
        InfoList= new List<ItemInfo>(); 

        
    }
    /*TODO JSON Item 불러와서 역직렬화*/

}


/*Drag AND DROP - Info 정보 바꾸기 로드 할때 바뀐 정보 그대로 전달 ITEMpANNEL에 OnEnter 함수 포함-> 정보 계속 채우기 */