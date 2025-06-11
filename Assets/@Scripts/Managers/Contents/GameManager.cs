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

}
public class GameManager
{

	GameSaveData _saveData = new GameSaveData();
	public GameSaveData SaveData { get { return _saveData; } set { _saveData = value; } }
	public int Gold
	{
		get { return _saveData.Gold; }
		private set
		{
			_saveData.Gold = value;
			(Managers.UI.SceneUI as UI_GameScene)?.RefreshGoldText();
		}
	}

	// Gold 관련 메서드들
	public void AddGold(int amount)
	{
		if (amount > 0)
		{
			Gold += amount;
			Debug.Log($"[GameManager] Gold 증가: +{amount}, 현재 Gold: {Gold}");
		}
	}

	public bool SubtractGold(int amount)
	{
		if (amount > 0 && Gold >= amount)
		{
			Gold -= amount;
			Debug.Log($"[GameManager] Gold 감소: -{amount}, 현재 Gold: {Gold}");
			return true;
		}
		Debug.Log($"[GameManager] Gold 부족: 필요 {amount}, 현재 {Gold}");
		return false;
	}

	public void SetGold(int amount)
	{
		Gold = Mathf.Max(0, amount);
		Debug.Log($"[GameManager] Gold 설정: {Gold}");
	}

    #region Sub Systems
    private CustomerCreator _customerCreator = new CustomerCreator();
    public  CustomerCreator CustomerCreator { get { Managers.Game?._customerCreator.SetInfo(); return Managers.Game?._customerCreator; } }

    private List<Item> _items = new List<Item>();
    public IReadOnlyList<Item> Items => _items;
    private Player _player;
    public Player Player => _player;

	#endregion
	public GameManager()
	{
		Debug.Log("<color=yellow>[GameManager]</color> 생성됨");



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