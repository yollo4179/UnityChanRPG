using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using static UnityEngine.Rendering.GPUSort;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using static ItemData;
using System.Text.RegularExpressions;
using UnityEngine.UIElements.Experimental;






/*Inventory련된 일은 Directir가 하기*/
public class InventoryDirector : UI_Base
{
    public enum Buttons
    {
        InvenBtnEquip_Button,
        InvenBtnConsumable_Button,
        InvenBtnIngredient_Button,
        SellButton_Button,
        Close_Button,
        TestItemCreator_Button_Test,
        END
    }

    public enum GameObjects
    {
        InvenGridScrollView
    }
    public enum TMPs
    {
        Money_TMP
    }

    InvenGridScrollView _scrollView;
    Button _generateItemBtnGO;
    Button _sellBtn;
    Button _closeBtn;
    Button _ingredientBtn;
    Button _equipBtn;
    Button _consumableBtn;
    TextMeshProUGUI _textMoney; public TextMeshProUGUI TextMoney{ get => _textMoney; }
    #region 외부 객체 변수 
    ItemDetail m_ItemDetail;
    ConfirmTrade m_ConfirmTrade;
    #endregion

    UI_Popup _handler;
    public void Awake()
    {
        _handler =transform.root.GetComponent<UI_Popup>();
        Init();


    }
    public override void Init()
    {   Bind<Button>(typeof(Buttons));
        Bind<GameObject>(typeof(GameObjects));
        Bind<TextMeshProUGUI>(typeof(TMPs));
        _scrollView = Get<GameObject>((int)GameObjects.InvenGridScrollView).GetComponent<InvenGridScrollView>();
        _closeBtn = Get<Button>((int)Buttons.Close_Button);
        _sellBtn = Get<Button>((int)Buttons.SellButton_Button);
        _generateItemBtnGO = Get<Button>((int)Buttons.TestItemCreator_Button_Test);
        _ingredientBtn = Get<Button>((int)Buttons.InvenBtnIngredient_Button);
        _equipBtn = Get<Button>((int)Buttons.InvenBtnEquip_Button);
        _consumableBtn = Get<Button>((int)Buttons.InvenBtnConsumable_Button);
        _textMoney = Get<TextMeshProUGUI>((int)TMPs.Money_TMP);


        /*외부 변수*/
        m_ItemDetail = Managers.UI.GetCachedUIByName("ItemDetailPannel_Canvas_Prefab").GetComponentInChildren<ItemDetail>();
        m_ConfirmTrade = Managers.UI.GetCachedUIByName("ConfirmTrade_Canvas_Prefab").GetComponentInChildren<ConfirmTrade>();

        _scrollView.Init(); /*Item Load*/
        m_ItemDetail.Init();
        _textMoney.text = Managers.Player.PlayerInfo.Money.ToString();

        _generateItemBtnGO.onClick.AddListener( () =>{
                /*Item 추가 로직*/
            GetRandomItem();}
        );
        _closeBtn.onClick.AddListener(() =>
        {

            ClearMySelf();
        });

       _equipBtn.onClick.AddListener(() =>
        {
            _scrollView.SetFilter(eITEMTYPE.EQUIPMENT).Refresh();
        });
       
        _ingredientBtn.onClick.AddListener(() =>
        {
            _scrollView.SetFilter(eITEMTYPE.INGREDIENT).Refresh();
        });
        _consumableBtn.onClick.AddListener(() =>
        {
            _scrollView.SetFilter(eITEMTYPE.CONSUMABLE).Refresh();
        });

        //_sellBtn.onClick.AddListener(() =>
        //{
        //    /*판매 버튼 클릭시 판매 UI 오픈*/
        //    InvenGridCellView CurCellView = _scrollView?.CurrentFocusCellView;
        //    if (null!= CurCellView)
        //        m_ConfirmTrade?.Refresh(CurCellView.ItemID, eTRADEPOLICY.POLICY_SELL)?.OnOpen();
        //});
        /*인벤토리 아이템 Detail 열고 닫는 기능*/
        _scrollView.OnFocus =(id) =>
        {
            /*Shop이 오픈 했는지 아닌지 확인 ->오픈= buy(샵에서) sell(여기서) /클로즈 = none*/

            m_ItemDetail?.Refresh(id,eTRADEPOLICY.POLICY_SELL).OnOpen();
        };
        
        m_ItemDetail.BtnClose.onClick.AddListener(() =>
        {

            m_ItemDetail?.OnClose();
        });

        /*Item Detail Sell*/
        //m_ItemDetail.BtnSell.onClick.AddListener(()=>
        //{
        //    m_ConfirmTrade?.Refresh(m_ItemDetail.ItemID, eTRADEPOLICY.POLICY_SELL)?.gameObject.SetActive(true);
        //});
        /*판매 구매 로직*/
        //m_ConfirmTrade.BtnTrade.onClick.AddListener(() =>
        //{
        //    string AmountToSell = m_ConfirmTrade.AmountInputString;
        //    int ItemID = m_ConfirmTrade.ItemID;
        //    eTRADEPOLICY TradePolicy= m_ConfirmTrade.TradePolicy;
        //    this.OnTrade(ItemID, AmountToSell, TradePolicy);
        //});

    }
    public void ClearMySelf()
    {
        InvenGridCellView CutInvenCellView = _scrollView?.CurrentFocusCellView;
        if (null!=CutInvenCellView)
        {
            m_ItemDetail?.OnClose();
            _scrollView.CurrentFocusCellView.SetFocusGOActive(false);
            _scrollView.CurrentFocusCellView=null;
        }
        Debug.Log($"<color=#00ffff>InvenGridScrollView.CurrentFocusCellView is null: {_scrollView.CurrentFocusCellView==null}</color>");
       
        Managers.UI.ClosePopupUI(_handler);
    }
    void OnTrade(int _itemID, string _amountToTrade ,eTRADEPOLICY _TradePolicy)
    {
        /*나중에 트레이드 매니저나 트레이드 서비스로 기능 옮기기*/
        {
            //common
            var itemData = Managers.Data.GetItemData(_itemID);
            int Price = itemData.price;

            var itemInfo = Managers.Inventory.InvenInfo.InfoList.Find((X) => (X.ID ==_itemID));
            bool isInInven = (null !=itemInfo);

            

            bool isInt =  int.TryParse(_amountToTrade, out int tradeAmount);
            if (isInt ==false) { ClearMySelf(); return; }

            int totalPrice = tradeAmount*Price;
            int nowAmount = 0;
            if (isInInven)
            {
                nowAmount  = itemInfo.Amount;
            }


            switch(_TradePolicy)
            {
                case eTRADEPOLICY.POLICY_SELL:
                    itemInfo.Amount -= tradeAmount;
                    if (itemInfo.Amount<=0)
                    {
                        Managers.Inventory.InvenInfo.InfoList.Remove(itemInfo);
                    }
                    Debug.Log($"<color=#00ff00>판매 완료! 판매한 아이템 : {itemData.ItemName} , 판매 수량 : {tradeAmount} , 판매 금액 : {totalPrice}</color>");

                    break;

                case eTRADEPOLICY.POLICY_BUY:
                     eITEMTYPE itemType = itemData.Type;
                    if (0==nowAmount)
                    {
                        itemInfo = new ItemInfo();
                        itemInfo.ID = _itemID; itemInfo.Type = itemType; itemInfo.Amount = tradeAmount;
                        Managers.Inventory.InvenInfo.InfoList.Add(itemInfo);
                        break;
                    }
                    ItemInfo foundItem = Managers.Inventory.InvenInfo.InfoList.Find((item) => (item.ID==_itemID));
                    foundItem.Amount+=tradeAmount;
                    break;
            }
            Managers.Inventory.SaveInventoryInfo();
           
        }
        _scrollView.Refresh();
        m_ConfirmTrade.OnClose();

    }


    public void GetRandomItem()
    {

        var ItemDataGO = Managers.Data.GetRandomIngredientData();

        int _id = ItemDataGO.ItemID;
        eITEMTYPE _Type= ItemDataGO.Type;
        /*내가 가지고 있는 아이템인지 검사*/
        var InfoDataGO = Managers.Inventory.InvenInfo.InfoList.Find((x) => (x.ID == _id));

        Debug.Log($"<color=yellow>생성 전{ItemDataGO?.ItemName}:{InfoDataGO?.Amount} </color>");
        if (null == InfoDataGO)
        {
            /*새로운 아이템을 얻는다 .  */
            ItemInfo newItem = new ItemInfo();
            newItem.ID = _id;
            newItem.Type = _Type;
            newItem.Amount =1; 
            Managers.Inventory.InvenInfo.InfoList.Add(newItem);
        }
        else
        {
           
            ++InfoDataGO.Amount; //참조니까 따로 추가할 필요 없다 .  
        }

        Managers.Inventory.SaveInventoryInfo();
        Debug.Log($"<color=yellow>생성 후{ItemDataGO.ItemName}:{InfoDataGO.Amount} </color>");
        _scrollView.Refresh();
    }
}
