using System;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//<sumary>
//Advenced ScrollRect
//</sumary>

public class ChildUIHandler : UI_Base,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IInitializePotentialDragHandler, IPointerClickHandler
{
    /*For Event Handling*/
    public Action<PointerEventData> OnClickHandler = null;
    public Action<PointerEventData> OnDragHandler = null;
    public System.Action<PointerEventData> ClickEvent { set => OnClickHandler = value; get => OnClickHandler; }
    public System.Action<PointerEventData> DragEvent { set => OnDragHandler = value; get => OnDragHandler; }


    [SerializeField] private ScrollRect parent;
    public override void Init()
    {

    }
    void Awake()
    {
        if (!parent) parent = GetComponentInParent<ScrollRect>();
    }

    public void OnInitializePotentialDrag(PointerEventData e)
    {
        // 부모 ScrollRect가 드래그 모멘텀 초기화할 수 있게 전달
        if (parent) parent.OnInitializePotentialDrag(e);
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (parent) parent.OnBeginDrag(e);
    }

    public virtual void OnDrag(PointerEventData e)
    {
        if (parent) parent.OnDrag(e);
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (parent) parent.OnEndDrag(e);
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        
    }
}



