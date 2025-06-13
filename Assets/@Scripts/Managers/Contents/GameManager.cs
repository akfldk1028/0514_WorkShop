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
 */

using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class GameSaveData
{

	public int Gold = 0;
	public int Glass = 8; // 유리잔 개수 추가

}

public enum ResourceType
{
	Gold,
	Glass
}

public class GameResourceManager
{
	[System.Serializable]
	public class ResourceData
	{
		public int Amount;
		[System.NonSerialized]
		public System.Action UIRefreshAction;
		
		public ResourceData(int amount = 0)
		{
			Amount = amount;
		}
	}
	
	private Dictionary<ResourceType, ResourceData> _resources = new Dictionary<ResourceType, ResourceData>();
	
	public void InitializeResource(ResourceType type, int initialAmount = 0, System.Action uiRefreshAction = null)
	{
		if (!_resources.ContainsKey(type))
		{
			_resources[type] = new ResourceData(initialAmount);
		}
		_resources[type].UIRefreshAction = uiRefreshAction;
	}
	
	public int GetResource(ResourceType type)
	{
		return _resources.ContainsKey(type) ? _resources[type].Amount : 0;
	}
	
	public void SetResource(ResourceType type, int amount)
	{
		if (!_resources.ContainsKey(type))
			InitializeResource(type);
			
		_resources[type].Amount = Mathf.Max(0, amount);
		_resources[type].UIRefreshAction?.Invoke();
		Debug.Log($"[GameResourceManager] {type} 설정: {_resources[type].Amount}");
	}
	
	public void AddResource(ResourceType type, int amount)
	{
		if (amount > 0)
		{
			if (!_resources.ContainsKey(type))
				InitializeResource(type);
				
			_resources[type].Amount += amount;
			_resources[type].UIRefreshAction?.Invoke();
			Debug.Log($"[GameResourceManager] {type} 증가: +{amount}, 현재 {type}: {_resources[type].Amount}");
		}
	}
	
	public bool SubtractResource(ResourceType type, int amount)
	{
		if (amount > 0 && GetResource(type) >= amount)
		{
			_resources[type].Amount -= amount;
			_resources[type].UIRefreshAction?.Invoke();
					Debug.Log($"[GameResourceManager] {type} 감소: -{amount}, 현재 {type}: {_resources[type].Amount}");
		return true;
	}
	Debug.Log($"[GameResourceManager] {type} 부족: 필요 {amount}, 현재 {GetResource(type)}");
		return false;
	}
	
	public bool HasEnoughResource(ResourceType type, int requiredAmount)
	{
		return GetResource(type) >= requiredAmount;
	}
	
	// 세이브 데이터용 딕셔너리 반환
	public Dictionary<ResourceType, int> GetAllResourcesForSave()
	{
		var result = new Dictionary<ResourceType, int>();
		foreach (var kvp in _resources)
		{
			result[kvp.Key] = kvp.Value.Amount;
		}
		return result;
	}
	
	// 세이브 데이터에서 로드
	public void LoadResourcesFromSave(Dictionary<ResourceType, int> savedResources)
	{
		foreach (var kvp in savedResources)
		{
			SetResource(kvp.Key, kvp.Value);
		}
	}
}

public class GameManager
{

	GameSaveData _saveData = new GameSaveData();
	public GameSaveData SaveData { get { return _saveData; } set { _saveData = value; } }
	
	// 리소스 매니저 추가
	private GameResourceManager _gameResourceManager = new GameResourceManager();
	public int Gold
	{
		get { return _gameResourceManager.GetResource(ResourceType.Gold); }
		private set
		{
			_gameResourceManager.SetResource(ResourceType.Gold, value);
			_saveData.Gold = value; // 세이브 데이터 동기화
		}
	}

	public int Glass
	{
		get { return _gameResourceManager.GetResource(ResourceType.Glass); }
		private set
		{
			_gameResourceManager.SetResource(ResourceType.Glass, value);
			_saveData.Glass = value; // 세이브 데이터 동기화
		}
	}

	// 통합 리소스 관리 메서드들
	public void AddResource(ResourceType type, int amount)
	{
		_gameResourceManager.AddResource(type, amount);
		SyncSaveData(type);
	}

	public bool SubtractResource(ResourceType type, int amount)
	{
		bool result = _gameResourceManager.SubtractResource(type, amount);
		if (result) SyncSaveData(type);
		return result;
	}

	public void SetResource(ResourceType type, int amount)
	{
		_gameResourceManager.SetResource(type, amount);
		SyncSaveData(type);
	}

	public int GetResource(ResourceType type)
	{
		return _gameResourceManager.GetResource(type);
	}

	public bool HasEnoughResource(ResourceType type, int amount)
	{
		return _gameResourceManager.HasEnoughResource(type, amount);
	}

	// 세이브 데이터 동기화
	private void SyncSaveData(ResourceType type)
	{
		switch (type)
		{
			case ResourceType.Gold:
				_saveData.Gold = Gold;
				break;
			case ResourceType.Glass:
				_saveData.Glass = Glass;
				break;
		}
	}

	// 기존 호환성을 위한 래퍼 메서드들
	public void AddGold(int amount) 
	{
		int oldGold = Gold;
		AddResource(ResourceType.Gold, amount);
		
		// 골드 추가 애니메이션을 위한 이벤트 발행
		Managers.PublishAction(ActionType.UI_AnimateGoldIncrease);
	}

	public bool SubtractGold(int amount) 
	{
		int oldGold = Gold;
		bool result = SubtractResource(ResourceType.Gold, amount);
		
		if (result) // 성공적으로 차감된 경우에만 애니메이션
		{
			// 골드 감소 애니메이션을 위한 이벤트 발행
			Managers.PublishAction(ActionType.UI_AnimateGoldDecrease);
		}
		
		return result;
	}
	public void SetGold(int amount) => SetResource(ResourceType.Gold, amount);
	
	public void AddGlass(int amount) => AddResource(ResourceType.Glass, amount);
	public bool SubtractGlass(int amount) => SubtractResource(ResourceType.Glass, amount);
	public void SetGlass(int amount) => SetResource(ResourceType.Glass, amount);

	// 레시피 제작 가능 여부 확인 (Glass 개수 기반)
	public bool CanCraftRecipe(int requiredGlass = 1)
	{
		return HasEnoughResource(ResourceType.Glass, requiredGlass);
	}

	// 레시피 제작 시 Glass 소모
	public bool CraftRecipe(int requiredGlass = 1)
	{
		if (SubtractResource(ResourceType.Glass, requiredGlass))
		{
			Debug.Log($"[GameManager] 레시피 제작 완료! Glass {requiredGlass}개 소모");
			return true;
		}
		return false;
	}

	#region Sub Systems
	private CustomerCreator _customerCreator = new CustomerCreator();
	public  CustomerCreator CustomerCreator { get { Managers.Game?._customerCreator.SetInfo(); return Managers.Game?._customerCreator; } }

	private List<Item> _items = new List<Item>();
	public IReadOnlyList<Item> Items => _items;
	private Player _player;
	public Player Player => _player;

	// 플레이어 인벤토리 - 완료된 레시피들 (간단한 구조체로 저장)
	[System.Serializable]
	public struct PlayerRecipe
	{
		public int recipeId;
		public string recipeName;
		public string prefabName;
		
		public PlayerRecipe(int id, string name, string prefab)
		{
			recipeId = id;
			recipeName = name;
			prefabName = prefab;
		}
	}
	
	private List<PlayerRecipe> _playerInventory = new List<PlayerRecipe>();
	public IReadOnlyList<PlayerRecipe> PlayerInventory => _playerInventory;

	// 플레이어 인벤토리 관리 메서드들
	public void AddRecipeToInventory(int recipeId, string recipeName, string prefabName)
	{
		var recipe = new PlayerRecipe(recipeId, recipeName, prefabName);
		_playerInventory.Add(recipe);
		Debug.Log($"<color=green>[GameManager]</color> 인벤토리에 레시피 추가: {recipeName} (ID: {recipeId})");
	}
	
	public bool RemoveRecipeFromInventory(int recipeId)
	{
		for (int i = 0; i < _playerInventory.Count; i++)
		{
			if (_playerInventory[i].recipeId == recipeId)
			{
				var removedRecipe = _playerInventory[i];
				_playerInventory.RemoveAt(i);
				Debug.Log($"<color=orange>[GameManager]</color> 인벤토리에서 레시피 제거: {removedRecipe.recipeName} (ID: {recipeId})");
				return true;
			}
		}
		return false;
	}
	
	public bool HasRecipe(int recipeId)
	{
		return _playerInventory.Any(recipe => recipe.recipeId == recipeId);
	}
	
	public PlayerRecipe? GetRecipe(int recipeId)
	{
		for (int i = 0; i < _playerInventory.Count; i++)
		{
			if (_playerInventory[i].recipeId == recipeId)
				return _playerInventory[i];
		}
		return null;
	}
	#endregion
	public GameManager()
	{
		Debug.Log("<color=yellow>[GameManager]</color> 생성됨");
		
		// 리소스 매니저 초기화 및 UI 콜백 설정
		_gameResourceManager.InitializeResource(ResourceType.Gold, 0, () => {
			(Managers.UI.SceneUI as UI_GameScene)?.RefreshGoldText();
		});
		
		_gameResourceManager.InitializeResource(ResourceType.Glass, 0, () => {
			// TODO: UI_GameScene에 RefreshGlassText() 메서드 구현 필요
			// (Managers.UI.SceneUI as UI_GameScene)?.RefreshGlassText();
		});
	}
 	#region Move
	private Vector2 _moveDir;
    public Vector2 MoveDir
    {
        get { return _moveDir; }
        set
        {
            _moveDir = value;
            //Debug.Log($"[GameManager] MoveDir: {_moveDir}"); 미안!
            _player?.Move(_moveDir); // Player.cs의 Move 함수 호출
            Managers.PublishAction(ActionType.MoveDirChanged);
        }
    }
    private Define.EJoystickState _joystickState;
    public Define.EJoystickState JoystickState
    {
        get { return _joystickState; }
        set
        {
            _joystickState = value;
            Debug.Log($"[GameManager] JoystickState: {_joystickState}");
            Managers.PublishAction(ActionType.JoystickStateChanged);
        }
    }
    #endregion
    public void SetPlayer(Player player)
    {
        _player = player;
    }
    public void RegisterItem(Item item)
    {
        if (!_items.Contains(item))
            _items.Add(item);

        // ObjectType 또는 타입 체크로 분기
        switch (item.ObjectType)
        {
            case Define.EObjectType.Table:
                Managers.Game.CustomerCreator.TableManager.RegisterTable(item as Table);
                break;
            case Define.EObjectType.Chair:
                // ChairManager.RegisterChair(item as Chair); // 필요시
                break;
            // ... 다른 타입도 필요시 추가
        }
    }

    public void UnregisterItem(Item item)
    {
        if (_items.Contains(item))
        {
            _items.Remove(item);
			Debug.Log($"[UnregisterItem] _items.Count: {_items.Count}");
        }

        // ObjectType 또는 타입 체크로 분기
        switch (item.ObjectType)
        {
            case Define.EObjectType.Table:
                Managers.Game.CustomerCreator.TableManager.UnregisterTable(item as Table);
                break;
            case Define.EObjectType.Chair:
                // ChairManager.UnregisterChair(item as Chair); // 필요시
                break;
            // ... 다른 타입도 필요시 추가
        }
    }
  
 


	#region Save & Load	
	public string Path { get { return Application.persistentDataPath + "/SaveData.json"; } }

	public void InitGame()
	{
		if (File.Exists(Path))
			return;

	}

	public void SaveGame()
	{
		string jsonStr = JsonUtility.ToJson(Managers.Game.SaveData);
		File.WriteAllText(Path, jsonStr);
		Debug.Log($"Save Game Completed : {Path}");
	}

	public bool LoadGame()
	{
		if (File.Exists(Path) == false)
			return false;

		string fileStr = File.ReadAllText(Path);
		GameSaveData data = JsonUtility.FromJson<GameSaveData>(fileStr);

		if (data != null)
			Managers.Game.SaveData = data;

		Debug.Log($"Save Game Loaded : {Path}");
		return true;
	}
	#endregion
}

// 골드 애니메이션 데이터 클래스
[System.Serializable]
public class GoldAnimationData
{
	public int oldAmount;
	public int newAmount;
	public int addedAmount;
}