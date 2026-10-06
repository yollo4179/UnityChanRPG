using System.Collections.Generic;
using UnityEngine;

// Keep top-level popup panels visible after resizing, reopening, or dragging.
[RequireComponent(typeof(Canvas))]
public class UI_KeepPanelsOnScreen : MonoBehaviour
{
    private readonly Dictionary<RectTransform, Vector3> _originalScales =
        new Dictionary<RectTransform, Vector3>();
    private readonly Vector3[] _corners = new Vector3[4];
    private RectTransform _canvasRect;
    private const float Margin = 12f;

    private void Awake()
    {
        _canvasRect = GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        Rect area = _canvasRect.rect;
        if (area.width <= Margin * 2 || area.height <= Margin * 2) return;

        foreach (Transform child in transform)
        {
            RectTransform panel = child as RectTransform;
            if (panel == null || !panel.gameObject.activeInHierarchy) continue;
            // Full-screen backgrounds must remain stretched to the canvas edges.
            if (panel.anchorMin != panel.anchorMax) continue;

            if (!_originalScales.TryGetValue(panel, out Vector3 originalScale))
            {
                originalScale = panel.localScale;
                _originalScales.Add(panel, originalScale);
            }

            float width = panel.rect.width * Mathf.Abs(originalScale.x);
            float height = panel.rect.height * Mathf.Abs(originalScale.y);
            if (width <= 0 || height <= 0) continue;
            float fit = Mathf.Min(1f, (area.width - Margin * 2) / width,
                (area.height - Margin * 2) / height);
            panel.localScale = originalScale * fit;

            panel.GetWorldCorners(_corners);
            Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            for (int i = 0; i < _corners.Length; i++)
            {
                Vector2 point = _canvasRect.InverseTransformPoint(_corners[i]);
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }

            Vector2 offset = Vector2.zero;
            if (min.x < area.xMin + Margin) offset.x = area.xMin + Margin - min.x;
            else if (max.x > area.xMax - Margin) offset.x = area.xMax - Margin - max.x;
            if (min.y < area.yMin + Margin) offset.y = area.yMin + Margin - min.y;
            else if (max.y > area.yMax - Margin) offset.y = area.yMax - Margin - max.y;
            panel.anchoredPosition += offset;
        }
    }
}
