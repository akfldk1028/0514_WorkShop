/*
 * 매니저 시스템 (Managers)
 * 
 * 역할:
 * 1. 모든 매니저 클래스의 통합 접점 - 싱글톤 패턴으로 구현된 중앙 관리 시스템
 * 2. 게임 전체에서 사용되는 핵심 매니저들에 대한 전역 접근 제공
 * 3. 두 가지 주요 카테고리로 매니저 구분:
 *    - Core: 데이터, 풀, 리소스, 씬, UI 등 게임 시스템 기본 기능 관리
 *    - Contents: 게임, 오브젝트, 맵 등 게임 콘텐츠 관련 기능 관리
 * 4. 씬 전환 시에도 유지되는 영구적 게임 오브젝트로 구현 (DontDestroyOnLoad)
 * 5. 게임 초기화 및 매니저 인스턴스 생성 자동화
 * 6. 모든 매니저에 쉽게 접근할 수 있는 정적 프로퍼티 제공
 * 7. 다른 시스템에서 Managers.시스템명 형태로 쉽게 접근 가능
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Managers : MonoBehaviour
{
	public static bool Initialized { get; set; } = false;

	private static Managers s_instance;
	private static Managers Instance { get { Init(); return s_instance; } }

	#region Contents
	private GameManager _game = new GameManager();
	private GameManagerDK _game_dk = new GameManagerDK();
    private InputManager _input = new InputManager(); // 추가된 부분
	private ObjectManager _object = new ObjectManager();
	private MapManager _map = new MapManager();

	public static GameManager Game { get { return Instance?._game; } }
	public static GameManagerDK Game_DK { get { return Instance?._game_dk; } }

	public static ObjectManager Object { get { return Instance?._object; } }
	public static MapManager Map { get { return Instance?._map; } }
	public static InputManager Input { get { return Instance?._input; } } // 추가된 부분
	#endregion

	#region Core
	private DataManager _data = new DataManager();
	private PoolManager _pool = new PoolManager();
	private ResourceManager _resource = new ResourceManager();
	private SceneManagerEx _scene = new SceneManagerEx();
	private UIManager _ui = new UIManager();

	public static DataManager Data { get { return Instance?._data; } }
	public static PoolManager Pool { get { return Instance?._pool; } }
	public static ResourceManager Resource { get { return Instance?._resource; } }
	public static SceneManagerEx Scene { get { return Instance?._scene; } }
	public static UIManager UI { get { return Instance?._ui; } }


	
	#endregion
    // MonoBehaviourHandler 추가
	// Managers.cs 수정
	public static Action UpdateHandler;  // static으로 변경
	public static Action LateUpdateHandler;  // static으로 변경

	private void Update()
	{
		UpdateHandler?.Invoke();
	}

	private void LateUpdate()
	{
		LateUpdateHandler?.Invoke();
	}

	public static void Init()
	{
		Debug.Log("<color=yellow>[Managers]</color> Init");
		if (s_instance == null && Initialized == false)
		{
			Initialized = true;

			GameObject go = GameObject.Find("@Managers");
			if (go == null)
			{
				go = new GameObject { name = "@Managers" };
				go.AddComponent<Managers>();
			}

			DontDestroyOnLoad(go);

			// 초기화
			s_instance = go.GetComponent<Managers>();
			            // MonoBehaviourHandler.Instance.ToString();

		}
	}

}
