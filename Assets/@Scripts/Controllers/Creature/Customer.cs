using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static Define;
using System.Collections.Generic;
using TMPro;
using System.Collections; // 코루틴 사용을 위해 추가

public enum ECustomerState
{
    None,
    EnteringRestaurant,
    WaitingForChair,
    WalkingToChair,
    SittingDown,
    Ordering,
    WaitingForFood,
    Eating,
    StandingUp,
    LeavingRestaurant
}

public class Customer : Unit
{
    [Header("Customer Components")]
    public Image earnMoneyImage;
    public Image orderImage;
    private Chair _chair;
    public Chair Chair => _chair;
    public Transform placeToSit;
    public Transform door;
    
    [Header("Settings")]
    public float eatingTime = 10f;
    public AudioClip gainGoldClip;
    
    [Header("Look At Settings")]
    [SerializeField] private float lookAtSpeed = 4f; // 회전 속도
    [SerializeField] private float lookAtPlayerDuration = 2f; // 플레이어를 바라보는 시간
    
    private GameObject modelInstance;
    private Animator modelAnimator;
    
    // 상태 관리
    public Dictionary<Table, List<Food>> orderedFoods = new Dictionary<Table, List<Food>>();
    private ECustomerState _customerState = ECustomerState.None;    
    private float _stateTimer;
    private float _sitTimer = 0f;
    private System.IDisposable _chairChangedSubscription;

    // LookAt 관련 변수들
    private Quaternion originalRotation;
    private bool isLookingAtPlayer = false;
    private bool hasLookedAtPlayer = false;
    
    // 🆕 대기열 시스템 관련 변수들
    private Vector3 _assignedWaitingPosition = Vector3.zero;
    private bool _isInWaitingQueue = false;
    private bool _isInEntryMode = false; // 입장 모드 (문으로 먼저 이동)
    
    [SerializeField]
    public TextMeshProUGUI orderText; // 인스펙터에서 할당 or 코드에서 찾기

    private Item ItemqueueManager;
    public ECustomerState CustomerState
    {
        get => _customerState;
        set
        {
            if (_customerState != value)
            {
                _customerState = value;
                _stateTimer = 0f;
                OnStateEnter(value);
            }
        }
    }


    public override bool Init()
    {
        if (!base.Init())
            return false;

        ObjectType = EObjectType.Customer;
        action = GetComponent<CharacterAction>();
        
        // GameManager.Items에서 가장 가까운 Item을 골라 참조
        ItemqueueManager = Managers.Game.Items
            .Where(item => item != null && item.gameObject != null && item.ObjectType == Define.EObjectType.Chair)
            .OrderBy(item => Vector3.Distance(transform.position, item.transform.position))
            .FirstOrDefault();
         _chairChangedSubscription = Managers.Subscribe(ActionType.Chair_Changed, OnChairChanged);


        Debug.Log("[Customer] ItemqueueManager: " + ItemqueueManager);
        return true;
    }

    public override void SetInfo<T>(int templateID, T client)
    {
        base.SetInfo(templateID, client);
        
        if (action == null)
        {
            Debug.LogWarning("[Customer] action이 null입니다. Unit.Init()에서 초기화되어야 합니다.");
        }

        ClientCustomer clientCustomer = client as ClientCustomer;
        if (clientCustomer?.ModelPrefab != null)
        {
            SetupCustomerModel(clientCustomer);
        }

        // 대기줄에서 바로 시작 - WaitingForChair 상태로 시작
        CustomerState = ECustomerState.WaitingForChair;
    }
    private void OnChairChanged()
    {
        if (CustomerState == ECustomerState.WaitingForChair)
        {
            var found = FindEmptyChair();
            if (found != null)
            {
                _chair = found;
                _chair.Reserve(this);
                placeToSit = found.placeToSit;
                CustomerState = ECustomerState.WalkingToChair;
            }
        }
    }

// (옵션) 오브젝트 파괴 시 구독 해제
private void OnDestroy()
{
    _chairChangedSubscription?.Dispose();
    
    // 혹시 남아있는 모델 인스턴스 정리
    if (modelInstance != null)
    {
        Debug.Log($"<color=orange>[Customer {this.name}] OnDestroy에서 모델 인스턴스 정리: {modelInstance.name}</color>");
        Destroy(modelInstance);
        modelInstance = null;
    }
}
    



    void Update()
    {
        HandleRootMotion();
        UpdateCurrentState();
    }

    private void HandleRootMotion()
    {
        if (modelInstance != null && modelAnimator != null)
        {
            Vector3 deltaPosition = modelInstance.transform.localPosition;
            if (deltaPosition.magnitude > 0.001f)
            {
                transform.position += transform.TransformDirection(deltaPosition);
                modelInstance.transform.localPosition = Vector3.zero;
            }
        }
    }

// OnStateEnter = 상태 시작할 때 딱 한 번만 실행

// 애니메이션 시작
// 목적지 설정
// UI 켜기/끄기
// 초기 설정들

// UpdateCurrentState = 상태가 지속되는 동안 매 프레임 실행

// 조건 체크 (도착했나? 시간 다 됐나?)
// 상태 전환 결정


    private void OnStateEnter(ECustomerState state)
    {
        if (action == null)
        {
            Debug.Log($"[Customer {this.name}] Action_Test 컴포넌트가 null입니다. State: {state}");
            return;
        }

        switch (state)
        {
            case ECustomerState.EnteringRestaurant:
                action.CustomerWalk();
                
                // Agent 상태 확인 및 활성화
                if (agent != null)
                {
                    agent.enabled = true;
                    agent.isStopped = false;
                    agent.speed = 3.5f;
                    
                    // 대기 위치가 할당되었다면 해당 위치로 이동
                    if (_assignedWaitingPosition != Vector3.zero)
                    {
                        agent.SetDestination(_assignedWaitingPosition);
                        Debug.Log($"<color=cyan>[Customer {this.name}]</color> EnteringRestaurant: 할당된 대기 위치로 이동 - {_assignedWaitingPosition}");
                    }
                    else
                    {
                        // 대기 위치가 아직 할당되지 않은 경우 Door 위치로 임시 이동
                        Vector3 restaurantCenter = Managers.Map.DoorPosition;
                        agent.SetDestination(restaurantCenter);
                        Debug.Log($"<color=yellow>[Customer {this.name}]</color> EnteringRestaurant: 대기 위치 미할당, Door로 임시 이동 - {restaurantCenter}");
                    }
                    
                    Debug.Log($"<color=magenta>[Customer {this.name}]</color> Agent 상태 - enabled: {agent.enabled}, isStopped: {agent.isStopped}, speed: {agent.speed}");
                }
                else
                {
                    Debug.LogError($"<color=red>[Customer {this.name}]</color> EnteringRestaurant: Agent가 null입니다!");
                }
                break;

            case ECustomerState.WaitingForChair:
                action.CustomerStandIdle();
                
                // 테이블을 찾으면 대기열에서 제거하고 이동
                var found = FindEmptyChair();
                if (found != null)
                {
                    RemoveFromWaitingQueue(); // 대기열에서 제거
                    _chair = found;
                    _chair.Reserve(this);
                    placeToSit = found.placeToSit;
                    CustomerState = ECustomerState.WalkingToChair;
                }
                
                Managers.PublishAction(ActionType.Customer_WaitingForTable);
                break;

            case ECustomerState.WalkingToChair:
                action.CustomerWalk();
                if(placeToSit != null) agent.SetDestination(placeToSit.position);
                // else Debug.LogError($"[Customer {this.name}] WalkingToChair인데 placeToSit이 null입니다!");
                Managers.PublishAction(ActionType.Customer_MovedToTable);
                break;

            case ECustomerState.SittingDown:
                // Debug.Log($"[Customer {this.name}] SittingDown 상태 진입. _chair is null? {(_chair == null)}");
                action.CustomerSit();
                if (_chair != null)
                {
                    _chair.SeatCustomer(this);
                    transform.position = _chair.placeToSit.position; 
                    transform.rotation = _chair.placeToSit.rotation; 
                    Managers.PublishAction(ActionType.Customer_Seated);

                    if (_chair.table != null && _chair.table.IsFullyOccupied)
                    {
                        Managers.PublishAction(ActionType.Customer_TableFullyOccupied);
                    }

                    // Debug.Log($"[Customer {this.name}] 자리에 앉았으므로 주문 생성을 시도합니다. _chair.table is null? {(_chair?.table == null)}");
                    GenerateOrderFromManager(); 
                }
                else 
                {
                    // Debug.LogError($"[Customer {this.name}] SittingDown 상태이지만 _chair가 null입니다!");
                }
                break;

            case ECustomerState.Ordering:
                action.CustomerOrder();
                
                // 원래 회전값 저장 (의자 방향)
                originalRotation = transform.rotation;
                
                // 플레이어 바라보기 시작
                isLookingAtPlayer = true;
                hasLookedAtPlayer = false;
                
                Debug.Log($"<color=cyan>[Customer {this.name}] 주문 상태 - 플레이어를 바라보기 시작</color>");
                Managers.PublishAction(ActionType.Customer_Ordered);
                break;

            case ECustomerState.WaitingForFood:
                action.CustomerSitIdle();
                // 이 상태는 이제 두 가지 경로로 진입 가능:
                // 1. 자연스러운 상태 전환(Ordering → WaitingForFood)
                // 2. 플레이어가 K키를 눌러 주문을 받은 경우
                
                // 로그 추가: 주문 정보가 남아있는지 확인 (디버깅용)
                if (_chair?.table != null && orderedFoods.ContainsKey(_chair.table))
                {
                    Debug.Log($"<color=magenta>[Customer {this.name}] WaitingForFood 상태 진입. 테이블 주문 정보 남아있음.</color>");
                }
                else
                {
                    Debug.Log($"<color=magenta>[Customer {this.name}] WaitingForFood 상태 진입. 테이블 주문 정보 없음 (이미 처리됨).</color>");
                }
                
                Managers.PublishAction(ActionType.Customer_WaitingForFood);
                break;

            case ECustomerState.Eating:
                action.CustomerEat();
                Managers.PublishAction(ActionType.Customer_StartedEating);
                break;

            case ECustomerState.StandingUp:
                Debug.Log($"<color=yellow>[Customer {this.name}] StandingUp 상태 진입 - 일어나는 애니메이션 시작</color>");
                action.CustomerStandIdle();
                _chair?.VacateSeat();
                Managers.PublishAction(ActionType.Customer_FinishedEating);
                CustomerState = ECustomerState.LeavingRestaurant;
                Debug.Log($"<color=green>[Customer {this.name}] StandingUp 완료 - LeavingRestaurant로 전환</color>");
                break;

            case ECustomerState.LeavingRestaurant:
                Debug.Log($"<color=green>[Customer {this.name}] LeavingRestaurant 상태 진입 - 문으로 이동 시작</color>");
                action.CustomerWalk();
                if (agent != null && Managers.Map != null)
                {
                    agent.enabled = true;  // Agent 확실히 활성화
                    agent.isStopped = false;
                    agent.speed = 3.5f;    // 속도 설정
                    Vector3 doorPosition = Managers.Map.DoorPosition;
                    agent.SetDestination(doorPosition);
                    Debug.Log($"<color=cyan>[Customer {this.name}] Agent 활성화 완료. 문 위치: {doorPosition}, Agent 목적지 설정 완료</color>");
                    Debug.Log($"<color=cyan>[Customer {this.name}] Agent 상태 - enabled: {agent.enabled}, isStopped: {agent.isStopped}, speed: {agent.speed}</color>");
                }
                else
                {
                    Debug.LogError($"<color=red>[Customer {this.name}] Agent({agent != null}) 또는 MapManager({Managers.Map != null})가 null입니다!</color>");
                }
                Managers.PublishAction(ActionType.Customer_Left);
                break;
        }
        UpdateCreatureState();
    }

    private void UpdateCurrentState()
    {
        _stateTimer += Time.deltaTime;

        switch (CustomerState)
        {
            case ECustomerState.EnteringRestaurant:
                // 대기 위치가 할당된 경우 해당 위치 도착 감지
                if (_assignedWaitingPosition != Vector3.zero)
                {
                    float distanceToWaitingSpot = Vector3.Distance(transform.position, _assignedWaitingPosition);
                    if (distanceToWaitingSpot <= 1.0f) // 대기 위치 도착
                    {
                        Debug.Log($"<color=green>[Customer {this.name}]</color> 대기 위치 도착! 의자 대기 상태로 전환");
                        CustomerState = ECustomerState.WaitingForChair;
                    }
                }
                else
                {
                    // 기존 로직 (일반적인 레스토랑 중앙 도착 감지)
                    if (agent != null 
                        && !agent.pathPending 
                        && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
                    {
                        CustomerState = ECustomerState.WaitingForChair;
                    }
                }
                break;
            case ECustomerState.WaitingForChair:
                var found = FindEmptyChair();
                if (found != null)
                {
                    _chair = found;
                    _chair.Reserve(this); // 예약!
                    placeToSit = found.placeToSit;
                    CustomerState = ECustomerState.WalkingToChair;
                }
                else
                {
                    action.CustomerStandIdle();
                }
                break;
            case ECustomerState.WalkingToChair:
                if (placeToSit != null && Vector3.Distance(transform.position, placeToSit.position) <= 0.5f)
                {
                    if (_chair != null && !_chair.IsOccupied && _chair.IsReserved && _chair._reservedBy == this)
                    {
                        agent.isStopped = true;
                        _chair.SeatCustomer(this); // 점유 및 예약 해제
                        CustomerState   = ECustomerState.SittingDown;
                    }
                    else
                    {
                        if (_chair != null) _chair.Unreserve();
                        CustomerState = ECustomerState.WaitingForChair;
                    }
                }
                break;
            case ECustomerState.SittingDown:
                _sitTimer += Time.deltaTime;
                if (_sitTimer > 1.0f)
                {
                    _sitTimer = 0f;
                    CustomerState = ECustomerState.Ordering;
                }
                break;
                
            case ECustomerState.Ordering:
                // 플레이어 바라보기 로직
                if (isLookingAtPlayer && !hasLookedAtPlayer)
                {
                    LookAtPlayer();
                    
                    // 일정 시간 후 테이블 방향으로 되돌리기
                    if (_stateTimer >= lookAtPlayerDuration)
                    {
                        hasLookedAtPlayer = true;
                        isLookingAtPlayer = false;
                        Debug.Log($"<color=cyan>[Customer {this.name}] 플레이어 바라보기 완료 - 테이블 방향으로 복귀</color>");
                    }
                }
                // 플레이어 바라보기가 끝났으면 테이블 방향으로 복귀
                else if (hasLookedAtPlayer && !isLookingAtPlayer)
                {
                    LookAtTable();
                    
                    // 테이블 방향 복귀 완료 후 WaitingForFood 상태로 전환
                    Quaternion targetTableRotation = GetTableLookRotation();
                    if (targetTableRotation != Quaternion.identity && Quaternion.Angle(transform.rotation, targetTableRotation) < 5f)
                    {
                        CustomerState = ECustomerState.WaitingForFood;
                    }
                    // 테이블 정보가 없으면 원래 방향 기준으로 체크
                    else if (_chair?.table == null && Quaternion.Angle(transform.rotation, originalRotation) < 5f)
                    {
                        CustomerState = ECustomerState.WaitingForFood;
                    }
                }
                break;
                
            case ECustomerState.Eating:
                if (_stateTimer >= eatingTime)
                {
                    CustomerState = ECustomerState.StandingUp;
                }
                break;

            case ECustomerState.LeavingRestaurant:
                if (Managers.Map != null)
                {
                    Vector3 doorPosition = Managers.Map.DoorPosition;
                    float distanceToDoor = Vector3.Distance(transform.position, doorPosition);
                    
                    // 문 주위 넓은 범위로 도착 감지 (3.0f로 크게)
                    if (distanceToDoor <= 3.0f)
                    {
                        Debug.Log($"<color=cyan>[Customer {this.name}] 문 주위 도착! 거리: {distanceToDoor}</color>");
                        CleanupAndReturn();
                    }
      
                }
                break;
        }
    }

    private void UpdateCreatureState()
    {
        switch (CustomerState)
        {
            case ECustomerState.WalkingToChair:
            case ECustomerState.LeavingRestaurant:
                CreatureState = ECreatureState.Move;
                break;

            case ECustomerState.Ordering:
            case ECustomerState.Eating:
                CreatureState = ECreatureState.Skill;
                break;

            default:
                CreatureState = ECreatureState.Idle;
                break;
        }
    }


    public void SiparisStateGec()
    {
        CustomerState = ECustomerState.Ordering;
    }

    public void SiparisVer()
    {
        CustomerState = ECustomerState.WaitingForFood;
    }

    #region Helper Methods

    public Chair FindEmptyChair()
    {
        var allChairs = FindObjectsOfType<Chair>().OrderBy(c => c.transform.position.x).ToList();
        return allChairs.FirstOrDefault(c => c.IsAvailable);
    }

    private void CleanupAndReturn()
    {
        Debug.Log($"<color=red>[Customer {this.name}] 문에 도착하여 디스폰됩니다.</color>");
        
        // 대기열에서 제거 (혹시 남아있을 수 있는 경우)
        if (_isInWaitingQueue)
        {
            Debug.Log($"<color=yellow>[Customer {this.name}] 디스폰 전 대기열에서 제거</color>");
            RemoveFromWaitingQueue();
        }
        
        // 주문 데이터 정리 (혹시 남아있을 수 있는 주문들)
        if (orderedFoods != null && orderedFoods.Count > 0)
        {
            Debug.Log($"<color=yellow>[Customer {this.name}] 디스폰 전 주문 데이터 정리</color>");
            
            // OrderManager에서 이 고객의 주문들 제거
            Managers.Game.CustomerCreator.OrderManager.RemoveOrdersByCustomer(this);
            
            // 개인 주문 데이터 정리
            orderedFoods.Clear();
        }
        
        // 의자 점유 해제 (혹시 남아있을 수 있는 점유 상태)
        if (_chair != null && _chair.IsOccupied && _chair._currentCustomer == this)
        {
            Debug.Log($"<color=yellow>[Customer {this.name}] 디스폰 전 의자 점유 해제</color>");
            _chair.VacateSeat();
        }
        
        // 모델 인스턴스 명시적 삭제
        if (modelInstance != null)
        {
            Debug.Log($"<color=yellow>[Customer {this.name}] 모델 인스턴스 삭제: {modelInstance.name}</color>");
            Destroy(modelInstance);
            modelInstance = null;
        }
        
        // 구독 해제
        _chairChangedSubscription?.Dispose();
        
        Debug.Log($"<color=green>[Customer {this.name}] 정리 완료 - 디스폰 진행</color>");
        Managers.Object.Despawn(this);
    }

    #endregion

    private void GenerateOrderFromManager()
    {
        orderedFoods.Clear(); // 이전 주문은 초기화

        var foodList = Managers.Game.CustomerCreator.FoodManager.GetAllFoods();
        if (foodList.Count == 0)
        {
            Debug.LogWarning($"[Customer {this.name}] FoodList가 비어있습니다. 주문을 생성할 수 없습니다.");
            return;
        }

        Table currentTable = _chair?.table; // 고객이 앉은 의자의 테이블
        if (currentTable == null)
        {
            Debug.LogWarning($"[Customer {this.name}] 현재 앉은 테이블 정보를 찾을 수 없습니다 (_chair 또는 _chair.table이 null). 주문을 생성할 수 없습니다.");
            return;
        }

        Debug.Log($"[Customer {this.name}] 테이블 ID: {currentTable.tableId} (객체 InstanceID: {currentTable.GetInstanceID()}) 에서 주문 생성을 시도합니다.");

        // 예시: 1개의 랜덤 음식 주문
        int numberOfItemsToOrder = 1;
        List<Food> foodsForThisOrder = new List<Food>();

        for (int i = 0; i < numberOfItemsToOrder; i++)
        {
            // 단순 랜덤 선택
            var selectedFood = foodList[Random.Range(0, foodList.Count)];
            var orderedFoodInstance = selectedFood.Clone(); // 복제
            
            foodsForThisOrder.Add(orderedFoodInstance);
            Debug.Log($"[Customer {this.name}] 테이블 ID: {currentTable.tableId}에 {orderedFoodInstance.RecipeName} (ID: {orderedFoodInstance.NO}, 수량: {orderedFoodInstance.Quantity}) 추가 시도.");
        }
        orderedFoods[currentTable] = foodsForThisOrder; // 현재 테이블에 주문 목록 할당
        UpdateOrderText(); // 주문 텍스트 UI 업데이트
    }


    public void UpdateOrderText()
    {
        string text = "";
        foreach (var tableKvp in orderedFoods)
        {
            var table = tableKvp.Key;
            var foodList = tableKvp.Value;
            
            foreach (var food in foodList)
            {
                text += $"{food.RecipeName} x{food.Quantity}";
                if (!string.IsNullOrEmpty(food.SpecialRequest))
                    text += $" ({food.SpecialRequest})";
                text += "\n";
            }
        }
        orderText.text = text.TrimEnd('\n');
    }


    private void SetupCustomerModel(ClientCustomer clientCustomer)
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        modelInstance = Instantiate(clientCustomer.ModelPrefab, transform.position, Quaternion.identity);
        modelInstance.transform.SetParent(transform);
        // Relay 스크립트 추가 및 연결
         // Relay 스크립트 추가 및 연결
        var relay = modelInstance.AddComponent<AnimationEventRelay>();
        relay.customer = this;

        modelAnimator = modelInstance.GetComponent<Animator>();
        if (modelAnimator == null)
            Debug.LogError("modelAnimator가 null입니다! 프리팹에 Animator가 붙어있는지 확인하세요.");

        if (modelAnimator != null && clientCustomer.AnimatorController != null)
        {
            modelAnimator.runtimeAnimatorController = clientCustomer.AnimatorController;
        }

        if (action != null)
        {
            action.SetAnimator(modelAnimator);
            if (modelAnimator == null)
                Debug.LogError("SetAnimator에 null 전달됨!");
        }

    
        if (action != null)
        {
            action.SetAnimator(modelAnimator);
        }
    }

    /// <summary>
    /// 플레이어를 바라보는 메서드
    /// </summary>
    private void LookAtPlayer()
    {
        if (Managers.Game?.Player == null) return;
        
        Vector3 playerPosition = Managers.Game.Player.transform.position;
        Vector3 lookDirection = (playerPosition - transform.position).normalized;
        lookDirection.y = 0; // Y축 회전만 적용 (수평 회전만)
        
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookAtSpeed * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// 테이블 방향으로 바라보는 메서드
    /// </summary>
    private void LookAtTable()
    {
        if (_chair?.table == null) 
        {
            // 테이블 정보가 없으면 원래 방향으로 복귀
            transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, lookAtSpeed * Time.deltaTime);
            return;
        }
        
        // 테이블 중앙을 바라보도록 계산
        Vector3 tablePosition = _chair.table.transform.position;
        Vector3 lookDirection = (tablePosition - transform.position).normalized;
        lookDirection.y = 0; // Y축 회전만 적용 (수평 회전만)
        
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookAtSpeed * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// 테이블을 바라보는 목표 회전값을 계산하는 메서드
    /// </summary>
    private Quaternion GetTableLookRotation()
    {
        if (_chair?.table == null) return Quaternion.identity;
        
        Vector3 tablePosition = _chair.table.transform.position;
        Vector3 lookDirection = (tablePosition - transform.position).normalized;
        lookDirection.y = 0; // Y축 회전만 적용
        
        return lookDirection != Vector3.zero ? Quaternion.LookRotation(lookDirection) : Quaternion.identity;
    }

    /// <summary>
    /// 대기 위치 설정 (CustomerCreator에서 호출)
    /// </summary>
    /// <param name="waitingPosition">할당된 대기 위치</param>
    public void SetWaitingPosition(Vector3 waitingPosition)
    {
        _assignedWaitingPosition = waitingPosition;
        _isInWaitingQueue = true;
        
        Debug.Log($"<color=cyan>[Customer {this.name}]</color> 대기 위치 할당됨: {waitingPosition}");
        
        // Agent 상태 확인 및 즉시 이동
        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
            agent.SetDestination(waitingPosition);
            Debug.Log($"<color=green>[Customer {this.name}]</color> Agent 활성화 및 대기 위치로 즉시 이동 시작 - Agent enabled: {agent.enabled}");
        }
        else
        {
            Debug.LogError($"<color=red>[Customer {this.name}]</color> Agent가 null입니다! 이동할 수 없습니다.");
        }
    }
    
    /// <summary>
    /// 대기열에서 제거 (테이블로 이동할 때 호출)
    /// </summary>
    public void RemoveFromWaitingQueue()
    {
        if (_isInWaitingQueue)
        {
            Managers.Map.RemoveFromWaitingQueue(this);
            _isInWaitingQueue = false;
            _assignedWaitingPosition = Vector3.zero;
            Debug.Log($"<color=orange>[Customer {this.name}]</color> 대기열에서 제거됨 - 뒤의 고객들이 자동으로 앞으로 이동할 것입니다");
        }
    }
    
    /// <summary>
    /// 현재 대기 중인지 확인
    /// </summary>
    public bool IsInWaitingQueue => _isInWaitingQueue;
    
    /// <summary>
    /// 할당된 대기 위치
    /// </summary>
    public Vector3 AssignedWaitingPosition => _assignedWaitingPosition;

}

