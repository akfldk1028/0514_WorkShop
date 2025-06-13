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
	public Vector3 PlayerPosition { get; private set; }

	Vector3 _waitingCellPos;

	public Vector3 WaitingCellPos => _waitingCellPos;
	
	// 🆕 간단한 줄서기 시스템
	private Customer[] _waitingQueue; // 대기열 (0번부터 순서대로)
	
	public int WaitingCustomerCount 
	{ 
		get 
		{
			if (_waitingQueue == null) return 0;
			int count = 0;
			for (int i = 0; i < _waitingQueue.Length; i++)
			{
				if (_waitingQueue[i] != null) count++;
			}
			return count;
		}
	}
	
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

		GameObject playerObj = GameObject.Find("Transform/Player");
		if (playerObj.IsValid())
		{
			PlayerPosition = playerObj.GetPosition();
			Debug.Log("Player Pos: " + PlayerPosition);
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
		InitializeWaitingQueue();
	}

	private void CacheWaitingPlaces()
	{
		_waitingCells.Clear();

		GameObject WaitingObj = GameObject.Find("WaitingPlaces/WaitingPlacesForCustomer");
		if (WaitingObj == null)
		{
			Debug.LogWarning("WaitingPlaces 오브젝트를 찾을 수 없습니다.");
			return;
		}

		Transform waitingRoot = WaitingObj.transform;
		_waitingCellPos = WaitingObj.transform.position;

		// 자식 오브젝트 수만큼 z축으로 일렬 배치
		int childCount = waitingRoot.childCount;
		Debug.Log($"[MapManager] WaitingPlaces found, childCount = {childCount}");
		
		// 기본 위치에서 z축으로 1.5씩 간격을 두고 배치
		Vector3 basePosition = _waitingCellPos;
		float spacing = 1.5f; // 대기 간격
		
		for (int i = 0; i < childCount; i++)
		{
			Vector3 waitingPosition = new Vector3(
				basePosition.x, 
				basePosition.y, 
				basePosition.z + (i * spacing) // z축으로 순서대로 배치
			);
			
			_waitingCells.Add(waitingPosition);
			Debug.Log($"  └─ 대기위치 {i}: {waitingPosition}");
		}
		
		Debug.Log($"[MapManager] z축 일렬 대기열 생성 완료! 총 {_waitingCells.Count}개 위치");
	}

	/// <summary>
	/// 대기열 시스템 초기화
	/// </summary>
	private void InitializeWaitingQueue()
	{
		_waitingQueue = new Customer[_waitingCells.Count];
		Debug.Log($"<color=magenta>[MapManager]</color> 간단한 대기열 시스템 초기화 완료 - 총 {_waitingCells.Count}개 대기 위치");
	}
	
	/// <summary>
	/// 다음 사용 가능한 대기 위치 반환 (스폰용)
	/// </summary>
	public Vector3 GetNextAvailableWaitingPositionForSpawn()
	{
		if (_waitingQueue == null) return _waitingCellPos; // 기본 위치 반환
		
		// 첫 번째 빈 자리 찾기
		for (int i = 0; i < _waitingQueue.Length; i++)
		{
			if (_waitingQueue[i] == null)
			{
				return _waitingCells[i];
			}
		}
		
		// 모든 자리가 꽉 찬 경우 기본 위치 반환
		return _waitingCellPos;
	}
	
	/// <summary>
	/// 고객을 대기열에 등록 (스폰 후 호출)
	/// </summary>
	public void RegisterCustomerInQueue(Customer customer, Vector3 position)
	{
		if (customer == null || _waitingQueue == null) return;
		
		// 해당 위치의 인덱스 찾기
		for (int i = 0; i < _waitingCells.Count; i++)
		{
			if (Vector3.Distance(_waitingCells[i], position) < 0.5f)
			{
				_waitingQueue[i] = customer;
				Debug.Log($"<color=green>[MapManager]</color> {customer.name} 대기열 {i}번 위치에 등록");
				return;
			}
		}
	}
	
	/// <summary>
	/// 고객을 대기열에서 제거하고 뒤의 고객들을 앞으로 이동
	/// </summary>
	public void RemoveFromWaitingQueue(Customer customer)
	{
		if (customer == null || _waitingQueue == null) return;
		
		// 고객 찾아서 제거
		for (int i = 0; i < _waitingQueue.Length; i++)
		{
			if (_waitingQueue[i] == customer)
			{
				_waitingQueue[i] = null;
				Debug.Log($"<color=orange>[MapManager]</color> {customer.name} 대기열 {i}번 위치에서 제거됨");
				
				// 뒤의 고객들을 앞으로 한 칸씩 이동
				MoveCustomersForward(i);
				break;
			}
		}
	}
	
	/// <summary>
	/// 지정된 위치부터 뒤의 고객들을 앞으로 한 칸씩 이동
	/// </summary>
	private void MoveCustomersForward(int startIndex)
	{
		for (int i = startIndex; i < _waitingQueue.Length - 1; i++)
		{
			_waitingQueue[i] = _waitingQueue[i + 1];
			
			// 고객이 있으면 새 위치로 이동 명령
			if (_waitingQueue[i] != null)
			{
				Vector3 newPosition = _waitingCells[i];
				if (_waitingQueue[i].agent != null)
				{
					_waitingQueue[i].agent.SetDestination(newPosition);
					Debug.Log($"<color=cyan>[MapManager]</color> {_waitingQueue[i].name} → {i}번 위치로 이동: {newPosition}");
				}
			}
		}
		
		// 마지막 자리는 null로 설정
		_waitingQueue[_waitingQueue.Length - 1] = null;
	}
	
	/// <summary>
	/// 대기열 상태 디버그 출력
	/// </summary>
	public void DebugPrintWaitingQueue()
	{
		Debug.Log($"<color=magenta>=== 간단한 대기열 상태 ===</color>");
		Debug.Log($"총 대기자: {WaitingCustomerCount}명");
		
		if (_waitingQueue != null)
		{
			for (int i = 0; i < _waitingQueue.Length; i++)
			{
				string customerName = _waitingQueue[i]?.name ?? "빈 자리";
				Debug.Log($"{i}번 위치: {customerName}");
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
		
		// Y값을 낮춰서 배치 (예: -2만큼 아래로)
		worldPos.y -= 0.5f;
	
		GameObject instance = Managers.Resource.Instantiate(prefab.name, worldPos);
		BaseObject baseObj = instance.GetComponent<BaseObject>();
		if (baseObj != null)
		{
			_cells[cellPos] = baseObj;
		}
		
		return instance;
	}


}
