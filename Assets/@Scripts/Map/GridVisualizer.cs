using UnityEngine;

[ExecuteInEditMode] // 에디터 모드에서도 실행되도록
[RequireComponent(typeof(Grid))]
public class GridVisualizer : MonoBehaviour
{
    public bool showGrid = true;
    public Color gridColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);

    // 그리드 크기는 셀 크기와 수량으로 계산됨
    [Header("Grid Size (in cells)")]
    public int gridSizeX = 20;
    public int gridSizeZ = 20;

    private Grid grid;

    private void OnEnable()
    {
        grid = GetComponent<Grid>();
    }

    private void OnValidate()
    {
        // Inspector에서 값이 바뀌었을 때 그리드 업데이트
        grid = GetComponent<Grid>();
    }

    private void OnDrawGizmos()
    {
        if (!showGrid || grid == null) return;

        Vector3 cellSize = grid.cellSize;
        Vector3 cellGap = grid.cellGap;
        Vector3 effectiveCellSize = cellSize + cellGap;

        Gizmos.color = gridColor;

        // 그리드 기준점
        Vector3 gridOrigin = transform.position;

        // X축 기준 선 (Z 방향으로 진행)
        for (int z = 0; z <= gridSizeZ; z++)
        {
            float zPos = z * effectiveCellSize.z;
            Vector3 startPos = gridOrigin + new Vector3(0, 0, zPos);
            Vector3 endPos = gridOrigin + new Vector3(gridSizeX * effectiveCellSize.x, 0, zPos);
            Gizmos.DrawLine(startPos, endPos);
        }

        // Z축 기준 선 (X 방향으로 진행)
        for (int x = 0; x <= gridSizeX; x++)
        {
            float xPos = x * effectiveCellSize.x;
            Vector3 startPos = gridOrigin + new Vector3(xPos, 0, 0);
            Vector3 endPos = gridOrigin + new Vector3(xPos, 0, gridSizeZ * effectiveCellSize.z);
            Gizmos.DrawLine(startPos, endPos);
        }
    }

    // 그리드 내 월드 좌표를 셀 중앙 좌표로 스냅
    public Vector3 SnapToGrid(Vector3 worldPosition)
    {
        // Grid 컴포넌트 활용하여 셀 좌표 계산
        Vector3Int cellPos = grid.WorldToCell(worldPosition);
        return grid.GetCellCenterWorld(cellPos);
    }

    // 디버깅용: 씬 뷰에서 셀 위치 표시
    public void DebugShowCellPosition(Vector3 worldPosition)
    {
        Vector3Int cellPos = grid.WorldToCell(worldPosition);
        Vector3 cellCenter = grid.GetCellCenterWorld(cellPos);

        Debug.Log($"World Position: {worldPosition}");
        Debug.Log($"Cell Position: {cellPos}");
        Debug.Log($"Cell Center: {cellCenter}");

        // 씬 뷰에 5초간 표시
        Debug.DrawLine(cellCenter - Vector3.right * 0.5f, cellCenter + Vector3.right * 0.5f, Color.red, 5f);
        Debug.DrawLine(cellCenter - Vector3.forward * 0.5f, cellCenter + Vector3.forward * 0.5f, Color.red, 5f);
    }
}