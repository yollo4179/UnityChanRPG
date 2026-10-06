using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public sealed class UI_ClosePopupButton : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(ClosePopup);
    }

    private void ClosePopup()
    {
        GetComponentInParent<UI_Popup>()?.ClosePopUpUI();
    }
}
