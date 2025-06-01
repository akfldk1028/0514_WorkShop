/*
 * 맵 매니저 (MapManager)
 * 
 * 역할:
 * 1. 게임 맵 데이터 로드 및 관리
 * 2. 맵 정보 (이름, 크기, 구조 등) 저장 및 접근 기능 제공
 * 3. 맵 그리드 시스템 관리 - 좌표계 변환 및 위치 기반 기능 지원
 * 4. 타일맵 기반 게임에서 타일 정보 접근 및 관리
 * 5. 게임 내 지형 및 장애물 데이터 제공
 * 6. 맵 관련 이벤트 및 상호작용 처리
 * 7. Managers 클래스를 통해 전역적으로 접근 가능한 맵 데이터 제공
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Define;


public class MapManager
{
	public MapManager()
	{
		Debug.Log("<color=magenta>[MapManager]</color> 생성됨");
	}
	
	public GameObject Map { get; private set; }
	public string MapName { get; private set; }
	public Grid CellGrid { get; private set; }
	Dictionary<Vector3Int, BaseObject> _cells = new Dictionary<Vector3Int, BaseObject>();
	private int MinX;
	private int MaxX;
	private int MinY;
	private int MaxY;

	public Vector3Int World2Cell(Vector3 worldPos)
	{
		// Floor의 스케일 때문에 복잡해진 계산을 단순화
		return CellGrid.WorldToCell(worldPos);
	}

	public Vector3 Cell2World(Vector3Int cellPos)
	{
		// 셀의 중심점을 반환
		return CellGrid.GetCellCenterWorld(cellPos);
	}    
	List<Vector3> _waitingCells = new List<Vector3>();

	
	public IReadOnlyList<Vector3> WaitingCells => _waitingCells;
	public Vector3 DoorPosition { get; private set; }

	Vector3 _waitingCellPos;

	public Vector3 WaitingCellPos => _waitingCellPos;
	
	public void LoadMap(string mapName)
	{
		if (Map != null)
		{
			Managers.Resource.Destroy(Map);
		}

		// GameObject map = Managers.Resource.Instantiate(mapName);
		GameObject map = GameObject.Find("Restaurant");
		if (map == null)
		{
			Debug.LogError("RestaurantBar 오브젝트를 찾을 수 없습니다.");
			return;
		}

	

		map.name = $"@Map_{mapName}";

		// Door 위치 캐싱
		GameObject doorObj = GameObject.Find("Transform/Door");
		if (doorObj.IsValid())
		{
			DoorPosition = doorObj.GetPosition();
			Debug.Log("Door Pos: " + DoorPosition);
		}


		Map = map;
		MapName = mapName;
		CellGrid = map.GetComponent<Grid>();
		// CellGrid = Util.FindChild<Grid>(map, "Floor", true);
		// CellGrid = map.GetComponentInChildren<Grid>(true); // true면 비활성화 포함

		// CellGrid = GameObject.Find("Floor").GetComponent<Grid>();

		
		if (CellGrid == null)
		{
			Debug.LogError("Grid 컴포넌트를 찾을 수 없습니다.");
		}

		CacheWaitingPlaces();
	}

	private void CacheWaitingPlaces()
	{
		_waitingCells.Clear();


		GameObject WaitingObj = GameObject.Find("WaitingPlaces");
		if (WaitingObj == null)
		{
			Debug.LogWarning("WaitingPlaces 오브젝트를 찾을 수 없습니다.");
			return;
		}

		Transform waitingRoot = WaitingObj.transform;

        _waitingCellPos = WaitingObj.transform.position;

		Debug.Log($"[MapManager] WaitingPlaces found, childCount = {waitingRoot.childCount}");
		foreach (Transform place in waitingRoot)
		{
			// 1) child 이름 찍어보고
			Debug.Log($"  └─ place: {place.name} @ worldPos={place.position}");

			// 2) 월드→셀 좌표 변환
			_waitingCells.Add(place.position);

			// 3) 변환된 셀좌표도 찍어보기
			// Debug.Log($"     → cellPos: {cellPos}");
			foreach (var cell in WaitingCells)
			{
				Debug.Log($"     → WaitingCells: {cell}");
			}
		}
	}



	
    public Vector3 GetCellCenterWorld(Vector3Int cellPos)
    {
        return CellGrid.GetCellCenterLocal(cellPos);
    }

    public bool IsCellValid(Vector3Int cellPos)
    {
        // if (cellPos.x < MinX || cellPos.x > MaxX || cellPos.y < MinY || cellPos.y > MaxY)
        //     return false;
        return !_cells.ContainsKey(cellPos);
    }
		// 오브젝트 배치
	public GameObject PlaceObjectAtCell(GameObject prefab, Vector3Int cellPos)
	{
		Vector3 worldPos = GetCellCenterWorld(cellPos);
	
		GameObject instance = Managers.Resource.Instantiate(prefab.name, worldPos);
		BaseObject baseObj = instance.GetComponent<BaseObject>();
		if (baseObj != null)
		{
			_cells[cellPos] = baseObj;
		}
		
		return instance;
	}


}
