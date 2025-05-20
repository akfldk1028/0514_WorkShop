using UnityEngine;

[ExecuteInEditMode] // ������ ��忡���� ����ǵ���
[RequireComponent(typeof(Grid))]
public class GridVisualizer : MonoBehaviour
{
    public bool showGrid = true;
    public Color gridColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);

    // �׸��� ũ��� �� ũ��� �������� ����
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
        // Inspector���� ���� �ٲ���� �� �׸��� ������Ʈ
        grid = GetComponent<Grid>();
    }

    private void OnDrawGizmos()
    {
        if (!showGrid || grid == null) return;

        Vector3 cellSize = grid.cellSize;
        Vector3 cellGap = grid.cellGap;
        Vector3 effectiveCellSize = cellSize + cellGap;

        Gizmos.color = gridColor;

        // �׸��� ������
        Vector3 gridOrigin = transform.position;

        // X�� ���� �� (Z �������� ����)
        for (int z = 0; z <= gridSizeZ; z++)
        {
            float zPos = z * effectiveCellSize.z;
            Vector3 startPos = gridOrigin + new Vector3(0, 0, zPos);
            Vector3 endPos = gridOrigin + new Vector3(gridSizeX * effectiveCellSize.x, 0, zPos);
            Gizmos.DrawLine(startPos, endPos);
        }

        // Z�� ���� �� (X �������� ����)
        for (int x = 0; x <= gridSizeX; x++)
        {
            float xPos = x * effectiveCellSize.x;
            Vector3 startPos = gridOrigin + new Vector3(xPos, 0, 0);
            Vector3 endPos = gridOrigin + new Vector3(xPos, 0, gridSizeZ * effectiveCellSize.z);
            Gizmos.DrawLine(startPos, endPos);
        }
    }

    // �׸��� �� ���� ��ǥ�� �� �߾� ��ǥ�� ����
    public Vector3 SnapToGrid(Vector3 worldPosition)
    {
        // Grid ������Ʈ Ȱ���Ͽ� �� ��ǥ ���
        Vector3Int cellPos = grid.WorldToCell(worldPosition);
        return grid.GetCellCenterWorld(cellPos);
    }

    // ������: �� �信�� �� ��ġ ǥ��
    public void DebugShowCellPosition(Vector3 worldPosition)
    {
        Vector3Int cellPos = grid.WorldToCell(worldPosition);
        Vector3 cellCenter = grid.GetCellCenterWorld(cellPos);

        Debug.Log($"World Position: {worldPosition}");
        Debug.Log($"Cell Position: {cellPos}");
        Debug.Log($"Cell Center: {cellCenter}");

        // �� �信 5�ʰ� ǥ��
        Debug.DrawLine(cellCenter - Vector3.right * 0.5f, cellCenter + Vector3.right * 0.5f, Color.red, 5f);
        Debug.DrawLine(cellCenter - Vector3.forward * 0.5f, cellCenter + Vector3.forward * 0.5f, Color.red, 5f);
    }
}