using UnityEngine;
using UnityEngine.UI;


[ExecuteAlways]
[RequireComponent(typeof(GridLayoutGroup), typeof(RectTransform))]
public class ResponsiveGrid : MonoBehaviour
{
    public int columns = 10;                 // 고정 열 개수
    public bool keepSquare = true;           // 정사각형 유지
    public Vector2 minCell = new Vector2(32, 32);
    public Vector2 maxCell = new Vector2(200, 200);

    GridLayoutGroup grid;
    RectTransform rt;

    void OnEnable() { Cache(); Recalc(); }
    void OnRectTransformDimensionsChange() { Recalc(); } // 부모/자기 크기 바뀌면 자동 갱신
    void Cache() { if (!grid) grid = GetComponent<GridLayoutGroup>(); if (!rt) rt = GetComponent<RectTransform>(); }

    void Recalc()
    {
        if (!grid || !rt || columns < 1) return;

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;

        var pad = grid.padding;
        float w = rt.rect.width  - pad.left - pad.right;
        float h = rt.rect.height - pad.top  - pad.bottom;

        float cellW = (w - grid.spacing.x * (columns - 1)) / columns;

        // 필요한 행 수 추정(자식 수가 변동된다면 더 정밀 계산해도 됨)
        int childCount = Mathf.Max(1, rt.childCount);
        int rows = Mathf.Max(1, Mathf.CeilToInt(childCount / (float)columns));
        float cellH = (h - grid.spacing.y * (rows - 1)) / rows;

        Vector2 cell = keepSquare ? Vector2.one * Mathf.Min(cellW, cellH) : new Vector2(cellW, cellH);
        cell.x = Mathf.Clamp(cell.x, minCell.x, maxCell.x);
        cell.y = Mathf.Clamp(cell.y, minCell.y, maxCell.y);

        grid.cellSize = cell;
        LayoutRebuilder.MarkLayoutForRebuild(rt);
    }
}

