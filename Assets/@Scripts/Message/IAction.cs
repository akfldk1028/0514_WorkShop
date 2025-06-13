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
    GameScene_UpdateOrderText,
    GameScene_AddCompletedRecipe,  // 완료된 레시피 아이콘을 UI에 추가
    GameScene_RemoveCompletedRecipe, // 완료된 레시피 아이콘을 UI에서 제거
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
    Player_InteractKey,    // K키 인터랙션 이벤트
    PlayerDeath,
    PlayerLevelUp,
    
    // UI 이벤트
    UIOpen,
    UIClose,
    UI_StartRhythmGame,    // 리듬게임 시작 버튼 클릭
    UI_UpdateRecipeText,   // 레시피 텍스트 업데이트
    UI_UpdateOrderText,    // 주문 텍스트 업데이트
    UI_UpdateGlassText,    // 유리잔 개수 텍스트 업데이트
    UI_AnimateGoldIncrease, // 골드 증가 애니메이션
    UI_AnimateGoldDecrease, // 골드 감소 애니메이션 💸

    
    MoveDirChanged,
    JoystickStateChanged,

    // 카메라 뷰 이벤트
    Camera_TopViewActivated,    // 탑뷰로 전환됨
    Camera_BackViewActivated,   // 백뷰로 전환됨

    // 기타 커스텀 이벤트
    CustomEvent
}