using System;
using UnityEngine;

// 싱글톤으로 구현된 MonoBehaviour 핸들러
// 매니저 시스템에서 Update 이벤트를 사용하기 위한 브릿지
public class MonoBehaviourHandler : MonoBehaviour
{
    private static MonoBehaviourHandler _instance;
    public static MonoBehaviourHandler Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("@MonoBehaviourHandler");
                _instance = go.AddComponent<MonoBehaviourHandler>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    
    public Action UpdateHandler;
    public Action LateUpdateHandler;
    
    void Update()
    {
        UpdateHandler?.Invoke();
    }
    
    void LateUpdate()
    {
        LateUpdateHandler?.Invoke();
    }
}