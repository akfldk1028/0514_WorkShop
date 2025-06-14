/*
 * UI 매니저 (UIManager)
 * 
 * 역할:
 * 1. 게임의 모든 UI 요소 생성, 관리 및 제어
 * 2. 팝업 UI 시스템: 스택 기반 팝업 관리로 순서와 계층 자동 처리
 * 3. 씬 UI 관리: 각 씬마다 필요한 기본 UI 요소 관리
 * 4. UI 캔버스 설정: 렌더링 모드, 정렬 순서, 스케일 등 자동 구성
 * 5. UI 프리팹 캐싱: 자주 사용되는 UI 요소 미리 로드하여 성능 최적화
 * 6. 다양한 UI 생성 메서드 제공:
 *    - 월드 스페이스 UI
 *    - 서브 아이템 UI
 *    - 베이스 UI
 *    - 씬 UI
 *    - 팝업 UI
 * 7. UI Root 자동 생성 및 관리
 * 8. 리소스 매니저와 연동한 UI 프리팹 로딩 시스템
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIManager
{
	public UIManager()
	{
		Debug.Log("<color=orange>[UIManager]</color> 생성됨");
	}

	private int _order = 10;

	private Dictionary<string, UI_Popup> _popups = new Dictionary<string, UI_Popup>();
	private Stack<UI_Popup> _popupStack = new Stack<UI_Popup>();

	private UI_Scene _sceneUI = null;
	public UI_Scene SceneUI
	{
		set { _sceneUI = value; }
		get { return _sceneUI; }
	}

	public GameObject Root
	{
		get
		{
			GameObject root = GameObject.Find("@UI_Root");
			if (root == null)
				root = new GameObject { name = "@UI_Root" };
			return root;
		}
	}

	public void CacheAllPopups()
	{
		var list = AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(assembly => assembly.GetTypes())
			.Where(type => type.IsSubclassOf(typeof(UI_Popup)));

		foreach (Type type in list)
		{
			CachePopupUI(type);
		}

		// ShowPopupUI<UI_WaypointPopup>();
		
		CloseAllPopupUI();
	}

	public void SetCanvas(GameObject go, bool sort = true, int sortOrder = 0)
	{
		Canvas canvas = Util.GetOrAddComponent<Canvas>(go);
		if (canvas == null)
		{
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.overrideSorting = true;
		}

		CanvasScaler cs = go.GetOrAddComponent<CanvasScaler>();
		if (cs != null)
		{
			cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			cs.referenceResolution = new Vector2(1080, 1920);
		}

		go.GetOrAddComponent<GraphicRaycaster>();

		if (sort)
		{
			canvas.sortingOrder = _order;
			_order++;
		}
		else
		{
			canvas.sortingOrder = sortOrder;
		}
	}

	public T GetSceneUI<T>() where T : UI_Base
	{
		return _sceneUI as T;
	}

	public T MakeWorldSpaceUI<T>(Transform parent = null, string name = null) where T : UI_Base
	{
		if (string.IsNullOrEmpty(name))
			name = typeof(T).Name;

		GameObject go = Managers.Resource.Instantiate($"{name}");
		if (parent != null)
			go.transform.SetParent(parent);

		Canvas canvas = go.GetOrAddComponent<Canvas>();
		canvas.renderMode = RenderMode.WorldSpace;
		canvas.worldCamera = Camera.main;

		return Util.GetOrAddComponent<T>(go);
	}

	public T MakeSubItem<T>(Transform parent = null, string name = null, bool pooling = true) where T : UI_Base
	{
		if (string.IsNullOrEmpty(name))
			name = typeof(T).Name;

		GameObject go = Managers.Resource.Instantiate(name, parent, pooling);
		go.transform.SetParent(parent);

		return Util.GetOrAddComponent<T>(go);
	}

	public T ShowBaseUI<T>(string name = null) where T : UI_Base
	{
		if (string.IsNullOrEmpty(name))
			name = typeof(T).Name;

		GameObject go = Managers.Resource.Instantiate(name);
		T baseUI = Util.GetOrAddComponent<T>(go);

		go.transform.SetParent(Root.transform);

		return baseUI;
	}

	public T ShowSceneUI<T>(string name = null) where T : UI_Scene
	{
		if (string.IsNullOrEmpty(name))
			name = typeof(T).Name;

		GameObject go = Managers.Resource.Instantiate(name);
		T sceneUI = Util.GetOrAddComponent<T>(go);
		_sceneUI = sceneUI;

		go.transform.SetParent(Root.transform);

		return sceneUI;
	}

	public void CloseSceneUI()
	{
		if (_sceneUI != null)
		{
			GameObject go = _sceneUI.gameObject;
			Managers.Resource.Destroy(go);
			_sceneUI = null;
		}
	}

	public void HideSceneUI()
	{
		if (_sceneUI != null)
		{
			_sceneUI.gameObject.SetActive(false);
		}
	}

	public void ShowSceneUI()
	{
		if (_sceneUI != null)
		{
			_sceneUI.gameObject.SetActive(true);
		}
	}

	public void CachePopupUI(Type type)
	{
		string name = type.Name;

		if (_popups.TryGetValue(name, out UI_Popup popup) == false)
		{
			GameObject go = Managers.Resource.Instantiate(name);
			popup = go.GetComponent<UI_Popup>();
			_popups[name] = popup;
		}

		_popupStack.Push(popup);
	}

	public T ShowPopupUI<T>(string name = null) where T : UI_Popup
	{
		if (string.IsNullOrEmpty(name))
			name = typeof(T).Name;

		if (_popups.TryGetValue(name, out UI_Popup popup) == false)
		{
			GameObject go = Managers.Resource.Instantiate(name);
			popup = Util.GetOrAddComponent<T>(go);
			_popups[name] = popup;
		}

		_popupStack.Push(popup);

		popup.transform.SetParent(Root.transform);
		popup.gameObject.SetActive(true);

		return popup as T;
	}

	public void ClosePopupUI(UI_Popup popup)
	{
		if (_popupStack.Count == 0)
			return;

		if (_popupStack.Peek() != popup)
		{
			Debug.Log("Close Popup Failed!");
			return;
		}

		ClosePopupUI();
	}

	public void ClosePopupUI()
	{
		if (_popupStack.Count == 0)
			return;

		UI_Popup popup = _popupStack.Pop();

		popup.gameObject.SetActive(false);
		//Managers.Resource.Destroy(popup.gameObject);

		_order--;
	}

	public void CloseAllPopupUI()
	{
		while (_popupStack.Count > 0)
			ClosePopupUI();
	}

	public int GetPopupCount()
	{
		return _popupStack.Count;
	}

	public void Clear()
	{
		CloseAllPopupUI();
		_sceneUI = null;
	}

	public static void BindEvent(GameObject go, Action<PointerEventData> action = null, Define.EUIEvent type = Define.EUIEvent.Click)
	{
		UI_EventHandler evt = Util.GetOrAddComponent<UI_EventHandler>(go);

		switch (type)
		{
			case Define.EUIEvent.Click:
				evt.OnClickHandler -= action;
				evt.OnClickHandler += action;
				break;
			case Define.EUIEvent.PointerDown:
				evt.OnPointerDownHandler -= action;
				evt.OnPointerDownHandler += action;
				break;
			case Define.EUIEvent.PointerUp:
				evt.OnPointerUpHandler -= action;
				evt.OnPointerUpHandler += action;
				break;
			case Define.EUIEvent.Drag:
				evt.OnDragHandler -= action;
				evt.OnDragHandler += action;
				break;
		}
	}

	// 향후 UI 팝업 관리, UI 사운드 재생 등의 메서드 추가 예정
}
