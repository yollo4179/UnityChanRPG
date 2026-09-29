using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class DefaultCellView : ChildUIHandler
{
    public override void OnDrag(PointerEventData eventData)
    {
       base.OnDrag(eventData);
        OnClickHandler?.Invoke(eventData);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        if (OnClickHandler != null)
            OnClickHandler.Invoke(eventData);
    }
}
