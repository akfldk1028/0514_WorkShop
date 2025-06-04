using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static Define;
using System.Collections.Generic;
using TMPro;

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
    
    private GameObject modelInstance;
    private Animator modelAnimator;
    
    // 상태 관리
    public Dictionary<Table, List<Food>> orderedFoods = new Dictionary<Table, List<Food>>();
    private ECustomerState _customerState = ECustomerState.None;    
    private float _stateTimer;
    private float _sitTimer = 0f;
    private System.IDisposable _chairChangedSubscription;

    
    [SerializeField]
    public TextMeshProUGUI orderText; 

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
        // GenerateOrderFromManager(); // << 호출 제거
        // UpdateOrderText(); // << 호출 제거

        CustomerState = ECustomerState.EnteringRestaurant;
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
                // Debug.Log($"[Customer {this.name}] EnteringRestaurant. agent: {(agent != null ? agent.GetInstanceID().ToString() : "null")}");
                if (agent != null)
                {
                    Vector3 restaurantCenter = Managers.Map.DoorPosition;
                    agent.SetDestination(restaurantCenter);
                }
                break;

            case ECustomerState.WaitingForChair:
                action.CustomerStandIdle();
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
                Managers.PublishAction(ActionType.Customer_Ordered);
                CustomerState = ECustomerState.WaitingForFood;
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
                action.CustomerStand();
                _chair?.VacateSeat();
                Managers.PublishAction(ActionType.Customer_FinishedEating);
                CustomerState = ECustomerState.LeavingRestaurant;
                break;

            case ECustomerState.LeavingRestaurant:
                action.CustomerWalk();
                if (agent != null && door != null)
                {
                    agent.isStopped = false;
                    agent.SetDestination(door.position);
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
                // (도착 감지 로직)
                if (agent != null 
                    && !agent.pathPending 
                    && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
                {
                    CustomerState = ECustomerState.WaitingForChair;

                    // Debug.Log($"[EnteringRestaurant] ECustomerState.EnteringRestaurante: {ECustomerState.EnteringRestaurant}");
                    // if (ItemqueueManager is Chair chair)
                    // {
                    //     chair.Queue.Push(this);
                    //     CustomerState = ECustomerState.WaitingForChair;
                    // }
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
                
            case ECustomerState.Eating:
                if (_stateTimer >= eatingTime)
                {
                    CustomerState = ECustomerState.StandingUp;
                }
                break;

            case ECustomerState.LeavingRestaurant:
                if (door != null && Vector3.Distance(transform.position, door.position) <= 0.5f)
                {
                    CleanupAndReturn();
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
        transform.parent.gameObject.SetActive(false);

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

        // 예시: 1~2개의 랜덤 음식 주문
        // int numberOfItemsToOrder = Random.Range(1, 2);
        int numberOfItemsToOrder = 1;
        List<Food> foodsForThisOrder = new List<Food>();

        for (int i = 0; i < numberOfItemsToOrder; i++)
        {
            var foodTemplate = foodList[Random.Range(0, foodList.Count -1)];
            var orderedFoodInstance = foodTemplate.Clone(); // 복제
            
            foodsForThisOrder.Add(orderedFoodInstance);
            Debug.Log($"[Customer {this.name}] 테이블 ID: {currentTable.tableId}에 {orderedFoodInstance.RecipeName} (수량: {orderedFoodInstance.Quantity}) 추가 시도.");
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
        Debug.Log("modelInstance: " + clientCustomer.ModelPrefab.name);

        modelInstance.transform.SetParent(transform);
        // Relay 스크립트 추가 및 연결
         // Relay 스크립트 추가 및 연결
        var relay = modelInstance.AddComponent<AnimationEventRelay>();
        relay.customer = this;

        modelAnimator = modelInstance.GetComponent<Animator>();
        Debug.Log("modelAnimator: " + clientCustomer.ModelPrefab.name);
        if (modelAnimator == null)
            Debug.LogError("modelAnimator가 null입니다! 프리팹에 Animator가 붙어있는지 확인하세요.");

        if (modelAnimator != null && clientCustomer.AnimatorController != null)
        {
            modelAnimator.runtimeAnimatorController = clientCustomer.AnimatorController;
        }
    
        if (action != null)
        {
            action.SetAnimator(modelAnimator);
        }
    }



}

