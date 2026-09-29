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
using System.Linq;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class InvenGridScrollView : UI_Base
{
    public enum GameObjects
    {
        Contents,
    }
    public enum TMPs
    {
        TextNoItems,
    }
    private ShopGridScrollView _shopScrollView; //from other

    [SerializeField]
    private Int32 m_Slot;//일단 아이템 개수로 할까? 
    [SerializeField]
    GameObject m_GridCellViewPrefab; //나중에 풀링으로

    Transform _content;
    TextMeshProUGUI _textNoItem;


    List<Poolable> _cellViewHandles = new List<Poolable>();
   


  

    InvenGridCellView m_CurrentFocusCellView;  // 내 포커스 뷰
    public InvenGridCellView CurrentFocusCellView { get => m_CurrentFocusCellView; set => m_CurrentFocusCellView =value; }
    public System.Action<int/*ID*/> OnFocus {get;set;}
    
    /* 이건 Inventory 클래스 하나 파서 분류하는 식으로 만들어보자.*/
    //[SerializedField]
    eITEMTYPE m_NowDisplay= eITEMTYPE.INGREDIENT;

    public override void Init()
    {
        Bind<GameObject>(typeof(GameObjects));
        Bind<TextMeshProUGUI>(typeof(TMPs));
        _content = Get<GameObject>((int)GameObjects.Contents).transform;
        _textNoItem = Get<TextMeshProUGUI>((int)TMPs.TextNoItems);
        _textNoItem.enabled = (0 >= Managers.Inventory.InvenInfo.InfoList.Count);
        MakeGridCellViews();
    }
    public InvenGridScrollView SetFilter(eITEMTYPE type)
    {
        m_NowDisplay = type;
        return this;
    }
   public void SetShopScrollView()
    {
        if(null ==_shopScrollView)
            _shopScrollView = Managers.UI.GetCachedUIByName("ShopPannel_Canvas_Prefab").GetComponentInChildren<ShopGridScrollView>();
    }
    /*GameManager 에서 한번만 호출*/
   
  public void ClearHandles()
    {
        foreach (var handle in _cellViewHandles)
        {
            Managers.Pool.GetBack(handle);
        }
        _cellViewHandles.Clear();
    }
    public void Refresh()
    {
        ClearHandles();


        if (null!=m_CurrentFocusCellView)
        m_CurrentFocusCellView?.SetFocusGOActive(false);
        foreach (Transform Child in _content)
        {
            Destroy(Child.gameObject);//트랜스폼으로 부터 자식의 게임 오브젝트를 지운다.)
        }
        /*Info Manager의 내용을 바탕으로 다시 화면을 갱신한다.*/
        
        MakeGridCellViews();
    }
    
  
    void MakeGridCellViews()
    {

        /*읽기 시도*/

        var ItemInfoList = Managers.Inventory.InvenInfo.InfoList;   
        /*Count 만큼 돈다.*/
        foreach (var info in ItemInfoList)
        {
            if(info.Type != m_NowDisplay)
                continue;

            var retPool = Managers.Pool.LendPoolableTo("InvenGridCellView", null, false);
            retPool.transform.SetParent(_content);
            retPool.transform.localScale = Vector3.one;
            _cellViewHandles.Add(retPool);
            GameObject retGO = retPool.gameObject;  

            var SpriteName = Managers.Data.GetItemData(info.ID).SpriteName;
            Sprite _Sprite=null;
            switch (m_NowDisplay)
            {
                case eITEMTYPE.CONSUMABLE:
                    {
                        _Sprite =AtlasManager.GetInstance().GetSpriteByName(eATLAS.ATLAS_ITEMLIST_CONSUMABLE, SpriteName);
                        
                            break;
                }
                case eITEMTYPE.INGREDIENT: {
                   _Sprite =AtlasManager.GetInstance().GetSpriteByName(eATLAS.ATLAS_ITEMIST_INGREDIENT, SpriteName);
                   break; 
                }
                case eITEMTYPE.EQUIPMENT: {
                   _Sprite =AtlasManager.GetInstance().GetSpriteByName(eATLAS.ATLAS_ITEMLIST_EQUIPMENT, SpriteName);
                   break;
                }

            }
            InvenGridCellView CellView = retGO.GetComponent<InvenGridCellView>();
            CellView.Init(info.ID, _Sprite, info);
            

            System.Action<PointerEventData> MainEvent =
                (x) =>
                {
                    /*x에는 마우스 이벤트 정보움직인 거리 등*/


                    if (m_CurrentFocusCellView)
                        Debug.Log($"<color=#ff0000>선택된 아이템:{Managers.Data.GetItemData(m_CurrentFocusCellView.ItemID).ItemName} </color>");

                   

                    SetShopScrollView();
                    ShopGridCellView CurShopCellView = _shopScrollView?.CurrentCellView;
                    ShopDirector shopDirector= _shopScrollView.GetComponentInParent<ShopDirector>();
                    if (null != shopDirector) shopDirector.TextAdvice.text ="판매 버튼을 눌러주세요";
                    if (null != CurShopCellView)
                    {
                        CurShopCellView?.SetFocusGOActive(false);
                        
                        _shopScrollView.CurrentCellView=null;
                    }

                    if (null == m_CurrentFocusCellView)
                    {
                        m_CurrentFocusCellView = CellView;
                    }
                    else if (null != m_CurrentFocusCellView)
                    {
                        m_CurrentFocusCellView.SetFocusGOActive(false);
                        m_CurrentFocusCellView =CellView;
                    }

                    Debug.Log($"<color=#ff0000>선택된 아이템:{Managers.Data.GetItemData(m_CurrentFocusCellView.ItemID).ItemName} </color>");
                    OnFocus(info.ID);
                    m_CurrentFocusCellView.SetFocusGOActive(true);

                };

            CellView.ClickEvent = MainEvent;
            CellView.DragEvent = MainEvent;


        }

        _textNoItem.enabled = ( 0 >= Managers.Inventory.InvenInfo.InfoList.Where((x) =>(x.Type == m_NowDisplay)).Count() );
    }
}
