using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetail : MonoBehaviour
{
    [SerializeField]
    Button m_Sell;
    public Button BtnSell { get => m_Sell; }
    [SerializeField]
    Button m_Action;
    public Button BtnAction { get => m_Action; }
    [SerializeField]
    Button m_Close;
    public Button BtnClose { get => m_Close; }

    [SerializeField]
    Image m_Image;
    [SerializeField]
    TextMeshProUGUI m_ItemNameTextMeshPro;
    [SerializeField]
    TextMeshProUGUI m_ItemTypeTextPro;
    [SerializeField]
    TextMeshProUGUI m_ItemExplanationTextPro;

    [SerializeField]
    TextMeshProUGUI m_PriceTextPro;

    UI_Popup _handler; 

    //eTRADEPOLICY m_TradePolicy = eTRADEPOLICY.POLICY_NONE;
    public eTRADEPOLICY TradePolicy{ get; }

    int m_ItemID;
    public int ItemID{ get => m_ItemID; }
    /*Debug*/
    int Amount;

    public void Init()
    {
         
        m_Action.onClick.AddListener(OnClickAction);
   
    }
   
    public ItemDetail Refresh(int _Itemid, eTRADEPOLICY Policy = eTRADEPOLICY.POLICY_NONE) {



        var ItemData=  Managers.Data.GetItemData(_Itemid);
        if(null ==ItemData)
        {
            Debug.Log($"<color=#ff0000> {_Itemid}번 아이디를 가진 아이템은 등록되지 않았습니다. </color>");
            return null;
        }
        m_ItemID = _Itemid;
        eITEMTYPE ItemType = ItemData.Type;
        string SpriteName = ItemData.SpriteName;
        m_Image.sprite = AtlasManager.GetInstance().GetSpriteByName(ItemRelationManager.GetInstance().GetItemEnumGroup(ItemType).Atlas ,  SpriteName);
        m_Image.SetNativeSize();
        m_Image.rectTransform.sizeDelta = new Vector2(64, 64);
        m_ItemNameTextMeshPro.text = ItemData.ItemName.ToString();
        m_ItemTypeTextPro.text = ItemType.ToString();
        m_ItemExplanationTextPro.text = ItemData.Explanation;
        m_PriceTextPro.text= "Price: "+ItemData.price.ToString();

        switch (Policy)
        {
            case eTRADEPOLICY.POLICY_NONE:
                BtnSell.gameObject.SetActive(false);
                BtnAction.gameObject.SetActive(true);
                break;
            case eTRADEPOLICY.POLICY_BUY:

                BtnSell.gameObject.SetActive(false);
                BtnAction.gameObject.SetActive(false);
                break;
            case eTRADEPOLICY.POLICY_SELL:
                BtnSell.gameObject.SetActive(true);
                BtnAction.gameObject.SetActive(true);
                break;
        }


        return this;

    }

    public void OnClickAction() { }
    public void OnClose() {

        //this.gameObject.SetActive(false);
        Managers.UI.ClosePopupUI(_handler);
    }
    public void OnOpen()
    {
        _handler =Managers.UI.ShowPopupUI<UI_Popup>("ItemDetailPannel_Canvas_Prefab"); //나 열어줘 (this)

        //this.gameObject.SetActive(true);
        
    }
}
