using System;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class UI_EventHandler : MonoBehaviour, IPointerClickHandler,IDragHandler
{ 
    public Action<PointerEventData> OnClickHandler = null;
    public Action<PointerEventData> OnDragHandler = null;
    public System.Action<PointerEventData> ClickEvent { set => OnClickHandler = value; get => OnClickHandler; }
    public System.Action<PointerEventData> DragEvent { set => OnDragHandler = value; get => OnDragHandler; }

    public void OnDrag(PointerEventData eventData)
    {
        OnClickHandler?.Invoke(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (OnClickHandler != null)
            OnClickHandler.Invoke(eventData);
    }


}

