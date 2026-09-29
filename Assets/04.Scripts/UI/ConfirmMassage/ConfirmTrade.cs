using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System;
using Item;
using Unity.VisualScripting;

public class ConfirmTrade : UI_Base
{
    public enum TMPs
    {
        AskingContent_TMP,
        TextTrade_TMP,

    }
    public enum TMPInputs
    {
        Amount_InputField,
    }
    public enum Buttons
    {
        TradeButton_Button,
        CancelButton_Button
    }

    public string AmountInputString { get => _amountInputField.text; }
    [SerializeField] public string _strBuy;
    [SerializeField] public string _strSell;
    [SerializeField] public string _strAskingToBuy;
    [SerializeField] public string _strAskingToSell;
    [SerializeField] public string _strDenyToBuy;
    [SerializeField] public string _strError;
    private TextMeshProUGUI _textAskingContent;
    private TextMeshProUGUI _textTrade;
    private TMP_InputField _amountInputField;
    bool _isInputNumber;
    int _resultInt;
    bool _allowTobuy;
    bool _allowTosell;

    private ItemInfo _tradeItem; public ItemInfo TradeItem { get => _tradeItem;}
    private Button _cancelButton;  public Button BtnCancel{ get => _cancelButton; }
    private Button _tradeButton; public Button BtnTrade{ get=> _tradeButton; }
    private int _itemID; public int ItemID { get => _itemID; }
    eTRADEPOLICY _tradePolicy = eTRADEPOLICY.POLICY_NONE; public eTRADEPOLICY TradePolicy { get => _tradePolicy; }
    UI_Popup _handler;

    public override void Init()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(TMPs));
        Bind<TMP_InputField>(typeof(TMPInputs));

        _cancelButton = Get<Button>((int)Buttons.CancelButton_Button);
        _tradeButton =  Get<Button>((int)Buttons.TradeButton_Button);

        //Tmp 
        _textAskingContent = Get<TextMeshProUGUI>((int)TMPs.AskingContent_TMP);
        _textTrade  = Get<TextMeshProUGUI>((int)TMPs.TextTrade_TMP);
        //TMP_ Input
        _amountInputField = Get<TMP_InputField>((int)TMPInputs.Amount_InputField);

        //낱 택스트 바뀌면 판별해줘
        _amountInputField.onValueChanged.AddListener(
            (AmountInputString) =>
            {
                _isInputNumber = int.TryParse(AmountInputString, out _resultInt);
                //1 . 숫자인지 확인
                if (false == _isInputNumber)
                {
                    //1-1. 숫자가 아님 ->x
                    _textAskingContent.text = _strError;
                    return;
                }

                switch (_tradePolicy)
                {
                    //1-2. 숫자임
                    case eTRADEPOLICY.POLICY_BUY:

                        //2-1. 사는 경우 허용할 수 있는 수치인지 확인한다.
                        int price = Managers.Data.GetItemData(_itemID).price;
                        int money = Managers.Player.PlayerInfo.Money;

                        if (0 >= _resultInt || money < _resultInt*price)
                        {
                            _textAskingContent.text = _strDenyToBuy;//거래할 수 없는 금액입니다.
                            _allowTobuy =false;
                            return;
                        }
                        _allowTobuy =true;//final
                        _textAskingContent.text =_strAskingToBuy;
                        _textTrade.text =_strBuy;
                        break;
                    case eTRADEPOLICY.POLICY_SELL:
                        //2-2. 파는 경우 음수가 아닌지(팔 수있는 수치인지 확인한다.). 
                        if (0>_resultInt)
                        {
                            _allowTosell = false;
                            return; 
                        }
                        _allowTosell=true;
                        _textAskingContent.text = _strAskingToSell;
                        _textTrade.text=_strSell;
                        break;
                    default:
                        _textAskingContent.text = "Error";
                        _textTrade.text= "Error";
                        break;
                }
                
            }

        ); 

        //For Events

        _cancelButton.onClick.AddListener(() => { OnClose(); });
        _tradeButton.onClick.AddListener(() =>
        {
            eITEMTYPE itemType = Managers.Data.GetItemData(_itemID).Type;
            if (_isInputNumber)
            {
                int money = _resultInt*Managers.Data.GetItemData(_itemID).price;  
                switch (_tradePolicy)
                {
                    case eTRADEPOLICY.POLICY_BUY:
                        if (false ==_allowTobuy) return; 
                        Managers.Inventory.TryAddItem(itemType, _itemID, _resultInt);
                        Managers.Player.AddMoney(-money);
                        break;
                    case eTRADEPOLICY.POLICY_SELL:
                        if(false ==_allowTosell) return;
                        switch (itemType)
                        {
                            case eITEMTYPE.EQUIPMENT:
                                Managers.Inventory.TryRemoveItem(TradeItem,1);
                                break;
                            default:
                                Managers.Inventory.TryRemoveItem(TradeItem, _resultInt);
                                break;
                        }
                        Managers.Player.AddMoney(money);
                        break;
                    default:
                        break;
                }
            }
        });
        
    }
    


    public ConfirmTrade Refresh(int itemID , ItemInfo tradeItem, eTRADEPOLICY policy= eTRADEPOLICY.POLICY_NONE)
    {
        _tradeItem= tradeItem; 

        _tradePolicy  =policy; 
        _itemID =itemID;
        var nowItemData = Managers.Data.GetItemData(_itemID);
        var NowItemInfo =Managers.Inventory.InvenInfo.InfoList.Find((X)=>(X.ID ==_itemID));


        switch(_tradePolicy)
        {
            case eTRADEPOLICY.POLICY_NONE:
                _textAskingContent.text ="ForWhat" ;
                _textTrade.text = "For What";
                break;
            case eTRADEPOLICY.POLICY_SELL:
                _textAskingContent.text ="거래량을 입력해주세요.";
                _textTrade.text = _strSell;
                break;
            case eTRADEPOLICY.POLICY_BUY:
                _textAskingContent.text = "거래량을 입력해주세요.";
                _textTrade.text = _strBuy;
                break;
        }

        return this;
    }
    /*Init*/
    private void Awake()
    {
        _handler = transform.root.GetComponent<UI_Popup>();
        Init();
    }
    public ConfirmTrade OnClose()
    {
       
        Managers.UI.ClosePopupUI(_handler);
        return this;
    }
    public ConfirmTrade OnOpen()
    {

        _amountInputField.text= "";
        _textAskingContent.text ="거래량을 입력해주세요.";
        Managers.UI.ShowPopupUI<UI_Popup>("ConfirmTrade_Canvas_Prefab");
        return this;
    }
}
