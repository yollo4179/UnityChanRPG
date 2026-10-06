using UnityEngine;
using UnityEngine.UI;

public class UI_IconPopupPairScript : UI_Scene
{
    public enum Buttons
    {
        Inven_Button = 0,
        Equip_Button = 1,
        Quest_Button = 2,
        Skill_Button = 3,
        Setting_Button = 4,
        END
    }

    private static readonly string[] PopupNames =
    {
        "InventoryPannel_Canvas_Prefab",
        "UI_Equipment_Canvas_Prefab",
        "Quest_Canvas_Prefab",
        "SkillBook_Canvas_Prefab",
        UI_SettingsPopup.PopupName
    };

    private static readonly KeyCode[] PopupKeys =
    {
        KeyCode.I,
        KeyCode.E,
        KeyCode.Q,
        KeyCode.K,
        KeyCode.Escape
    };

    public UI_Popup[] popUpHandle = new UI_Popup[(int)Buttons.END];

    private void Awake()
    {
        Bind<Button>(typeof(Buttons));

        for (int i = 0; i < PopupKeys.Length; i++)
        {
            int index = i;
            (_objects[typeof(Button)][index] as Button).onClick.AddListener(
                () => TogglePopup(index));
        }
    }

    private void Update()
    {
        GameObject settings = Managers.UI.GetOpenUIByName(UI_SettingsPopup.PopupName);
        if (settings != null && settings.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                settings.GetComponent<UI_SettingsPopup>().Back();
            return;
        }

        for (int i = 0; i < PopupKeys.Length; i++)
        {
            if (!Input.GetKeyDown(PopupKeys[i])) continue;
            TogglePopup(i);
            break;
        }
    }

    private void TogglePopup(int index)
    {
        if (index != (int)Buttons.Setting_Button &&
            Managers.UI.GetOpenUIByName(UI_SettingsPopup.PopupName) != null) return;

        string popupName = PopupNames[index];
        GameObject openPopup = Managers.UI.GetOpenUIByName(popupName);

        if (openPopup != null)
        {
            UI_Popup popup = openPopup.GetComponent<UI_Popup>();
            if (popup != null)
                Managers.UI.ClosePopupUI(popup);
            popUpHandle[index] = null;
            return;
        }

        if (index == (int)Buttons.Setting_Button && Managers.Pool.GetOriginal(popupName) == null)
        {
            GameObject prefab = Managers.Resource.Load<GameObject>(
                "Prefabs/UI/PopUpUI/" + UI_SettingsPopup.PopupName);
            if (prefab == null)
            {
                Debug.LogError("Settings popup prefab is missing.");
                return;
            }
            Managers.Pool.CreatePool(prefab, true, 1);
        }

        popUpHandle[index] = Managers.UI.ShowPopupUI<UI_Popup>(popupName);
    }
}
