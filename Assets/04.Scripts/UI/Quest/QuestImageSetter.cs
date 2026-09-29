using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class QuestImageSetter : UI_Base
{
    public enum Images
    {
        item_Image
    }
    Image _img;
    bool _hasInitialized = false; 
    public override void Init()
    {
        if (true ==_hasInitialized) return;
        _hasInitialized =true; 
        Bind<Image>(typeof(Images));
        _img = Get<Image>((int)Images.item_Image);
    }
    public void SetImage (ItemData itemData )
    {
        Init();

        eATLAS atlas = eATLAS.ATLAS_ITEMIST_INGREDIENT;
        switch (itemData.Type)
        {
            case eITEMTYPE.INGREDIENT:
                atlas= eATLAS.ATLAS_ITEMIST_INGREDIENT;
                break;
            case eITEMTYPE.CONSUMABLE:
                atlas= eATLAS.ATLAS_ITEMLIST_CONSUMABLE;
                break;
            case eITEMTYPE.EQUIPMENT:
                atlas= eATLAS.ATLAS_ITEMLIST_EQUIPMENT;
                break;
        }
       _img.sprite = AtlasManager.GetInstance().GetSpriteByName(atlas, itemData.SpriteName);
        
    }
}
