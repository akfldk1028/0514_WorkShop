// ActionType.cs
public enum ActionType
{
    // Unity 생명주기 이벤트
    Managers_Update,
    Managers_LateUpdate,
    Managers_FixedUpdate,
    Customer_Spawned,
    Customer_WaitingForTable,
    Customer_MovedToTable,
    Customer_Seated,         // 손님이 자리에 앉음

    Customer_TableFullyOccupied, // 테이블 만석
    Customer_Ordered,        // 손님이 주문함
    Customer_WaitingForFood,
    Customer_ReceivedFood,   // 손님이 음식을 받음
    Customer_StartedEating,  // 손님이 먹기 시작함
    Customer_FinishedEating, // 손님이 식사를 마침
    Customer_Left,

    Chair_OccupiedChanged, // 의자 착석 상태 변경
    Chair_Changed,
    // 게임 상태 이벤트
    GameStart,
    GamePause,
    GameResume,
    GameEnd,
    
    // 씬 관련 이벤트
    SceneLoaded,
    SceneUnloaded,
    
    // 플레이어 이벤트
    Player_Spawned,
    PlayerDeath,
    PlayerLevelUp,
    
    // UI 이벤트
    UIOpen,
    UIClose,
    
    MoveDirChanged,
    JoystickStateChanged,

    // 기타 커스텀 이벤트
    CustomEvent
}