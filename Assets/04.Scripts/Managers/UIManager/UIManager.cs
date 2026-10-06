using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using System.IO;
using System.Threading;
using Unity.AppUI.UI;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Rendering.LookDev;
public enum UI_SortOrder
{
    WorldUIBase =0,
    SceneUIBase = 1,
    PopUpUIBase =10,
    DraggableUIBase =1000

}
public class UIManager
{
    int _zOrder =(int)UI_SortOrder.PopUpUIBase; //다음에 올 팝업에게 배정할 Count

    Stack<UI_Popup> _popupStack = new Stack<UI_Popup>();
    private readonly HashSet<UI_Draggable_Move> _activeDrags = new HashSet<UI_Draggable_Move>();
    private static readonly HashSet<string> NonModalPopupNames = new HashSet<string>(System.StringComparer.Ordinal)
    {
        "InventoryPannel_Canvas_Prefab",
        "UI_Equipment_Canvas_Prefab",
        "SkillBook_Canvas_Prefab",
        "Quest_Canvas_Prefab"
    };

    private UI_Popup _focusedPopup;
    private EventSystem _pointerEventSystem;
    private PointerEventData _pointerEventData;
    private readonly List<RaycastResult> _pointerHits = new List<RaycastResult>();
    private int _lastPointerFrame = -1;
    private bool _consumeLeftClickUntilRelease;
    private bool _consumeGameplayInputUntilRelease;
    private bool _clickedUIThisFrame;

    public bool IsDragging => _activeDrags.Count > 0;
    private bool IsFocusedPopupActive =>
        _focusedPopup != null && _focusedPopup.gameObject.activeInHierarchy;

    private bool HasModalPopup
    {
        get
        {
            foreach (UI_Popup popup in _popupStack)
            {
                if (popup != null && popup.gameObject.activeInHierarchy &&
                    !NonModalPopupNames.Contains(popup.name)) return true;
            }
            return false;
        }
    }

    private static bool IsTextInput(GameObject selected)
    {
        return selected != null &&
            (selected.GetComponentInParent<TMP_InputField>() != null ||
             selected.GetComponentInParent<UnityEngine.UI.InputField>() != null);
    }

    private static bool HasTextInputFocus()
    {
        EventSystem eventSystem = EventSystem.current;
        return eventSystem != null && IsTextInput(eventSystem.currentSelectedGameObject);
    }

    // Resolve the left click once per frame so the camera and player use the same decision.
    private void UpdateInputFocus()
    {
        if (_lastPointerFrame == Time.frameCount) return;
        _lastPointerFrame = Time.frameCount;
        _clickedUIThisFrame = false;

        if (!Input.GetMouseButton(0))
        {
            _consumeLeftClickUntilRelease = false;
            _consumeGameplayInputUntilRelease = false;
        }
        if (!Input.GetMouseButtonDown(0)) return;

        EventSystem eventSystem = EventSystem.current;
        UI_Popup clickedPopup = null;
        bool clickedUI = false;
        if (eventSystem != null)
        {
            if (_pointerEventSystem != eventSystem)
            {
                _pointerEventSystem = eventSystem;
                _pointerEventData = new PointerEventData(eventSystem);
            }
            _pointerEventData.Reset();
            _pointerEventData.position = Input.mousePosition;
            _pointerHits.Clear();
            eventSystem.RaycastAll(_pointerEventData, _pointerHits);
            foreach (RaycastResult hit in _pointerHits)
            {
                if (!(hit.module is UnityEngine.UI.GraphicRaycaster)) continue;
                clickedUI = true;
                clickedPopup = hit.gameObject.GetComponentInParent<UI_Popup>();
                break;
            }
            _pointerHits.Clear();
        }

        if (clickedUI)
        {
            _clickedUIThisFrame = true;
            _consumeLeftClickUntilRelease = true;
            if (clickedPopup != null) BringPopupToFront(clickedPopup);
            return;
        }

        if (HasModalPopup || IsDragging)
        {
            _consumeLeftClickUntilRelease = true;
            return;
        }

        GameObject selected = eventSystem != null ? eventSystem.currentSelectedGameObject : null;
        bool selectedButton = selected != null &&
            selected.GetComponentInParent<UnityEngine.UI.Button>() != null;
        bool selectedTextInput = IsTextInput(selected);
        bool hadFocusedPopup = IsFocusedPopupActive;

        if (selected != null) eventSystem.SetSelectedGameObject(null);
        _focusedPopup = null;
        if (selectedTextInput || (hadFocusedPopup && !selectedButton))
        {
            _consumeLeftClickUntilRelease = true;
            _consumeGameplayInputUntilRelease = true;
        }
    }

    public bool IsPointerOverUI
    {
        get
        {
            UpdateInputFocus();
            if (Input.GetMouseButtonDown(0)) return _clickedUIThisFrame;
            EventSystem eventSystem = EventSystem.current;
            return eventSystem != null && eventSystem.IsPointerOverGameObject();
        }
    }

    public bool HasFocusedPopup
    {
        get
        {
            UpdateInputFocus();
            return HasModalPopup || IsFocusedPopupActive;
        }
    }

    public bool BlocksGameplayInput
    {
        get
        {
            UpdateInputFocus();
            return HasModalPopup || IsFocusedPopupActive || IsDragging ||
                HasTextInputFocus() || _consumeGameplayInputUntilRelease;
        }
    }

    public bool BlocksAttackInput
    {
        get
        {
            UpdateInputFocus();
            return HasModalPopup || IsFocusedPopupActive || IsDragging ||
                HasTextInputFocus() || _consumeLeftClickUntilRelease;
        }
    }

    public bool BlocksCameraInput
    {
        get
        {
            UpdateInputFocus();
            return HasModalPopup || IsFocusedPopupActive || IsDragging ||
                HasTextInputFocus() || _consumeLeftClickUntilRelease;
        }
    }

    public void BringPopupToFront(UI_Popup popup)
    {
        if (popup == null || !popup.gameObject.activeInHierarchy || !_popupStack.Contains(popup)) return;
        _focusedPopup = popup;
        if (_popupStack.Peek() == popup) return;

        UI_Popup[] popups = _popupStack.ToArray();
        _popupStack.Clear();
        _zOrder = (int)UI_SortOrder.PopUpUIBase;
        for (int i = popups.Length - 1; i >= 0; i--)
        {
            UI_Popup current = popups[i];
            if (current == null || current == popup) continue;
            current.GetComponent<UnityEngine.Canvas>().sortingOrder = _zOrder++;
            _popupStack.Push(current);
        }
        popup.GetComponent<UnityEngine.Canvas>().sortingOrder = _zOrder++;
        _popupStack.Push(popup);
        popup.transform.SetAsLastSibling();
    }
    public void SetDragging(UI_Draggable_Move draggable, bool dragging)
    {
        if (draggable == null) return;
        if (dragging) _activeDrags.Add(draggable);
        else _activeDrags.Remove(draggable);
    }
    UI_Scene _sceneUI ; // 고정 캔버스 UI (hud) 

    List<GameObject> _gameObjects= new List<GameObject>();
    Dictionary<string, GameObject> _openUIObjects = new Dictionary<string, GameObject>();
    Dictionary <string, GameObject> _cachedUIObjects = new Dictionary<string, GameObject>();
    public void Init()
    {
        GameObject  root =Root;
       
    }
    public GameObject Root
    {
        get
        {
            GameObject root = GameObject.Find("@UI_Root");
            if (null ==root)
                root= new GameObject { name = "@UI_Root" };
            return root; 
        }
    }
 
    public GameObject GetOpenUIByName(string Name)
    {
        if(_openUIObjects.ContainsKey(Name))
            return _openUIObjects[Name];
        return null;
    }
    public void RegisterCachedUI(string Name, GameObject go)
    {
        if (!_cachedUIObjects.ContainsKey(Name))
            _cachedUIObjects.Add(Name, go);
    }
    public GameObject GetCachedUIByName(string Name)
    {
        if (_cachedUIObjects.ContainsKey(Name))
            return _cachedUIObjects[Name];
        return null;
    }
    public void SetCanvas(GameObject go, bool sort =true)
    {
        //소트오더만 관ㄹ리

        UnityEngine.Canvas canvas = UIUtil.GetOrAddComponent<UnityEngine.Canvas>(go);
        //겜오에 캔버스 있으면 가져와 
        canvas.renderMode= RenderMode.ScreenSpaceOverlay; //캔버스는 기본 오버레이모드로 

        canvas.overrideSorting = true;  //캔버스 중첩이면  , 자식의 오더값만 처리 

        if (sort)//팝업이면 젤 나중에 
        {
            canvas.sortingOrder =(int)UI_SortOrder.PopUpUIBase;  /*_zOrder;*/
            //++_zOrder;
        }
        else
        {
            if (null == go.GetComponent<UI_Popup>())
            {
                canvas.sortingOrder = (int)UI_SortOrder.SceneUIBase;
            }
            if (null == go.GetComponent<UI_World>())
            {

                canvas.sortingOrder = (int)UI_SortOrder.WorldUIBase;
            }
        }
    }

    public T ShowSceneUI<T>(string path ) where T : UI_Scene 
    {
        Debug.Assert(! string.IsNullOrEmpty(path));//타입의 이름으로

        GameObject go = Managers.Resource.Instantiate($"UI/{path}");//풀링이면 풀링으로 가져오기


        T sceneUI = UIUtil.GetOrAddComponent<T>(go);
        _sceneUI = sceneUI;
        go.transform.SetParent(Root.transform);

        _openUIObjects.Add(go.name, go);
        if( !_cachedUIObjects.ContainsKey(go.name))
        _cachedUIObjects.Add(go.name, go);
        return sceneUI;
    }
    public T ShowWorldUI<T>(string path) where T : UI_World
    {
        Debug.Assert(!string.IsNullOrEmpty(path));//타입의 이름으로

        GameObject go = Managers.Resource.Instantiate($"UI/{path}");//풀링이면 풀링으로 가져오기
        T sceneUI = UIUtil.GetOrAddComponent<T>(go);
        go.transform.SetParent(Root.transform);

        if (!_cachedUIObjects.ContainsKey(go.name))
            _cachedUIObjects.Add(go.name, go);
        _openUIObjects.Add(go.name, go);
        return sceneUI;

    }

    //Init() (SetCavas)-> ShowPopupUI()->
    public T ShowPopupUI<T>(string name) where T : UI_Popup// ui 킴  //얘는 미리 등록
    {
        Debug.Assert(!string.IsNullOrEmpty(name));//타입의 이름으로

        GameObject go = GetOpenUIByName(name);
        T popup=null;
        bool closeReturn = false;
        if (null != go)
        {
            Debug.Log($"이미 열려있는 팝업 {name} ");
            popup = UIUtil.GetOrAddComponent<T>(go);
            closeReturn = ClosePopupUI(popup);// 나 일단 지워
        }
        Debug.Assert(closeReturn==true || null==go); //삭제 했으니까 없느 상태

        go = Managers.Pool.LendPoolableTo(name, null, true, 1).gameObject; //풀에서 액티브 후 꺼내기 //미리 등록
        
        popup = UIUtil.GetOrAddComponent<T>(go);
        
        

        //위에 추가

        _openUIObjects.Add(go.name, go);
        _focusedPopup = popup;
        _popupStack.Push(popup);// 스택이니까 가장 마지막에 연게 팝업
        go.transform.SetParent(Root.transform);// 루트에 등록) @root
        UnityEngine.Canvas canvas = UIUtil.GetOrAddComponent<UnityEngine.Canvas>(popup.gameObject);
        if (canvas) canvas.sortingOrder = _zOrder++;

        if (!_cachedUIObjects.ContainsKey(go.name))
            _cachedUIObjects.Add(go.name, go);

        return popup;
    }
   //Reorder : 외부에서 닫고, Init()-> ShowPopup다시 수행 -> Reorder 
    public bool ClosePopupUI(UI_Popup popup) // Safe //닫힘 버튼 누르면 이거 호출
    {
        if (0==_popupStack.Count) // 비어있는 스택이라면 삭제 불가
            return false;

        bool check = false;
        //_zOrder = (int)UI_SortOrder.PopUpUIBase;
        //O(2n)
        if (_popupStack.Peek() != popup)
        {
            Stack<UI_Popup> popUpList = new Stack<UI_Popup>();
            while (_popupStack.Count>0)
            {
                // 꺼낸다 -> 비교한다 -> 맞다( 삭제하고 다시 원상복구)// 없다  삭제없이 원상복구

                UI_Popup _nowPeek = _popupStack.Peek();
                if (_nowPeek ==  popup)
                {
                    UnityEngine.Canvas canvas = UIUtil.GetOrAddComponent<UnityEngine.Canvas>(popup.gameObject);
                    if (canvas) _zOrder =canvas.sortingOrder;

                    check =true;
                    ClosePopupUI(); //꺼내서 삭제하고 나가서 원상복구
                    break;
                }

                popUpList.Push(_popupStack.Peek());
                _popupStack.Pop();
            }

            while (0 < popUpList.Count)
            {
                UI_Popup peek = popUpList.Pop();
                UnityEngine.Canvas canvas = UIUtil.GetOrAddComponent<UnityEngine.Canvas>(peek.gameObject);
                if (canvas) canvas.sortingOrder = _zOrder;
                _popupStack.Push(peek);
                ++_zOrder;
            }

            if (!check)
            {
                Debug.Log($"없어 스택에 Close Popup Failed!"); // 스택의 가장 위에있는 Peek() 것만 삭제할 수 잇기 때문에 popup이 Peek()가 아니면 삭제 못함
                return false;
            }
        }
        else {
            --_zOrder;
            ClosePopupUI(); //그냥 닫기
        }
        _openUIObjects.Remove(popup.name);
        EventSystem eventSystem = EventSystem.current;
        GameObject selected = eventSystem != null ? eventSystem.currentSelectedGameObject : null;
        if (selected != null && selected.transform.IsChildOf(popup.transform))
            eventSystem.SetSelectedGameObject(null);
        if (_focusedPopup == popup)
        {
            _focusedPopup = null;
            foreach (UI_Popup openPopup in _popupStack)
            {
                if (openPopup == null || !openPopup.gameObject.activeInHierarchy) continue;
                _focusedPopup = openPopup;
                break;
            }
        }
        
        return true;
    }

    private void ClosePopupUI()
    {
        if (0==_popupStack.Count)
            return;

        UI_Popup popup = _popupStack.Pop();
        Managers.Resource.Destroy(popup.gameObject); //삭제 아니면 gETbACK(내부에서 )
        popup = null;
    
    }

    public void CloseAllPopupUI()
    {
        while (_popupStack.Count > 0)
            ClosePopupUI();
        _focusedPopup = null;
        _consumeLeftClickUntilRelease = false;
        _consumeGameplayInputUntilRelease = false;
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem != null) eventSystem.SetSelectedGameObject(null);
    }
}


