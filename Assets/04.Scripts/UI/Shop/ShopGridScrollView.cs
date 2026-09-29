using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShopGridScrollView : UI_Base
{
    public enum GameObejcts
    {
        Contents
    }


    InvenGridScrollView _invenScrollView; 


    //[SerializeField]
    Transform _contents; //Contents
   
    GameObject m_ShopCellViewPrefab; //너도 풀링으로 하자.

    eTRADERS m_TraderType = eTRADERS.SHOP1; 
    eITEMTYPE _nowItemType = eITEMTYPE.CONSUMABLE;

    ShopGridCellView _nowCellView; public ShopGridCellView CurrentCellView { get => _nowCellView; set => _nowCellView= value; }

    List<Poolable> _cellViewHandles = new List<Poolable>();
    /*SHOP Director 에서 등록*/
    public System.Action<int> OnFocus{ set; get; }
    public System.Action OnOpen { set; get; } 
    public System.Action OnClose { set; get;  }
   
    public override void Init()
    {
        Bind<GameObject>(typeof(GameObejcts));
        _contents =Get<GameObject>((int)GameObejcts.Contents).transform;
       
    }
    public void SetTrader(eTRADERS Trader = eTRADERS.SHOP1) {
       
        m_TraderType = Trader;
       
    }
    public void SetInvenInfo()
    {
        if (null==_invenScrollView)
            _invenScrollView = Managers.UI
               .GetCachedUIByName("InventoryPannel_Canvas_Prefab")
               .GetComponentInChildren<InvenGridScrollView>();
    }
 
    /*Refresh도 필요 없을 듯 (아이템이 교체되나?)*/
    public void Refresh(eITEMTYPE nowItemType)
    {
        _nowCellView?.SetFocusGOActive(false);
        Clear();
        _nowItemType = nowItemType;

        MakeCellViews();

    }
    public void Clear()
    {
        foreach (var item in _cellViewHandles) {
            Managers.Pool.GetBack(item);
        }
        _cellViewHandles.Clear();
    }
    public void MakeCellViews()
    {
        var ShopItemInfoList = Managers.Shop.ShopInfo[(int)m_TraderType].InfoList;
        foreach(var info in ShopItemInfoList)
        {
            /*ShopData를 가져온다.*/
            if(info.Type != _nowItemType) continue;


            var ItemData = Managers.Data.GetItemData(info.ID);
            var retPool = Managers.Pool.LendPoolableTo("ShopGridCellView", null,false);
            
            retPool.transform.SetParent(_contents);
            retPool.transform.localScale = Vector3.one;
            _cellViewHandles.Add(retPool);
            /*Sprite*/

            var SpriteName= AtlasManager.Instance.GetSpriteByName(ItemRelationManager.GetInstance().GetItemEnumGroup(info.Type).Atlas, ItemData.SpriteName);

            ShopGridCellView GridCellView= retPool.GetComponent<ShopGridCellView>();
            GridCellView.Init(info.ID, SpriteName, 1,info);

            /*Focus Registration*/

            System.Action<PointerEventData> FocusEvent =  (X) =>
                {
                    SetInvenInfo();
                    InvenGridCellView CurInvenCellView = _invenScrollView?.CurrentFocusCellView;
                    ShopDirector shopDirector = GetComponentInParent<ShopDirector>();
                    shopDirector.TextAdvice.text ="구매 버튼을 눌러주세요";

                    if (null != CurInvenCellView)
                    {
                        CurInvenCellView.SetFocusGOActive(false);
                        CurInvenCellView= null; 
                    }
                   

                    if (null != _nowCellView)
                    {
                        _nowCellView.SetFocusGOActive(false);
                    }
                    _nowCellView = GridCellView;
                    _nowCellView.SetFocusGOActive(true);
                    OnFocus(info.ID);
                };
            GridCellView.ClickEvent += FocusEvent;
            GridCellView.DragEvent +=FocusEvent; //리프레시 하면 안에서 새로운거 더해주고 이전거에서 빼주거나 (Refresh)

        }

    }


}
