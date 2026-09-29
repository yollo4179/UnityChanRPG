using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.U2D;
using UnityEditor;
using System.Net.Http.Headers;
using System.Security;
using UnityEngine.EventSystems;
using Unity.AppUI.UI;

public class InvenGridCellView : DefaultCellView
{



    [SerializeField]
    private GameObject _focusGo;

    private GameObject _invenGridCellViewPrefab;

    private int _itemID;
    bool _hasInitialized = false; 
    public int ItemID { get => _itemID; }
    GameObject[] _itemIconDraggable = new GameObject[2];
    GameObject _itemIcon;//Copyable
    ItemInfo _itemInfo;  public ItemInfo ItemInfo { get=> _itemInfo; }
    public void SetFocusGOActive(bool _bActive)
    {
        _focusGo?.gameObject?.SetActive(_bActive);
    }
   
    public void OnEnable()
    {
      
        _invenGridCellViewPrefab = gameObject;
    }
    public void UpdateIcons(Transform parent, Sprite sprite)
    {
        GameObject[] icon = { _itemIcon , _itemIconDraggable[0],_itemIconDraggable[1]};
        for (int i = 0; i<3; ++i)
        {

            icon[i].GetComponent<CanvasGroup>().blocksRaycasts = true; ;


            var rect = icon[i].GetComponent<RectTransform>();

            // 1) 부모 붙일 때 로컬 기준 보존

            rect.SetParent(parent, worldPositionStays: false);
            icon[i].transform.SetAsLastSibling();
            FixItem(rect, new Vector2(95, 95));

            ItemData itemData = Managers.Data.GetItemData(_itemID);
            //Sprite
            icon[i].GetComponentInChildren<Image>().sprite = sprite;
            //TextMeshProUGUI text = icon[i].GetComponentInChildren<TextMeshProUGUI>();

            ////Text
            //InstanceItemInfo instance = (_itemInfo as InstanceItemInfo);
            //if (eITEMTYPE.EQUIPMENT == itemData.Type /*&&  null != instance*/ )
            //{
            //   if( 1 < instance.CurrentReinforce)
            //    {
            //        text.text = "+ " +instance.CurrentReinforce.ToString();
            //    }
            //   else
            //    {
            //        text.text = "";
            //    }

            //}
            //else
            //{
            //    if (1 < _itemInfo.Amount)
            //        text.text = _itemInfo.Amount.ToString();
            //    else
            //        text.text = "";
            //}
            
            icon[i].GetComponent<ItemDataStorage>().UpdateItemData(itemData, sprite, _itemInfo);
            icon[i].GetComponent<ItemDataStorage>().Init();
        }
    }
    /*처음 생성될 때 한 번 호출*/
    public void Init(int itemID, Sprite sprite,ItemInfo itemInfo )
    {
        _itemInfo  =itemInfo;
        _itemID = itemID;

        if (false ==_hasInitialized)
        {
            AddIcon(out _itemIcon);
            for (int i = 0; i<2; ++i)
            {
                AddIcon(out _itemIconDraggable[i]);//draggable
                _itemIconDraggable[i].AddComponent<UI_Draggable_Move>().SetOriginTransform(_invenGridCellViewPrefab.transform);
            }
            _hasInitialized =true; 
        }
       
         UpdateIcons(_invenGridCellViewPrefab.transform, sprite);
        
    }
    public void AddIcon(out GameObject icon)
    {
        icon =(Managers.Resource.Instantiate("UI/Part/ItemIcon_Prefab", null)); //미니 스롯 생성
    }
}
