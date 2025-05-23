using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using static Define;

public class Customer : Unit
{

    public CustomerWaitForChairState musteriChairBekleState;
    public CustomerEatingState customerEatingState;   
    public CustomerOrderState customerOrderState; 
    public CustomerSittingIdleState sittingIdleState; 
    public MusteriSitToStand sitToStand; 
    public CustomerWalkState customerWalkState;   
    public CustomerStandToSitState standToSitState;  
    [Space(10)]
    public Transform parent;
    public Image earnMoneyImage;
    public Image orderImage;
    public float eatingTime;
    public Transform door;
    Animator animator;
    public Chair chair;
    public Transform placeToSit;




    // Restaurant restaurant;
    // void Awake()
    // {
    //     Debug.Log("<color=magenta>[Customer]</color> Awake");
    //     // isReady = false;
    //     // level = FindObjectOfType<Level>();
    //     // restaurant = level.restaurant;
    //     // orderImage.sprite = level.orderSprite;
    //     // action =  GetComponent<Action>();
    //     animator = GetComponent<Animator>();
    //     agent = GetComponent<NavMeshAgent>();
    // }

        [Header("References")]
    public GameObject modelInstance;  // 인스턴스화된 3D 모델
    public Animator modelAnimator;    // 모델의 애니메이터
   	public float UpdateAITick { get; protected set; } = 0.0f;

	public override ECreatureState CreatureState 
	{
		get { return base.CreatureState; }
		set
		{
			if (_creatureState != value)
			{
				base.CreatureState = value;
				switch (value)
				{
					case ECreatureState.Idle:
						UpdateAITick = 0.5f;
						break;
					case ECreatureState.Move:
						UpdateAITick = 0.0f;
						break;
					case ECreatureState.Skill:
						UpdateAITick = 0.0f;
						break;
					case ECreatureState.Dead:
						UpdateAITick = 1.0f;
						break;
				}
			}
		}
	}

	public override bool Init()
	{
        Debug.Log("<color=magenta>[Customer]</color> Init");
		if (base.Init() == false)
			return false;

		ObjectType = EObjectType.Customer;




		return true;
	}
    void Update()
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

    public override void SetInfo<T>(int templateID, T client) 
    {
        base.SetInfo(templateID, client);
        CreatureState = ECreatureState.Idle;
        ClientCustomer clientCustomer = client as ClientCustomer;
        if (clientCustomer.ModelPrefab != null)
        {
            // 1. 먼저 모델 프리팹을 인스턴스화
            modelInstance = Instantiate(clientCustomer.ModelPrefab);
            
            // 2. 생성된 인스턴스를 현재 Customer의 자식으로 설정
            modelInstance.transform.SetParent(transform);
            
            // 3. 로컬 위치와 회전을 초기화 (필요에 따라 조정)
            // modelInstance.transform.localPosition = Vector3.zero;
            // modelInstance.transform.localRotation = Quaternion.identity;
            // modelInstance.transform.localScale = Vector3.one;
            
            // 애니메이터 컨트롤러 설정
            modelAnimator = modelInstance.GetComponent<Animator>();
            if (modelAnimator != null && clientCustomer.AnimatorController != null)
            {
                modelAnimator.runtimeAnimatorController = clientCustomer.AnimatorController;
                // modelAnimator.applyRootMotion = false; // Root Motion 끄기
                // modelAnimator.updateMode = AnimatorUpdateMode.Normal;

            }
        }
        // earnMoneyImage.sprite = clientCustomer.EarnMoneyImage.sprite;
        // orderImage.sprite = clientCustomer.OrderImage.sprite;
    }
}
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
