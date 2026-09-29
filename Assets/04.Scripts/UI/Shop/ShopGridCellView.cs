
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopGridCellView : DefaultCellView
{
      
  
        [SerializeField]
        private GameObject m_FocusGo;
        [SerializeField]
        private Image m_ItemIcon;
        [SerializeField]
        Button m_BtnFocus;
        
    
        private int m_ItemID;
        public int ItemID { get => m_ItemID; }

        ItemInfo _itemInfo;public ItemInfo ItemInfo { get; } 
        public void SetFocusGOActive(bool _bActive)
        {
            m_FocusGo.gameObject.SetActive(_bActive);
        }


        /*처음 생성될 때 한 번 호출*/
        public void Init(int _ItemID, Sprite _Sprite, int _Amount, ItemInfo itemInfo)
        {

            _itemInfo = itemInfo;
            m_ItemID = _ItemID;
            m_ItemIcon.sprite = _Sprite;
            m_ItemIcon.SetNativeSize();
            m_ItemIcon.rectTransform.sizeDelta = new Vector2(50, 50);
            
            m_ItemIcon.GetComponent<Image>().gameObject.SetActive(null!=_Sprite);


        }

}
