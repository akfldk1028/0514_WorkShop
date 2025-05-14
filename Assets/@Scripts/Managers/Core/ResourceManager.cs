/*
 * 리소스 매니저 (ResourceManager)
 * 
 * 역할:
 * 1. 게임에서 사용되는 모든 리소스(프리팹, 텍스처, 오디오 등)의 로드 및 관리
 * 2. 리소스 캐싱 시스템 구현으로 중복 로드 방지 및 메모리 효율성 향상
 * 3. Unity Addressable Asset 시스템 활용하여 비동기 리소스 로딩 지원
 * 4. 게임 오브젝트 생성(Instantiate) 및 제거(Destroy) 기능 통합 관리
 * 5. 풀 매니저와 연동하여 재사용 가능한 오브젝트의 효율적인 관리
 * 6. 동적 리소스 로딩 및 언로딩을 통한 메모리 최적화
 * 7. 비동기 로딩 완료 콜백 처리를 통한 유연한 리소스 관리
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

public class ResourceManager
{
	public ResourceManager()
	{
		Debug.Log("<color=cyan>[ResourceManager]</color> 생성됨");
	}

	private Dictionary<string, UnityEngine.Object> _resources = new Dictionary<string, UnityEngine.Object>();
	private Dictionary<string, AsyncOperationHandle> _handles = new Dictionary<string, AsyncOperationHandle>();

	#region Load Resource
	public T Load<T>(string key) where T : Object
	{
		if (_resources.TryGetValue(key, out Object resource))
			return resource as T;

		if (typeof(T) == typeof(Sprite) && key.Contains(".sprite") == false)
		{
			if (_resources.TryGetValue($"{key}.sprite", out resource))
				return resource as T;
		}

		return null;
	}

	public GameObject Instantiate(string key, Transform parent = null, bool pooling = false)
	{
		GameObject prefab = Load<GameObject>(key);
		if (prefab == null)
		{
			Debug.LogError($"Failed to load prefab : {key}");
			return null;
		}

		if (pooling)
			return Managers.Pool.Pop(prefab);

		GameObject go = Object.Instantiate(prefab, parent);
		go.name = prefab.name;

		return go;
	}

	public void Destroy(GameObject go)
	{
		if (go == null)
			return;

		if (Managers.Pool.Push(go))
			return;

		Object.Destroy(go);
	}
	#endregion

	#region Addressable
	private void LoadAsync<T>(string key, Action<T> callback = null) where T : UnityEngine.Object
	{
		// Cache
		if (_resources.TryGetValue(key, out Object resource))
		{
			callback?.Invoke(resource as T);
			return;
		}

		string loadKey = key;
		if (key.Contains(".sprite"))
			loadKey = $"{key}[{key.Replace(".sprite", "")}]";

		var asyncOperation = Addressables.LoadAssetAsync<T>(loadKey);
		asyncOperation.Completed += (op) =>
		{
			_resources.Add(key, op.Result);
			_handles.Add(key, asyncOperation);
			callback?.Invoke(op.Result);
		};
	}

	public void LoadAllAsync<T>(string label, Action<string, int, int> callback) where T : UnityEngine.Object
	{
		var opHandle = Addressables.LoadResourceLocationsAsync(label, typeof(T));
		opHandle.Completed += (op) =>
		{
			int loadCount = 0;
			int totalCount = op.Result.Count;

			foreach (var result in op.Result)
			{
				if (result.PrimaryKey.Contains(".sprite"))
				{
					LoadAsync<Sprite>(result.PrimaryKey, (obj) =>
					{
						loadCount++;
						callback?.Invoke(result.PrimaryKey, loadCount, totalCount);
					});
				}
				else
				{
					LoadAsync<T>(result.PrimaryKey, (obj) =>
					{
						loadCount++;
						callback?.Invoke(result.PrimaryKey, loadCount, totalCount);
					});
				}
			}
		};
	}

	public void Clear()
	{
		_resources.Clear();

		foreach (var handle in _handles)
			Addressables.Release(handle);

		_handles.Clear();
	}
	#endregion
}
