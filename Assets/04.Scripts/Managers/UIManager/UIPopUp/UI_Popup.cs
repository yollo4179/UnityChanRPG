using UnityEngine;

public class UI_Popup : UI_Base
{
    public void showPopUp()
    {
        Managers.UI.ShowPopupUI<UI_Popup>(transform.name);
    }
    public override void Init() {
        Managers.UI.SetCanvas(gameObject,true);
    }
    public virtual void ClosePopUpUI()
    {
        Managers.UI.ClosePopupUI(this);
    }

   
}
