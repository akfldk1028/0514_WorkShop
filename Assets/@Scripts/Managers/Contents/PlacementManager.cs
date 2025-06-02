/*
 * 게임 매니저 (GameManager)
 * 
 * 역할:
 * 1. 게임의 핵심 데이터와 게임 진행 상태 관리
 * 2. 게임 세이브 데이터(리소스, 영웅 등) 저장 및 로드
 * 3. 플레이어 진행 상황 추적 및 저장
 * 4. 영웅 소유 상태 및 레벨, 경험치 등의 데이터 관리
 * 5. JSON 형식으로 게임 데이터를 저장하고 로드하는 기능 제공
 * 6. 게임 초기화 및 데이터 설정 제어
 * 7. Managers 클래스를 통해 전역적으로 접근 가능한 게임 데이터 제공
 * 8. 그리드 클릭 및 드래그 앤 드롭 인터랙션 처리
 */

using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class PlacementManager
{
    public PlacementManager()
    {
        Debug.Log("<color=yellow>[GameManager]</color> 생성됨");
    }
    public bool IsDraggingFromUI => _isDraggingFromUI;  // 프로퍼티로 노출
    public bool _isDraggingFromUI = false;
    #region Save & Load    
    public string Path { get { return Application.persistentDataPath + "/SaveData.json"; } }
    
    // 여기에 세이브/로드 관련 코드 추가
    #endregion
    
        #region Grid Control
    // 그리드 컨트롤 관련 변수
    private GameObject _highlightPrefab;
    private GameObject _placeablePrefab;
    private GameObject _ghostInstance;
    private GameObject _lastHighlight;
    
    // 그리드 초기화
       // 그리드 초기화


       
    public void InitGridControl(GameObject highlightPrefab)
    {
        _highlightPrefab = highlightPrefab;
        // _placeablePrefab = placeablePrefab;
        
        // 입력 이벤트 구독
        Managers.Input.OnMouseClickCell += HandleCellClick;
        Managers.Input.OnMouseDragCell += HandleCellDrag;
        Managers.Input.OnMouseDragEndCell += HandleCellDragEnd;
        
        Debug.Log("[GameManager] 그리드 이벤트 핸들러 등록 완료");
    }
    
    public void CancelDragFromUI()
    {
        if (_ghostInstance != null)
        {
            GameObject.Destroy(_ghostInstance);
            _ghostInstance = null;
        }
        
        ClearHighlight();
        _isDraggingFromUI = false;
    }
        
    // 레이어 설정 헬퍼
    private void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }

        // UI 드래그 시작
    public void StartDragFromUI(string prefabName)
    {
        Debug.Log($"[PlacementManager] StartDragFromUI: {prefabName}");
        _isDraggingFromUI = true;
        
        // SetPlaceablePrefab이 제대로 작동하는지 확인
        SetPlaceablePrefab(prefabName);
        Debug.Log($"_placeablePrefab null? {_placeablePrefab == null}");
        
        if (_placeablePrefab == null)
        {
            Debug.LogError($"프리팹 로드 실패: {prefabName}");
            return;
        }
        
        // 리소스 로드 확인
        Debug.Log($"프리팹 로드 성공: {_placeablePrefab.name}");
        
        // Y=0 평면과의 교차점 계산
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        
        float enter;
        if (groundPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Debug.Log($"Ground hit at: {hitPoint}");
            
            // 고스트 생성 전 확인
            Debug.Log($"고스트 생성 시도: {prefabName}");
            
            // Instantiate 방식 변경 시도
            _ghostInstance = GameObject.Instantiate(_placeablePrefab);
            
            if (_ghostInstance == null)
            {
                Debug.LogError("고스트 인스턴스 생성 실패!");
                return;
            }
            
            Debug.Log($"고스트 생성 성공: {_ghostInstance.name}");
            
            // 위치 설정
            Vector3Int cellPos = Managers.Map.World2Cell(hitPoint);
            Vector3 cellCenter = Managers.Map.Cell2World(cellPos);
            _ghostInstance.transform.position = new Vector3(cellCenter.x, 0.5f, cellCenter.z);
            
            // 반투명 처리
            ApplyTransparency(_ghostInstance, 0.5f);
            
            // 하이라이트 생성
            CreateHighlight(cellCenter);
        }
    }
    // UI 드래그 중 업데이트 (Update에서 호출)
    public void UpdateDragFromUI()
    {
        if (!_isDraggingFromUI || _ghostInstance == null) return;
        
        // Y=0 평면 사용 (Physics.Raycast 대신)
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        
        float enter;
        if (groundPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            
            // Floor 범위 내인지 확인
            if (Mathf.Abs(hitPoint.x) <= 50 && Mathf.Abs(hitPoint.z) <= 50)
            {
                Vector3Int cellPos = Managers.Map.World2Cell(hitPoint);
                Vector3 cellCenter = Managers.Map.Cell2World(cellPos);
                
                // 고스트 위치 업데이트
                _ghostInstance.transform.position = new Vector3(cellCenter.x, 0.5f, cellCenter.z);
                
                // 하이라이트 업데이트
                ClearHighlight();
                CreateHighlight(cellCenter);
                
                // 배치 가능 여부에 따라 색상 변경
                bool isValid = Managers.Map.IsCellValid(cellPos);
                UpdateGhostValidity(isValid);
            }
        }
    }
        
        // UI 드래그 종료
    // EndDragFromUI도 같은 방식으로 수정
    public void EndDragFromUI()
    {
        Debug.Log("[PlacementManager] EndDragFromUI 시작");
        
     
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        
        float enter;
        if (groundPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Debug.Log($"Hit Point: {hitPoint}");
            
            if (Mathf.Abs(hitPoint.x) <= 50 && Mathf.Abs(hitPoint.z) <= 50)
            {
                Vector3Int cellPos = Managers.Map.World2Cell(hitPoint);
                Debug.Log($"Cell Pos: {cellPos}");
                
                bool isValid = Managers.Map.IsCellValid(cellPos);
                Debug.Log($"Cell Valid: {isValid}");
                
                if (isValid)
                {
                    GameObject placedObj = Managers.Map.PlaceObjectAtCell(_placeablePrefab, cellPos);
                    Debug.Log($"배치된 오브젝트: {placedObj}");
                }
                else
                {
                    Debug.LogWarning("셀이 유효하지 않음!");
                }
            }
            else
            {
                Debug.Log("Floor 범위 밖");
            }
        }
        else
        {
            Debug.Log("Ground plane raycast 실패");
        }
        
        // 정리
        if (_ghostInstance != null)
        {
            GameObject.Destroy(_ghostInstance);
            _ghostInstance = null;
        }
        
        ClearHighlight();
        _isDraggingFromUI = false;
    }
    // 셀 클릭 처리
    private void HandleCellClick(Vector3Int cellPos)
    {
        Vector3 cellCenter = Managers.Map.GetCellCenterWorld(cellPos);
        Debug.Log($"[GameManager] 셀 클릭: {cellPos} -> {cellCenter}");
        
        // 이전 하이라이트 제거
        ClearHighlight();
        
        // 새 하이라이트 생성
        CreateHighlight(cellCenter);
    }
    
    // 드래그 시작/드래그 중 처리
    private void HandleCellDrag(Vector3Int cellPos)
    {
        Vector3 cellCenter = Managers.Map.GetCellCenterWorld(cellPos);
        
        // 첫 드래그일 경우 고스트 인스턴스 생성
        if (_ghostInstance == null)
        {
            _ghostInstance = Managers.Resource.Instantiate(_placeablePrefab.name);
            ApplyTransparency(_ghostInstance, 0.5f);
            Debug.Log("[GameManager] 드래그 시작");
        }
        
        // 고스트 인스턴스 위치 업데이트
        _ghostInstance.transform.position = cellCenter;
        
        // 배치 가능 여부에 따라 색상 변경
        bool isValid = Managers.Map.IsCellValid(cellPos);
        UpdateGhostValidity(isValid);
        
        Debug.Log($"[GameManager] 드래그 위치 업데이트: {cellPos}, 유효: {isValid}");
    }
    public void SetPlaceablePrefab(string prefabName)
    {
        _placeablePrefab = Managers.Resource.Load<GameObject>(prefabName);
        Debug.Log("[PlacementManager] SetPlaceablePrefab: " + prefabName);
    }
    





    // 드래그 종료 처리
    private void HandleCellDragEnd(Vector3Int cellPos)
    {
        if (_ghostInstance == null)
            return;
            
        // 배치 가능한 위치인지 확인
        if (Managers.Map.IsCellValid(cellPos))
        {
            // 맵 매니저를 통해 오브젝트 배치
            Managers.Map.PlaceObjectAtCell(_placeablePrefab, cellPos);
            Debug.Log("[GameManager] 드래그 완료, 오브젝트 배치");
        }
        
        GameObject.Destroy(_ghostInstance);
        _ghostInstance = null;
        ClearHighlight();
    }
    
    // 하이라이트 생성
    private void CreateHighlight(Vector3 position)
    {
        Vector3 highlightPos = position;
        highlightPos.y = 1f; // 바닥 위에 살짝 띄움
        _lastHighlight = GameObject.Instantiate(
            _highlightPrefab,
            highlightPos,
            Quaternion.Euler(90f, 0f, 0f)
        );
        
        _lastHighlight.transform.localScale = new Vector3(
            Managers.Map.CellGrid.cellSize.x,
            Managers.Map.CellGrid.cellSize.z,
            1f
        );
    }
    
    // 하이라이트 제거
    private void ClearHighlight()
    {
        if (_lastHighlight != null)
        {
            GameObject.Destroy(_lastHighlight);
            _lastHighlight = null;
        }
    }
    
    // 유효성에 따른 고스트 색상 업데이트
    private void UpdateGhostValidity(bool isValid)
    {
        foreach (var rend in _ghostInstance.GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in rend.materials)
            {
                if (mat.HasProperty("_Color"))
                {
                    Color color = isValid ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
                    mat.color = color;
                }
            }
        }
    }
    
    // 반투명 머티리얼 적용 헬퍼
    private void ApplyTransparency(GameObject obj, float alpha)
    {
        foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
        {
            foreach (var material in renderer.materials)
            {
                // Standard Shader의 경우
                if (material.HasProperty("_Color"))
                {
                    Color color = material.color;
                    color.a = alpha;
                    material.color = color;
                }
                
                // Rendering Mode를 Transparent로 변경
                material.SetFloat("_Mode", 3); // Transparent
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
            }
        }
    }
}
#endregion