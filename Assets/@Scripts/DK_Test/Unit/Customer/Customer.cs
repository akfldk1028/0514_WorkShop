using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static Define;

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
    public Transform parent;
    public Image earnMoneyImage;
    public Image orderImage;
    public Chair chair;
    public Transform placeToSit;
    public Transform door;
    
    [Header("Settings")]
    public float eatingTime = 10f;
    public AudioClip gainGoldClip;
    
    [Header("Model References")]
    public GameObject modelInstance;
    public Animator modelAnimator;
    
    // 상태 관리
    private ECustomerState _customerState = ECustomerState.None;
    private float _stateTimer;
    private AudioSource _audioSource;

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
        _audioSource = GetComponent<AudioSource>();
        action = GetComponent<Action_Test>();
        return true;
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
                CustomerState = ECustomerState.WaitingForChair;
                break;

            case ECustomerState.WaitingForChair:
                action.CustomerStandIdle();
                break;

            case ECustomerState.WalkingToChair:
                action.CustomerWalk();
                if (placeToSit != null && agent != null)
                {
                    agent.isStopped = false;
                    agent.SetDestination(placeToSit.position);
                }
                break;

            case ECustomerState.SittingDown:
                action.CustomerSit();
                if (chair?.platePlace != null)
                {
                    transform.LookAt(chair.platePlace);
                    chair.SetMusteri(this);
                }
                CustomerState = ECustomerState.Ordering;
                break;

            case ECustomerState.Ordering:
                action.CustomerOrder();
                if (orderImage != null)
                    orderImage.gameObject.SetActive(true);
                CustomerState = ECustomerState.WaitingForFood;
                break;

            case ECustomerState.WaitingForFood:
                action.CustomerSitIdle();
                if (orderImage != null)
                    orderImage.gameObject.SetActive(false);
                break;

            case ECustomerState.Eating:
                action.CustomerEat();
                break;

            case ECustomerState.StandingUp:
                action.CustomerStand();
                if (_audioSource && gainGoldClip)
                {
                    _audioSource.PlayOneShot(gainGoldClip);
                }
                chair?.MasadanKalkma();
                CustomerState = ECustomerState.LeavingRestaurant;
                break;

            case ECustomerState.LeavingRestaurant:
                action.CustomerWalk();
                if (agent != null && door != null)
                {
                    agent.isStopped = false;
                    agent.SetDestination(door.position);
                }
                break;
        }

        UpdateCreatureState();
    }

    private void UpdateCurrentState()
    {
        _stateTimer += Time.deltaTime;

        switch (CustomerState)
        {
            case ECustomerState.WaitingForChair:
                chair = FindEmptyChair();
                if (chair != null)
                {
                    CustomerState = ECustomerState.WalkingToChair;
                }
                break;

            case ECustomerState.WalkingToChair:
                if (placeToSit != null && agent != null &&
                    Vector3.Distance(transform.position, placeToSit.position) <= 0.3f)
                {
                    agent.isStopped = true;
                    CustomerState = ECustomerState.SittingDown;
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

    #region Public Methods (기존 호환성 유지)

    public void MasadanKalk()
    {
        CustomerState = ECustomerState.StandingUp;
    }

    public void SiparisStateGec()
    {
        CustomerState = ECustomerState.Ordering;
    }

    public void SiparisVer()
    {
        CustomerState = ECustomerState.WaitingForFood;
    }

    public void StartEatingFood()
    {
        CustomerState = ECustomerState.Eating;
    }

    #endregion

    #region Animation Methods - Action_Test 사용

    // Action_Test 컴포넌트를 통한 애니메이션 제어
    // 기존 PlayAnimation 메서드는 제거하고 Action_Test 사용

    #endregion

    #region Helper Methods

    public Chair FindEmptyChair()
    {
        // 빈 의자 찾기 로직
        return null;
    }

    private void CleanupAndReturn()
    {
        placeToSit = null;
        chair = null;
        parent.gameObject.SetActive(false);
    }

    #endregion

    public override void SetInfo<T>(int templateID, T client)
    {
        base.SetInfo(templateID, client);
        
        // 컴포넌트 초기화
        agent = GetComponent<NavMeshAgent>();
        
        // 필수 컴포넌트 체크
        if (agent == null)
        {
            Debug.LogError("[Customer] NavMeshAgent를 찾을 수 없습니다!");
            return;
        }
        
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

        // 모든 초기화가 완료된 후 상태 변경
        CustomerState = ECustomerState.EnteringRestaurant;
    }

    private void SetupCustomerModel(ClientCustomer clientCustomer)
    {
        modelInstance = Instantiate(clientCustomer.ModelPrefab);
        modelInstance.transform.SetParent(transform);

        modelAnimator = modelInstance.GetComponent<Animator>();
        if (modelAnimator != null && clientCustomer.AnimatorController != null)
        {
            modelAnimator.runtimeAnimatorController = clientCustomer.AnimatorController;
        }
    }
}
// public class Customer : Unit
// {

//     public CustomerWaitForChairState musteriChairBekleState;
//     public CustomerEatingState customerEatingState;   
//     public CustomerOrderState customerOrderState; 
//     public CustomerSittingIdleState sittingIdleState; 
//     public MusteriSitToStand sitToStand; 
//     public CustomerWalkState customerWalkState;   
//     public CustomerStandToSitState standToSitState;  
//     [Space(10)]
//     public Transform parent;
//     public Image earnMoneyImage;
//     public Image orderImage;
//     public float eatingTime;
//     public Transform door;
//     Animator animator;
//     public Chair chair;
//     public Transform placeToSit;




//     // Restaurant restaurant;
//     // void Awake()
//     // {
//     //     Debug.Log("<color=magenta>[Customer]</color> Awake");
//     //     // isReady = false;
//     //     // level = FindObjectOfType<Level>();
//     //     // restaurant = level.restaurant;
//     //     // orderImage.sprite = level.orderSprite;
//     //     // action =  GetComponent<Action>();
//     //     animator = GetComponent<Animator>();
//     //     agent = GetComponent<NavMeshAgent>();
//     // }

//         [Header("References")]
//     public GameObject modelInstance;  // 인스턴스화된 3D 모델
//     public Animator modelAnimator;    // 모델의 애니메이터
//    	public float UpdateAITick { get; protected set; } = 0.0f;

// 	public override ECreatureState CreatureState 
// 	{
// 		get { return base.CreatureState; }
// 		set
// 		{
// 			if (_creatureState != value)
// 			{
// 				base.CreatureState = value;
// 				switch (value)
// 				{
// 					case ECreatureState.Idle:
// 						UpdateAITick = 0.5f;
// 						break;
// 					case ECreatureState.Move:
// 						UpdateAITick = 0.0f;
// 						break;
// 					case ECreatureState.Skill:
// 						UpdateAITick = 0.0f;
// 						break;
// 					case ECreatureState.Dead:
// 						UpdateAITick = 1.0f;
// 						break;
// 				}
// 			}
// 		}
// 	}

// 	public override bool Init()
// 	{
//         Debug.Log("<color=magenta>[Customer]</color> Init");
// 		if (base.Init() == false)
// 			return false;

// 		ObjectType = EObjectType.Customer;




// 		return true;
// 	}
//     void Update()
//     {
//         if (modelInstance != null && modelAnimator != null)
//         {
//             Vector3 deltaPosition = modelInstance.transform.localPosition;
//             if (deltaPosition.magnitude > 0.001f)
//             {
//                 transform.position += transform.TransformDirection(deltaPosition);
//                 modelInstance.transform.localPosition = Vector3.zero;
//             }
//         }
//     }

//     public override void SetInfo<T>(int templateID, T client) 
//     {
//         base.SetInfo(templateID, client);
//         CreatureState = ECreatureState.Idle;
//         agent = GetComponent<NavMeshAgent>();
//         Debug.Log($"<color=magenta>[Customer]</color> {agent}");
//         ClientCustomer clientCustomer = client as ClientCustomer;
//         if (clientCustomer.ModelPrefab != null)
//         {
//             // 1. 먼저 모델 프리팹을 인스턴스화
//             modelInstance = Instantiate(clientCustomer.ModelPrefab);
            
//             // 2. 생성된 인스턴스를 현재 Customer의 자식으로 설정
//             modelInstance.transform.SetParent(transform);
            
//             // 3. 로컬 위치와 회전을 초기화 (필요에 따라 조정)
//             // modelInstance.transform.localPosition = Vector3.zero;
//             // modelInstance.transform.localRotation = Quaternion.identity;
//             // modelInstance.transform.localScale = Vector3.one;
            
//             // 애니메이터 컨트롤러 설정
//             modelAnimator = modelInstance.GetComponent<Animator>();
//             if (modelAnimator != null && clientCustomer.AnimatorController != null)
//             {
//                 modelAnimator.runtimeAnimatorController = clientCustomer.AnimatorController;
//                 // modelAnimator.applyRootMotion = false; // Root Motion 끄기
//                 // modelAnimator.updateMode = AnimatorUpdateMode.Normal;

//             }
//         }
//         // earnMoneyImage.sprite = clientCustomer.EarnMoneyImage.sprite;
//         // orderImage.sprite = clientCustomer.OrderImage.sprite;
//     }
// }
//////////////////////////////////////////////////////////////////////////
///
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.AI;

// public class Customer : Unit
// {

//     public CustomerWaitForChairState musteriChairBekleState;
//     public CustomerEatingState customerEatingState;   
//     public CustomerOrderState customerOrderState; 
//     public CustomerSittingIdleState sittingIdleState; 
//     public MusteriSitToStand sitToStand; 
//     public CustomerWalkState customerWalkState;   
//     public CustomerStandToSitState standToSitState;  
//     [Space(10)]
//     public Transform parent;
//     public Image earnMoneyImage;
//     public Image orderImage;
//     public float eatingTime;
//     public Transform door;
//     Animator animator;
//     public Chair chair;
//     public Transform placeToSit;
//     // Restaurant restaurant;
//     void Awake()
//     {
//         // isReady = false;
//         // level = FindObjectOfType<Level>();
//         // restaurant = level.restaurant;
//         // orderImage.sprite = level.orderSprite;
//         // action =  GetComponent<Action>();
//         animator = GetComponent<Animator>();
//         agent = GetComponent<NavMeshAgent>();
//     }
//     private void OnEnable() {
        
//     }
//     private void OnDisable() {
        
//     }
//     void Start()
//     {
//         currState = customerWalkState;
//     }
//     void Update()
//     {
//         currState.UpdateState(action);
//     }
//     public void MasadanKalk()
//     {
//         action.CustomerStand();
//     }
//     public void MasadanKalkState()
//     {
//         currState = sitToStand;
//     }
//     public void SiparisStateGec()
//     {
//         currState = customerOrderState;
//     }
//     public void SiparisVer()
//     {
//         // level.restaurant.waitingForFoodChairs.Add(chair);
//         currState = sittingIdleState;
//         // restaurant.WaiterDeliverFood?.Invoke();
//     }
//     public void FindEmptyChair()
//     {
//         // if(level.restaurant.emptyChairs.Count == 0)
//         // {
//             // return null;
//         // }
//         // var _chair = level.restaurant.emptyChairs[0];
//         // chair = _chair;
//         // placeToSit = _chair.placeToSit;
//         // Debug.Log("find " + _chair , _chair);
//         // level.restaurant.emptyChairs.Remove(level.restaurant.emptyChairs[0]);
//         // return _chair;
//     }
// }
