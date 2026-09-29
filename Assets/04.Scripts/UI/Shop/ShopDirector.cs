using TMPro;
#if UNITY_EDITOR
using UnityEditor.Search;
#endif
using UnityEngine;
using UnityEngine.UI;

public class ShopDirector : UI_Base
{
    public enum Buttons
    {
        Sell_Button,
        Buy_Button, 
        Close_Button,
        Equipment_Button,
        Consumable_Button
    }
    public enum TMPs
    {
        Advice_TMP
    }

    eTRADERS _shopOwner=eTRADERS.SHOP1;
    //private int ShopID = 0;
   
    Button _btnBuy;
    Button _btnSell;
    Button _btnClose;
    Button _btnEquipment;
    Button _btnComsumable;
    TextMeshProUGUI _textAdvice;  public TextMeshProUGUI TextAdvice { get => _textAdvice; }
    eITEMTYPE CurrentItemType = eITEMTYPE.CONSUMABLE;

    ShopGridScrollView m_ShopGridScrollView;
    
    ItemDetail _itemDetail;
    UI_Popup _itemDetailHandler;
    
    ConfirmTrade _confirmTrade;
    UI_Popup _confirmTradeHandler;
    
    UI_Popup _handler;
    public void SetShopOwner(eTRADERS owner)
    {
        _shopOwner  = owner; 
    }
    public override void Init()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(TMPs)); 
        m_ShopGridScrollView= GetComponentInChildren<ShopGridScrollView>();
        _confirmTrade = Managers.UI.GetCachedUIByName("ConfirmTrade_Canvas_Prefab").GetComponentInChildren<ConfirmTrade>();
        _confirmTradeHandler = Managers.UI.GetCachedUIByName("ConfirmTrade_Canvas_Prefab").GetComponent<UI_Popup>();
        _itemDetail = Managers.UI.GetCachedUIByName("ItemDetailPannel_Canvas_Prefab").GetComponentInChildren<ItemDetail>();
        _itemDetailHandler =  Managers.UI.GetCachedUIByName("ItemDetailPannel_Canvas_Prefab").GetComponent<UI_Popup>(); 
        _btnBuy = Get<Button>((int)Buttons.Buy_Button);
        _btnSell = Get<Button>((int)Buttons.Sell_Button);
        _btnClose = Get<Button>((int)Buttons.Close_Button);
        _btnEquipment = Get<Button>((int)Buttons.Equipment_Button);
        _btnComsumable = Get<Button>((int)Buttons.Consumable_Button);
        _textAdvice = Get<TextMeshProUGUI>((int)TMPs.Advice_TMP);
        _handler = transform.root.GetComponent<UI_Popup>();
    }
    void Awake()
    {
        Init();

        _btnComsumable.onClick.AddListener(() =>
        {
            /*Consumable Item Type Shop Grid View Refresh*/
            CurrentItemType = eITEMTYPE.CONSUMABLE;
            m_ShopGridScrollView.Refresh(CurrentItemType);
        });
        _btnEquipment.onClick.AddListener(() =>
        {
            /*Equipment Item Type Shop Grid View Refresh*/
            CurrentItemType = eITEMTYPE.EQUIPMENT;
            m_ShopGridScrollView.Refresh(CurrentItemType);
        });
        _btnClose.onClick.AddListener(() =>
        {
            /*Shop 닫기*/

            ClearMySelf(); 
        });
        _btnBuy.onClick.AddListener(() =>
        {
            ShopGridCellView nowCellView = m_ShopGridScrollView.CurrentCellView;
            if (null ==nowCellView) { _textAdvice.text="구매할 상품을 선택해주세요."; return; }
            int itemID = nowCellView.ItemID;


            _confirmTrade.OnOpen().Refresh(itemID,nowCellView.ItemInfo ,eTRADEPOLICY.POLICY_BUY); 
            

        });
        _btnSell.onClick.AddListener(() =>
        {
            InvenGridScrollView invenGridScrollView = Managers.UI.GetCachedUIByName("InventoryPannel_Canvas_Prefab").GetComponentInChildren<InvenGridScrollView>();
            InvenGridCellView nowCellView = invenGridScrollView.CurrentFocusCellView;
            if (null ==nowCellView) { _textAdvice.text="판매할 상품을 선택해주세요."; return;}
            int itemID = nowCellView.ItemID;

            ItemInfo itemInfo = nowCellView.ItemInfo;
            ItemData itemData =Managers.Data.GetItemData(itemID);
            if (null ==itemInfo) return; 
            switch(itemInfo.Type)
            {
                case eITEMTYPE.EQUIPMENT:
                    Managers.Inventory.TryRemoveItem(itemInfo,1);
                    Managers.Player.AddMoney(itemData.price);
                    //Todo<Publish>
                    break;

                default:
                    _confirmTrade.Refresh(itemID, itemInfo, eTRADEPOLICY.POLICY_SELL).OnOpen();
                    break; 
            }

            
        });


        Debug.Log($"<color=#00ff00> Shop Info Load Complete</color> :{Managers.Shop.ShopInfo[(int)_shopOwner]}");
        m_ShopGridScrollView.OnFocus = (ID) => {
            _itemDetail.Refresh(ID, eTRADEPOLICY.POLICY_NONE).OnOpen();
        };
        m_ShopGridScrollView.Init();
        m_ShopGridScrollView.SetTrader(_shopOwner);
        m_ShopGridScrollView.Refresh(eITEMTYPE.CONSUMABLE);

       

        /*Close는 인벤에서 SellMode로 */
       
        
        
       
    }
    public void ClearMySelf()
    {
        ShopGridCellView CurInvenCellView = m_ShopGridScrollView?.CurrentCellView;
        if (null!=CurInvenCellView)
        {
            CurInvenCellView.SetFocusGOActive(false);
            m_ShopGridScrollView.CurrentCellView=null;

            
            Managers.UI.ClosePopupUI(_itemDetailHandler);

        }
        Debug.Log($"<color=#00ffff> {m_ShopGridScrollView.CurrentCellView==null}</color>");

        Managers.UI.ClosePopupUI(_handler);

    }


}

    
