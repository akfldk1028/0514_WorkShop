using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static Define;
using System.Collections.Generic;
using TMPro;

public class FoodOrderInfo
{
    public int quantity;
    public string specialRequest;
    public bool isRecommended;
}


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
    public Dictionary<Food, FoodOrderInfo> orderedFoods = new Dictionary<Food, FoodOrderInfo>();
    private ECustomerState _customerState = ECustomerState.None;
    private float _stateTimer;
    private float _sitTimer = 0f;
    private System.IDisposable _chairChangedSubscription;

    
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
        
     
        // action은 Unit 클래스에서 관리되므로 여기서는 체크만
        if (action == null)
        {
            Debug.LogWarning("[Customer] action이 null입니다. Unit.Init()에서 초기화되어야 합니다.");
        }

        ClientCustomer clientCustomer = client as ClientCustomer;
        if (clientCustomer?.ModelPrefab != null)
        {
            SetupCustomerModel(clientCustomer);
        }
         GenerateOrderFromManager();
         UpdateOrderText();

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
        // Action_Test 컴포넌트 체크
        if (action == null)
        {
            Debug.Log($"[Customer] Action_Test 컴포넌트가 null입니다. State: {state}");
            return;
        }

        switch (state)
        {
            case ECustomerState.EnteringRestaurant:
                action.CustomerWalk();
                Debug.Log("[EnteringRestaurant] agent " + agent);
                // 레스토랑 안쪽으로 목적지 설정
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
                agent.SetDestination(placeToSit.position);
                Managers.PublishAction(ActionType.Customer_MovedToTable);
                break;

            case ECustomerState.SittingDown:
                Debug.Log("SittingDown 상태 진입");
                action.CustomerSit();
                if (_chair != null)
                {
                    _chair.SeatCustomer(this);
                    transform.position = _chair.placeToSit.position;
                    Managers.PublishAction(ActionType.Customer_Seated);
                    // 테이블 만석 체크 및 알림
                    if (_chair.table != null && _chair.table.IsFullyOccupied)
                    {
                        Managers.PublishAction(ActionType.Customer_TableFullyOccupied);
                    }
                }
                break;

            case ECustomerState.Ordering:
                action.CustomerOrder();
                // if (orderImage != null)
                //     orderImage.gameObject.SetActive(true);
                Managers.PublishAction(ActionType.Customer_Ordered);
                CustomerState = ECustomerState.WaitingForFood;
                break;

            case ECustomerState.WaitingForFood:
                action.CustomerSitIdle();
                // if (orderImage != null)
                //     orderImage.gameObject.SetActive(false);
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
        orderedFoods.Clear();

        // FoodManager에서 음식 리스트 받아오기
        var foodList = Managers.Game.CustomerCreator.FoodManager.GetAllFoods();
        if (foodList.Count == 0) return;

        // 랜덤 주문 예시
        var pick = foodList[Random.Range(0, foodList.Count)];
        orderedFoods[pick] = new FoodOrderInfo
        {
            quantity = Random.Range(1, 3),
            specialRequest = Random.value > 0.5f ? "덜 맵게" : null,
            isRecommended = Random.value > 0.7f
        };
    }


    public void UpdateOrderText()
    {

        string text = "";
        foreach (var kvp in orderedFoods)
        {
            var food = kvp.Key;
            var info = kvp.Value;
            text += $"{food.foodName} x{info.quantity}";
            if (!string.IsNullOrEmpty(info.specialRequest))
                text += $" ({info.specialRequest})";
            text += "\n";
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
            if (modelAnimator == null)
                Debug.LogError("SetAnimator에 null 전달됨!");
        }
        else
        {
            Debug.LogError("action이 null입니다!");
        }
    
        if (action != null)
        {
            action.SetAnimator(modelAnimator);
        }
    }



}

