using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_SettingsPopup : UI_Popup
{
    public const string PopupName = "UI_Settings_Canvas_Prefab";

    private TMP_FontAsset _font;
    private TextMeshProUGUI _title;
    private TextMeshProUGUI _continueLabel;
    private TextMeshProUGUI _quitLabel;
    private bool _confirmQuit;

    private void Awake()
    {
        Init();
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();
        _font = Resources.Load<TMP_FontAsset>("Font/MALGUNBD SDF");

        RectTransform backdrop = CreateRect("Backdrop", transform, Vector2.zero, Vector2.zero);
        backdrop.anchorMin = Vector2.zero;
        backdrop.anchorMax = Vector2.one;
        backdrop.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0.65f);

        RectTransform panel = CreateRect("SettingsPanel", backdrop, new Vector2(480, 340), Vector2.zero);
        panel.gameObject.AddComponent<Image>().color = new Color(0.12f, 0.10f, 0.08f, 0.98f);
        Outline outline = panel.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.65f, 0.48f, 0.25f);
        outline.effectDistance = new Vector2(2, -2);

        _title = CreateLabel(panel, "Title", new Vector2(430, 80), new Vector2(0, 100), 32);
        _continueLabel = CreateButton(panel, "Continue", new Vector2(0, 0), Back);
        _quitLabel = CreateButton(panel, "Quit", new Vector2(0, -85), RequestQuit);
    }

    private void OnEnable()
    {
        ShowMenu();
    }

    private void ShowMenu()
    {
        _confirmQuit = false;
        _title.text = "설정";
        _continueLabel.text = "계속하기";
        _quitLabel.text = "게임 종료";
    }

    public void Back()
    {
        if (_confirmQuit) ShowMenu();
        else ClosePopUpUI();
    }

    private void RequestQuit()
    {
        if (!_confirmQuit)
        {
            _confirmQuit = true;
            _title.text = "게임을 종료하시겠습니까?";
            _continueLabel.text = "취소";
            _quitLabel.text = "종료";
            return;
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private TextMeshProUGUI CreateButton(Transform parent, string name, Vector2 position, UnityAction action)
    {
        RectTransform rect = CreateRect(name, parent, new Vector2(360, 64), position);
        Image background = rect.gameObject.AddComponent<Image>();
        background.color = new Color(0.38f, 0.28f, 0.17f);
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = background;
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f);
        button.colors = colors;
        button.onClick.AddListener(action);
        return CreateLabel(rect, "Label", new Vector2(340, 60), Vector2.zero, 28);
    }

    private TextMeshProUGUI CreateLabel(Transform parent, string name, Vector2 size, Vector2 position, float fontSize)
    {
        RectTransform rect = CreateRect(name, parent, size, position);
        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = _font;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(1, 0.93f, 0.79f);
        label.raycastTarget = false;
        return label;
    }

    private static RectTransform CreateRect(string name, Transform parent, Vector2 size, Vector2 position)
    {
        RectTransform rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        rect.gameObject.layer = parent.gameObject.layer;
        return rect;
    }
}
