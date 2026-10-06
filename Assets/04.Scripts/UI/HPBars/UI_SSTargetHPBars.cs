using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DefaultExecutionOrder(100)]
public class UI_SSTargetHPBars : MonoBehaviour
{
    [SerializeField, Tooltip("Layers that block monster HP bars: Default, Terrain, Obstacles.")]
    private LayerMask _occlusionMask = (1 << 0) | (1 << 3) | (1 << 6);

    private sealed class TargetBar
    {
        public Transform Target;
        public StatusScript Status;
        public UI_TargetHPBar Bar;
        public Renderer[] Renderers;
        public bool IsBoss;
    }

    private Camera _cam;
    private readonly List<TargetBar> _targets = new List<TargetBar>();
    private const string PoolingKey = "UI/WorldUI/Part/TargetHPBar";

    private void Start()
    {
        _cam = Camera.main;
        foreach (GameObject target in GameObject.FindGameObjectsWithTag("Enemies"))
        {
            StatusScript status = target.GetComponent<StatusScript>();
            if (status == null) continue;
            GameObject barObject = Managers.Resource.Instantiate(PoolingKey, transform);
            UI_TargetHPBar bar = barObject.GetComponent<UI_TargetHPBar>();
            bool isBoss = target.GetComponent<SmaugController>() != null;
            foreach (Graphic graphic in barObject.GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = false;
            if (isBoss) ConfigureBossBar(bar);
            bar.LoadSlider(status);
            barObject.SetActive(false);
            status.OnHealthChangedEvent += bar.UpdateSlider;
            _targets.Add(new TargetBar
            {
                Target = target.transform,
                Status = status,
                Bar = bar,
                IsBoss = isBoss,
                Renderers = target.GetComponentsInChildren<Renderer>(true)
            });
        }
    }

    private void LateUpdate()
    {
        if (_cam == null || !_cam.isActiveAndEnabled) _cam = Camera.main;
        foreach (TargetBar target in _targets)
        {
            if (target.Bar == null) continue;
            bool visible = _cam != null && _cam.isActiveAndEnabled &&
                target.Target != null && target.Target.gameObject.activeInHierarchy &&
                target.Status != null && !target.Status.IsDead && IsVisible(target);

            GameObject barObject = target.Bar.gameObject;
            if (barObject.activeSelf != visible)
            {
                barObject.SetActive(visible);
                if (visible) target.Bar.LoadSlider(target.Status);
            }
            if (visible)
            {
                if (target.IsBoss) LayoutBossBar(target.Bar);
                else target.Bar.transform.position = _cam.WorldToScreenPoint(GetBarAnchor(target));
            }
        }
    }

    private bool IsVisible(TargetBar target)
    {
        // Do not leave bars on screen when their anchors move behind/outside the camera.
        if (!target.IsBoss && !IsOnScreen(GetBarAnchor(target))) return false;

        foreach (Renderer renderer in target.Renderers)
        {
            if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy ||
                renderer.forceRenderingOff) continue;
            if (!(renderer is SkinnedMeshRenderer) && !(renderer is MeshRenderer)) continue;
            if ((_cam.cullingMask & (1 << renderer.gameObject.layer)) == 0) continue;

            Bounds bounds = renderer.bounds;
            // An exposed upper body still counts when low cover hides the center.
            if (HasClearView(target.Target, bounds.center) ||
                HasClearView(target.Target, bounds.center + Vector3.up * bounds.extents.y * 0.8f))
                return true;
        }
        return false;
    }

    private Vector3 GetBarAnchor(TargetBar target)
    {
        bool found = false;
        Bounds bounds = new Bounds();
        foreach (Renderer renderer in target.Renderers)
        {
            if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;
            if (!(renderer is SkinnedMeshRenderer) && !(renderer is MeshRenderer)) continue;
            if (!found) { bounds = renderer.bounds; found = true; }
            else bounds.Encapsulate(renderer.bounds);
        }
        return found ? new Vector3(bounds.center.x, bounds.max.y + 0.25f, bounds.center.z)
            : target.Target.position + Vector3.up;
    }

    private void ConfigureBossBar(UI_TargetHPBar bar)
    {
        RectTransform rect = (RectTransform)bar.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(720f, 56f);

        GameObject labelObject = new GameObject("BossName", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(rect, false);
        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.font = Resources.Load<TMP_FontAsset>("Font/MALGUNBD SDF");
        label.text = "스마우그";
        label.fontSize = 26f;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = labelRect.anchorMax = new Vector2(0.5f, 1f);
        labelRect.sizeDelta = new Vector2(720f, 36f);
        labelRect.anchoredPosition = new Vector2(0f, 12f);
        LayoutBossBar(bar);
    }

    private void LayoutBossBar(UI_TargetHPBar bar)
    {
        RectTransform parent = (RectTransform)transform;
        float scale = Mathf.Min(parent.rect.width / 1920f, parent.rect.height / 1080f);
        RectTransform rect = (RectTransform)bar.transform;
        rect.localScale = Vector3.one * Mathf.Max(0.01f, scale);
        rect.anchoredPosition = new Vector2(0f, -85f * scale);
    }

    private bool IsOnScreen(Vector3 point)
    {
        Vector3 viewport = _cam.WorldToViewportPoint(point);
        return viewport.z >= _cam.nearClipPlane && viewport.z <= _cam.farClipPlane &&
            viewport.x >= 0f && viewport.x <= 1f && viewport.y >= 0f && viewport.y <= 1f;
    }

    private bool HasClearView(Transform target, Vector3 point)
    {
        if (!IsOnScreen(point)) return false;
        Vector3 origin = _cam.transform.position;
        Vector3 direction = point - origin;
        float distance = direction.magnitude;
        if (distance <= Mathf.Epsilon) return true;
        if (!Physics.Raycast(origin, direction / distance, out RaycastHit hit,
                distance, _occlusionMask, QueryTriggerInteraction.Ignore)) return true;
        return hit.transform == target || hit.transform.IsChildOf(target);
    }

    private void OnDisable()
    {
        foreach (TargetBar target in _targets)
            if (target.Bar != null) target.Bar.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        foreach (TargetBar target in _targets)
            if (target.Status != null && target.Bar != null)
                target.Status.OnHealthChangedEvent -= target.Bar.UpdateSlider;
    }
}
