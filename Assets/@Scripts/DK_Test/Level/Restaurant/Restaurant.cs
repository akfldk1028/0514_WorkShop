// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;

// public class Restaurant : Department 
// {
//     [Header("Sound")]
//     [SerializeField] private AnimationCurve crowdSoundVolumeCurve;

//     [Header("Lists")]
//     public List<Chair> dirtyPlates;
//     public List<Chair> waitingForFoodChairs;
//     public List<Counter> foodReadyCounters;   
//     public List<Chair> emptyChairs;
//     [SerializeField] private List<Chair> allChairs;
//     [SerializeField] private List<GameObject> allWaiters;
//     [SerializeField] private List<GameObject> allTables;
//     [SerializeField] private List<Transform> waiterWaitPlace;
//     public List<Waiter> availableWaiters;

//     public override Level level {get; set;}
//     public override GameObject dataPanel { get; set; }
//     public override Transform camPlace { get; set; }
//     [Header("UI")]
//     [SerializeField] private Transform _camTransform;
//     [SerializeField] private GameObject _dataPanel;
//     [Header("Costs")]
//     public Gold waiterCost;
//     public Gold tableCost;
//     public Gold waiterSpeedCost;
//     public Gold customerFrequencyCost;
//     [HideInInspector] private float frequencyDecreasePercentage = 3;
//     [HideInInspector] public float moveSpeedPercentageIncrease = 4;
//     [HideInInspector] public float frequencyNext;
//     [HideInInspector] public float moveNext;
//     [HideInInspector] public int tableCapacity;
//     [Header("Data")]
//     [SerializeField] private RestaurantUIData restaurantUIData;
//     [SerializeField] private RestaurantData restaurantData;
//     public int earnedMoneyFromCustomer;
//     public int tableCount =0;
//     public int waiterCapacity;
//     public int waiterCount = 0;
//     public float moveSpeed = 2;
//     public System.Action WaiterDeliverFood;
//     public System.Action WaiterCollectDirties;
//     void OnEnable()
//     {
//         WaiterDeliverFood += CheckDeliverWaiter;
//         WaiterCollectDirties += CheckCollectDirties;
//     }
//     private void OnDisable() {
        
//         WaiterDeliverFood -= CheckDeliverWaiter;
//         WaiterCollectDirties -= CheckCollectDirties;
//     }
//     void Awake()
//     {
//         levelManager = FindObjectOfType<LevelManager>();
//         level = GetComponentInParent<Level>();
//         restaurantUIData = GetComponentInChildren<RestaurantUIData>();
//     }
//     void Start()
//     {
//         tableCapacity = allTables.Count;
//         camPlace = _camTransform;
//         selectableCollider = GetComponent<Collider>();
//         dataPanel = _dataPanel;
//         // LoadRestaurant();c
//     }
//     public float GetClowdVolume()
//     {
//         float value =  (float)(tableCount)/(float)(allTables.Count); 
//         return value;
//     }
//     private void CheckCollectDirties()
//     {
//         Waiter waiter = GetAvailableWaiter();
//         if(waiter == null) 
//             return;
//         waiter.CheckDirtyPlate();
//     }
//     private void CheckDeliverWaiter()
//     {
//         Waiter waiter = GetAvailableWaiter();
//         if(waiter == null) 
//             return;
//         waiter.CheckDeliver();
//     }
//     public Waiter GetAvailableWaiter()
//     {
//         if(availableWaiters.Count == 0) 
//             return null;
//         return availableWaiters[0];
//     }
//     public void SaveRestaurant()
//     {
//         restaurantData = new RestaurantData
//         {
//             restaurantIsLocked = isLocked,
//             tableCount = tableCount,
//             waiterCount = allWaiters.Count,
//             waiterSpeed = moveSpeed,
//             customerFrequency = GetComponentInChildren<CustomerCreator>().frequency,
//             tableCost = tableCost.GetGold(),
//             waiterSpeedCost = waiterSpeedCost.GetGold(),
//             customerFrequencyCost = customerFrequencyCost.GetGold(),
//         };
//         level.levelData.restaurantData = restaurantData;
//     }
//     public void LoadRestaurant()
//     {
//         if(level.levelData.restaurantData != null)
//         {
//             restaurantData = level.levelData.restaurantData;
//             isLocked = restaurantData.restaurantIsLocked;
//             if(!isLocked)
//             {
//                 @lock.SetActive(false);
//                 for (int i = 0; i < restaurantData.tableCount; i++)
//                 {
//                     BuyTable(false);
//                 }
//                 for (int i = 0; i < restaurantData.waiterCount; i++)
//                 {
//                     BuyWaiter(false);
//                 }
//             }
//         }
//         else
//         {
//             restaurantData = new RestaurantData();
//         }
//         GetComponentInChildren<CustomerCreator>().frequency = restaurantData.customerFrequency;
//         moveSpeed = restaurantData.waiterSpeed;
//         foreach (var item in allWaiters)
//         {
//             item.GetComponentInChildren<Waiter>().moveSpeed = moveSpeed;
//         }
//         tableCost.SetGold(restaurantData.tableCost);
//         waiterSpeedCost.SetGold(restaurantData.waiterSpeedCost);
//         customerFrequencyCost.SetGold(restaurantData.customerFrequencyCost);
//         moveNext = moveSpeed + moveSpeed * (moveSpeedPercentageIncrease/100);
//         frequencyNext = GetComponentInChildren<CustomerCreator>().frequency - (GetComponentInChildren<CustomerCreator>().frequency * (frequencyDecreasePercentage/100));
//         restaurantUIData.UpdateData();
//     }
//     public void BuyWaiter(bool isPaid)
//     {
//         if(waiterCapacity == waiterCount)
//         {
//             return;
//         }
//         if(isPaid)
//         {
//             if(GameManager.instance.GetMoney() < waiterCost.GetGold())
//                 return;    
//             else
//             {
//                 GameManager.instance.SetMoney(-waiterCost.GetGold());
//             }
//         }
//         waiterCount ++;
//         GameObject waiterGameobject = Instantiate(levelManager.waiterPrefab,waiterWaitPlace[waiterCount-1].position,Quaternion.identity);
//         Waiter waiter = waiterGameobject.GetComponentInChildren<Waiter>(); 
//         waiter.waitingPlace = waiterWaitPlace[waiterCount-1];
//         waiter.level = level;
//         waiter.moveSpeed = moveSpeed;
//         waiterCost.IncreaseGold(100);
//         allWaiters.Add(waiterGameobject);
//         availableWaiters.Add(waiter);
//         CheckWaiterButton();
//         GameManager.instance.SetIdleMoneyText(level.CalculateEarnedMoneyOfPerSeconds());
//         restaurantUIData.UpdateData();
//     }
//     public void BuyTable(bool isPaid)
//     {
//         if(tableCount == tableCapacity)
//             return;
//         if(isPaid)
//         {
//             if(GameManager.instance.GetMoney() < tableCost.GetGold())
//                 return;    
//             else
//             {
//                 GameManager.instance.SetMoney(-tableCost.GetGold());
//             }
//         }

//         tableCount ++;
//         allTables[tableCount-1].gameObject.SetActive(true);
//         emptyChairs.Add(allTables[tableCount-1].transform.GetChild(0).GetComponent<Chair>());
//         emptyChairs.Add(allTables[tableCount-1].transform.GetChild(2).GetComponent<Chair>());
//         allChairs.Add(allTables[tableCount-1].transform.GetChild(0).GetComponent<Chair>());
//         allChairs.Add(allTables[tableCount-1].transform.GetChild(2).GetComponent<Chair>());
//         tableCost.IncreaseGold(100);
//         level.SetVolume(GetClowdVolume());
//         GameManager.instance.SetIdleMoneyText(level.CalculateEarnedMoneyOfPerSeconds());
//         restaurantUIData.UpdateData();
//     }
//     public void IncreaseMovementSpeed(bool isPaid)
//     {
//         if(isPaid)
//         {
//             if(GameManager.instance.GetMoney() < waiterSpeedCost.GetGold())
//                 return;    
//             else
//             {
//                 GameManager.instance.SetMoney(-waiterSpeedCost.GetGold());
//             }
//         }

//         GameManager.instance.SetMoney(-waiterSpeedCost.GetGold());
//         moveSpeed += moveSpeed * (moveSpeedPercentageIncrease/100);
//         for (int i = 0; i < allWaiters.Count; i++)
//         {
//             allWaiters[i].transform.GetChild(0).GetComponent<Waiter>().moveSpeed = moveSpeed;
//         }
//         moveNext = moveSpeed + moveSpeed * (moveSpeedPercentageIncrease/100);
//         waiterSpeedCost.IncreaseGold(100);
//         GameManager.instance.SetIdleMoneyText(level.CalculateEarnedMoneyOfPerSeconds());
//         restaurantUIData.UpdateData();
//     }
//     public void MusteriSikligiArttir(bool isPaid)
//     {
//         if(isPaid)
//         {
//             if(GameManager.instance.GetMoney() < customerFrequencyCost.GetGold())
//                 return;    
//             else
//             {
//                 GameManager.instance.SetMoney(-customerFrequencyCost.GetGold());
//             }
//         }
//         if(GameManager.instance.GetMoney() < customerFrequencyCost.GetGold())
//             return;
//         GameManager.instance.SetMoney(-customerFrequencyCost.GetGold());
//         GetComponentInChildren<CustomerCreator>().frequency -= (GetComponentInChildren<CustomerCreator>().frequency * (frequencyDecreasePercentage/100));
//         frequencyNext = GetComponentInChildren<CustomerCreator>().frequency - (GetComponentInChildren<CustomerCreator>().frequency * (frequencyDecreasePercentage/100));
//         customerFrequencyCost.IncreaseGold(100);
//         GameManager.instance.SetIdleMoneyText(level.CalculateEarnedMoneyOfPerSeconds());
//         restaurantUIData.UpdateData();
//     }
//     public void UnlockRestaurant()
//     {
//         if(unlockCost.GetGold() <= GameManager.instance.GetMoney())
//         {
//             //if (level.restoranTask.activeInHierarchy == true)
//             //    level.restoranTask.SetActive(false);
//             GameManager.instance.SetMoney(-unlockCost.GetGold());
//             lockedPanel.SetActive (false);
//             BuyTable(false);
//             BuyWaiter(false);
//             isLocked = false;
//             @lock.SetActive(false);
//             restaurantUIData.UpdateData();
//             level.RestaurantReady(false);
//             SelectManager.instance.BackButton();
//         }
//     }
//     public float PizzaDistributingTime()
//     {
//         float tableDistanceAverage =0;
//         for (int i = 0; i < allChairs.Count; i++)
//         {
//             tableDistanceAverage += Vector3.Distance (waiterWaitPlace[0].position,allChairs[i].transform.position);
//         }
//         tableDistanceAverage = (tableDistanceAverage / allChairs.Count)/moveSpeed;
//         float dishCounterAverageDistance =0;
//         var temp = 0;
//         for (int i = 0; i < level.allSculleries.Count; i++)
//         {
//             for (int j = 0; j < level.allSculleries[i].currentDishCounters.Count; j++)
//             {
//                 dishCounterAverageDistance += Vector3.Distance(waiterWaitPlace[0].position,level.allSculleries[i].currentDishCounters[j].transform.position);
//                 temp++;
//             }
//         }
//         var temp2 = 0;
//         dishCounterAverageDistance = (dishCounterAverageDistance / temp)/moveSpeed;
//         float counterAverageDistance = 0;
//         for (int i = 0; i < level.allKitchens.Count; i++)
//         {
                
//             for (int j = 0; j < level.allKitchens[i].useableCounters.Count; j++)
//             {
//                 counterAverageDistance += Vector3.Distance(waiterWaitPlace[0].position,level.allKitchens[i].useableCounters[j].transform.position);
//                 temp2 ++;
//             }
//         }
//         counterAverageDistance = (counterAverageDistance / temp2)/moveSpeed;
//         return (counterAverageDistance + dishCounterAverageDistance + tableDistanceAverage) / allWaiters.Count;
        
//     }
//     public void CheckWaiterButton()
//     {
//         int counterCount = 0;
//         for (int i = 0; i < level.unlockedSculleries.Count; i++)
//         {
//             counterCount += level.unlockedSculleries[i].currentDishCounters.Where(x=>x.dishwashers.Count > 0).Count();
//         }
//         int queueCount = 4;
//         if( allWaiters.Count >= counterCount*queueCount)
//         {
//             restaurantUIData.ToggleWaiterButton(false);
//         }
//         else
//         {
//             restaurantUIData.ToggleWaiterButton(true);
//         }
//     }
// }
