using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
// 1️⃣ 게임 시작 시 (1번만)
// 게임 시작
// ↓
// Managers 생성
// ↓
// InputManager 생성
// ↓ 
// InputManager가 "나 Update 받을게!" 하고 구독 등록
// (아직 아무 메시지도 안 옴)
// 2️⃣ 게임 실행 중 (매 프레임)
// Unity가 Managers.Update() 호출
// ↓
// Managers가 "Update 메시지 발행!"
// ↓
// 구독해둔 InputManager가 "아, 내가 기다리던 메시지다!" 
// ↓
// InputManager.OnUpdate() 실행
/// </summary>
public class MessageManager<T> : IMessageChannel<T>
{
    private const string LOG_PREFIX = "[MessageChannel]";
    readonly List<Action<T>> m_MessageHandlers = new List<Action<T>>();

    /// <summary>
    /// 구독/해제 대기 중인 핸들러를 관리하는 딕셔너리
    /// - Key: 메시지 핸들러
    /// - Value: true=구독 예정, false=구독 해제 예정
    /// 메시지 처리 도중 구독/해제 시 발생할 수 있는 문제를 방지
    /// </summary>
    readonly Dictionary<Action<T>, bool> m_PendingHandlers = new Dictionary<Action<T>, bool>();

    public bool IsDisposed { get; private set; } = false;

    /// <summary>
    /// 채널의 리소스를 정리하고 더 이상의 메시지 처리를 중단
    /// </summary>
    public virtual void Dispose()
    {
        if (!IsDisposed)
        {
            Debug.Log($"{LOG_PREFIX} Disposing channel for {typeof(T).Name}, Handlers: {m_MessageHandlers.Count}, Pending: {m_PendingHandlers.Count}");
            IsDisposed = true;
            m_MessageHandlers.Clear();
            m_PendingHandlers.Clear();
        }
    }

    /// <summary>
    /// 모든 구독자에게 메시지를 발행
    /// </summary>
    /// <param name="message">전달할 메시지</param>
    public virtual void Publish(T message)
    {
        // Pending 핸들러 처리
        if (m_PendingHandlers.Count > 0)
        {
            var pendingList = m_PendingHandlers.ToList(); // 복사본 생성
            m_PendingHandlers.Clear();
            
            foreach (var kvp in pendingList)
            {
                if (kvp.Value) // 구독 추가
                {
                    if (!m_MessageHandlers.Contains(kvp.Key))
                    {
                        m_MessageHandlers.Add(kvp.Key);
                        Debug.Log($"{LOG_PREFIX} 핸들러 추가됨 for {typeof(T).Name}");
                    }
                }
                else // 구독 해제
                {
                    m_MessageHandlers.Remove(kvp.Key);
                    Debug.Log($"{LOG_PREFIX} 핸들러 제거됨 for {typeof(T).Name}");
                }
            }
        }

        // 모든 핸들러에게 메시지 전달
        var handlers = m_MessageHandlers.ToList(); // 복사본으로 작업 (반복 중 수정 방지)
        foreach (var messageHandler in handlers)
        {
            if (messageHandler != null)
            {
                try
                {
                    messageHandler.Invoke(message);
                }
                catch (Exception e)
                {
                    Debug.LogError($"{LOG_PREFIX} 핸들러 실행 중 오류 발생: {e}");
                }
            }
        }
    }
    /// <summary>
    /// 액션을 발행하는 헬퍼 메서드
    /// </summary>
  
    /// <summary>
    /// 새로운 구독자 등록
    /// </summary>
    /// <param name="handler">메시지를 처리할 콜백 함수</param>
    /// <returns>구독 해제에 사용할 수 있는 IDisposable 객체</returns>
    // public virtual IDisposable Subscribe(Action<T> handler)
    // {
    //     Assert.IsTrue(!IsSubscribed(handler), "Attempting to subscribe with the same handler more than once");

    //     if (m_PendingHandlers.ContainsKey(handler))
    //     {
    //         if (!m_PendingHandlers[handler])
    //         {
    //             m_PendingHandlers.Remove(handler);
    //         }
    //     }
    //     else
    //     {
    //         m_PendingHandlers[handler] = true;
    //     }

    //     Debug.Log($"{LOG_PREFIX} 구독 등록됨 (pending) for {typeof(T).Name}");

    //     var subscription = new DisposableSubscription<T>(this, handler);
    //     return subscription;
    // }
    
    public virtual IDisposable Subscribe(Action<T> handler)
{
    Assert.IsTrue(!IsSubscribed(handler), "Attempting to subscribe with the same handler more than once");

    // 이 부분을 변경: m_PendingHandlers 대신 m_MessageHandlers에 바로 추가
    if (!m_MessageHandlers.Contains(handler))
    {
        m_MessageHandlers.Add(handler);
    }

    var subscription = new DisposableSubscription<T>(this, handler);
    return subscription;
}


    /// <summary>
    /// 구독자 제거
    /// </summary>
    /// <param name="handler">제거할 메시지 핸들러</param>
    // public void Unsubscribe(Action<T> handler)
    // {
    //     Debug.Log($"{LOG_PREFIX} Unsubscribing from {typeof(T).Name}");
    //     if (IsSubscribed(handler))
    //     {
    //         if (m_PendingHandlers.ContainsKey(handler))
    //         {
    //             if (m_PendingHandlers[handler])
    //             {
    //                 Debug.Log($"{LOG_PREFIX} Removing pending subscribe for {typeof(T).Name}");
    //                 m_PendingHandlers.Remove(handler);
    //             }
    //         }
    //         else
    //         {
    //             Debug.Log($"{LOG_PREFIX} Adding pending unsubscribe for {typeof(T).Name}");
    //             m_PendingHandlers[handler] = false;
    //         }
    //     }
    // }
public void Unsubscribe(Action<T> handler)
{
    Debug.Log($"{LOG_PREFIX} Unsubscribing from {typeof(T).Name}");
    
    // m_MessageHandlers에서 바로 제거
    if (m_MessageHandlers.Contains(handler))
    {
        m_MessageHandlers.Remove(handler);
        Debug.Log($"{LOG_PREFIX} 핸들러 제거됨 from {typeof(T).Name}");
    }
}


    /// <summary>
    /// 특정 핸들러가 현재 구독 중인지 확인
    /// </summary>
    /// <param name="handler">확인할 메시지 핸들러</param>
    /// <returns>구독 중이면 true, 아니면 false</returns>
    // bool IsSubscribed(Action<T> handler)
    // {
    //     var isPendingRemoval = m_PendingHandlers.ContainsKey(handler) && !m_PendingHandlers[handler];
    //     var isPendingAdding = m_PendingHandlers.ContainsKey(handler) && m_PendingHandlers[handler];
    //     return m_MessageHandlers.Contains(handler) && !isPendingRemoval || isPendingAdding;
    // }
    bool IsSubscribed(Action<T> handler)
{
    return m_MessageHandlers.Contains(handler);
}
}


