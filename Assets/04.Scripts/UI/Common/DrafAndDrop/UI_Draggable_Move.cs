
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
#endif
public class UI_Draggable_Move : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerDownHandler
 {
    private Transform _originTransform;//내 원래 부모 

    private Transform canvas; //UI가 소속되어있는 최상단의 Canvas Transfiorm
    private Transform previousTransform; //해당 오브젝트가 직전에 소속되어있던 부모 Transform
    private RectTransform rect; //ui위치 제어를 위한 RectTransform 
    private CanvasGroup canvasGroup;//UI 알파 , 상호작용 제어 

    private RectTransform _dragPlane;
    private Camera _dragCamera;
    private Vector3 _pointerOffset;

    [SerializeField] bool _isPopupUI = false;

    bool _isPrevOriginal = true; public bool IsPrevOriginal {  get { return _isPrevOriginal; }set { _isPrevOriginal=value; } }
   public Transform PreviousTransform { get { return previousTransform; } set { previousTransform=value; } }

   public Action DropEvent=null;  
    public Transform OriginTransform { get { return _originTransform; } }

    public void SetDraggable(UI_Draggable_Move oth)
    {
        IsPrevOriginal = oth.IsPrevOriginal;
        previousTransform = oth.PreviousTransform;
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        _isPopupUI =false; 

    }
    public void Awake()
    {
        rect = GetComponent<RectTransform >();
        canvasGroup = GetComponent<CanvasGroup>();

        if (true ==_isPopupUI)
            _originTransform= transform.root;//Canvas
    }
    public void SetOriginTransform(Transform originTransform)
    {
        PreviousTransform =_originTransform =originTransform;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            Managers.UI.BringPopupToFront(GetComponentInParent<UI_Popup>());
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        Canvas dragCanvas = Managers.UI.GetCachedUIByName("Draggable_Canvas").GetComponentInParent<Canvas>();
        canvas = dragCanvas.transform;
        _dragPlane = canvas as RectTransform;
        _dragCamera = dragCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : dragCanvas.worldCamera;
        Managers.UI.SetDragging(this, true);
        
       
        transform.SetParent(canvas);
        transform.SetAsLastSibling();

        canvasGroup.alpha= 0.6F;
        canvasGroup.blocksRaycasts = false;
        _pointerOffset = Vector3.zero;
        if (_isPopupUI && RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _dragPlane, eventData.pressPosition, _dragCamera, out Vector2 pressPoint))
        {
            _pointerOffset = rect.localPosition - (Vector3)pressPoint;
        }
        OnDrag(eventData);
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (_dragPlane != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _dragPlane, eventData.position, _dragCamera, out Vector2 pointerPoint))
        {
            rect.localPosition = (Vector3)pointerPoint + _pointerOffset;
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        Managers.UI.SetDragging(this, false);
        if(true ==_isPopupUI)
        {
            transform.SetParent(_originTransform);
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts =true;
            Managers.UI.BringPopupToFront(GetComponentInParent<UI_Popup>());

            return;
        }
        if(transform.parent ==canvas)
        {
            //슬롯
            if (OriginTransform != null)
            {
                if (PreviousTransform!= OriginTransform)
                    PreviousTransform.gameObject
                        ?.GetComponentInChildren<UI_Base>()
                        ?.EmptySlotKey(GetComponentInChildren<ItemDataStorage>());

                GetBackToOrigin();
            }

            DropEvent?.Invoke(); //외부에서 원하는 드롭이벤트 추가 하고 수행후 초기화
            DropEvent=null;
        }
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts =true; 
    }
    private void OnDisable()
    {
        Managers.UIIfExists?.SetDragging(this, false);
    }

    public void GetBackToOrigin()
    {
        transform.SetParent(_originTransform);
        rect.position = _originTransform.GetComponent<RectTransform>().position;
        _originTransform?.gameObject?.GetComponentInParent<UI_Base>()?.FixDropItem(rect);

        previousTransform = _originTransform;
        _isPrevOriginal =true;
    }
}
