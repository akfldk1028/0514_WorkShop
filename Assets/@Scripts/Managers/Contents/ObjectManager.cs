/*
 * 오브젝트 매니저 (ObjectManager)
 * 
 * 역할:
 * 1. 게임 내 모든 동적 오브젝트(영웅, 몬스터 등) 생성 및 관리
 * 2. 오브젝트 스폰 및 제거 시스템 제공
 * 3. 오브젝트 유형별 컬렉션 관리 (영웅, 몬스터 등)
 * 4. 계층 구조 관리 - 오브젝트 타입별 루트 오브젝트 자동 생성 및 정리
 * 5. 템플릿 ID 기반 오브젝트 생성 및 초기화
 * 6. 리소스 매니저 및 풀 매니저와 연동하여 효율적인 오브젝트 생명주기 관리
 * 7. 오브젝트의 정보 설정 및 업데이트 처리
 * 8. Managers 클래스를 통해 전역적으로 접근 가능한 오브젝트 관리 시스템 제공
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using static Define;

public class ObjectManager
{
	public ObjectManager()
	{
		Debug.Log("<color=cyan>[ObjectManager]</color> 생성됨");
	}

	// public HashSet<Hero> Heroes { get; } = new HashSet<Hero>();
	// public HashSet<Monster> Monsters { get; } = new HashSet<Monster>();
	public HashSet<Customer> Customers { get; } = new HashSet<Customer>();
	public HashSet<Player> Players { get; } = new HashSet<Player>();

	#region Roots
	public Transform GetRootTransform(string name)
	{
		GameObject root = GameObject.Find(name);
		if (root == null)
			root = new GameObject { name = name };

		return root.transform;
	}

	public Transform HeroRoot { get { return GetRootTransform("@Heroes"); } }
	public Transform MonsterRoot { get { return GetRootTransform("@Monsters"); } }
	public Transform CustomerRoot { get { return GetRootTransform("@Customers"); } }
	public Transform PlayerRoot { get { return GetRootTransform("@Players"); } }
	#endregion

	
	public GameObject SpawnGameObject(Vector3 position, string prefabName)
	{
		GameObject go = Managers.Resource.Instantiate(prefabName, pooling: true);
		go.transform.position = position;
		return go;
	}



	public T Spawn<T>(Vector3 position, int templateID, bool pooling = false) where T : BaseObject
	{
		string prefabName = typeof(T).Name;
		Debug.Log($"<color=magenta>[ObjectManager]</color> Spawn {prefabName} at {position}");


		GameObject go = Managers.Resource.Instantiate(prefabName, position: position, pooling: pooling);
		
		Debug.Log($"<color=magenta>[ObjectManager]</color> Spawn {prefabName} at {position}");
		go.name = prefabName;
		go.transform.position = position;



		BaseObject obj = go.GetComponent<BaseObject>();
	    Debug.Log($"<color=magenta>[ObjectManager]</color> Spawn {prefabName} at {obj.ObjectType}");

		
		if (obj.ObjectType == EObjectType.Customer)
		{
			Data.CustomerData data = Managers.Data.CustomerDic[templateID];
			ClientCustomer clientCustomer = Managers.Resource.Load<ClientCustomer>(data.ClientPrefab);
			Customer customer = go.GetComponent<Customer>();
			customer.SetInfo(templateID, clientCustomer);
			obj.transform.parent = CustomerRoot;
			Customers.Add(customer);

		}
        else if (obj.ObjectType == EObjectType.Player)
        {
            Debug.Log($"<color=magenta>[ObjectManager]</color> Spawn {prefabName} at {templateID}");
            Debug.Log($"<color=magenta>[ObjectManager]</color> Spawn {prefabName} at { Managers.Data.PlayerDic[templateID]}");

            Data.PlayerData data = Managers.Data.PlayerDic[templateID];
			ClientPlayer clientPlayer = Managers.Resource.Load<ClientPlayer>(data.ClientPrefab);
            Player player = go.GetComponent<Player>();
            player.SetInfo(templateID, clientPlayer);
            obj.transform.parent = PlayerRoot;
            Players.Add(player);
        }



		return obj as T;
	}

	public void Despawn<T>(T obj) where T : BaseObject
	{
		EObjectType objectType = obj.ObjectType;

		if (obj.ObjectType == EObjectType.Hero)
		{
			// Hero hero = obj.GetComponent<Hero>();
			// Heroes.Remove(hero);
		}
		else if (obj.ObjectType == EObjectType.Monster)
		{
			// Monster monster = obj.GetComponent<Monster>();
			// Monsters.Remove(monster);
		}
		else if (obj.ObjectType == EObjectType.Customer)
		{
			Customer customer = obj.GetComponent<Customer>();
			Customers.Remove(customer);
			Debug.Log($"<color=orange>[ObjectManager]</color> Customer {customer.name} 디스폰됨. 남은 고객 수: {Customers.Count}");
		}
		else if (obj.ObjectType == EObjectType.Player)
		{
			Player player = obj.GetComponent<Player>();
			Players.Remove(player);
			Debug.Log($"<color=cyan>[ObjectManager]</color> Player {player.name} 디스폰됨");
		}

		Managers.Resource.Destroy(obj.gameObject);
	}

}
