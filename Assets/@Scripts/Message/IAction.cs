// ActionType.cs
public enum ActionType
{
    // Unity 생명주기 이벤트
    Managers_Update,
    Managers_LateUpdate,
    Managers_FixedUpdate,
    Customer_Spawned,
    Customer_MovedToTable,
    Customer_OrderComplete,
    Customer_Left,
    // 게임 상태 이벤트
    GameStart,
    GamePause,
    GameResume,
    GameEnd,
    
    // 씬 관련 이벤트
    SceneLoaded,
    SceneUnloaded,
    
    // 플레이어 이벤트
    PlayerSpawn,
    PlayerDeath,
    PlayerLevelUp,
    
    // UI 이벤트
    UIOpen,
    UIClose,
    
    // 기타 커스텀 이벤트
    CustomEvent
}