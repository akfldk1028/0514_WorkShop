/*
 * 게임 매니저 (GameManager)
 * 
 * 역할:
 * 1. 게임의 핵심 데이터와 게임 진행 상태 관리
 * 2. 게임 세이브 데이터(리소스, 영웅 등) 저장 및 로드
 * 3. 플레이어 진행 상황 추적 및 저장
 * 4. 영웅 소유 상태 및 레벨, 경험치 등의 데이터 관리
 * 5. JSON 형식으로 게임 데이터를 저장하고 로드하는 기능 제공
 * 6. 게임 초기화 및 데이터 설정 제어
 * 7. Managers 클래스를 통해 전역적으로 접근 가능한 게임 데이터 제공
 */

using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


public class GameManager
{
	public GameManager()
	{
		Debug.Log("<color=yellow>[GameManager]</color> 생성됨");
	}
    private IDisposable customerSubscription;
    
    public void Init()
    {
        // 여러 Customer 이벤트를 한번에 구독
        customerSubscription = Managers.SubscribeMultiple(OnCustomerAction, 
            ActionType.Customer_Spawned,
            ActionType.Customer_MovedToTable,
            ActionType.Customer_Left);
    }

    private void OnCustomerAction(ActionType actionType)
    {
        switch (actionType)
        {
            case ActionType.Customer_Spawned:
                Debug.Log("[GameManager] 새 고객 도착!");
                // 점수, UI 업데이트 등
                break;
                
            case ActionType.Customer_MovedToTable:
                Debug.Log("[GameManager] 고객이 테이블로 이동!");
                break;
                
            case ActionType.Customer_Left:
                Debug.Log("[GameManager] 고객 퇴장!");
                break;
        }
    }

    void OnDestroy()
    {
        customerSubscription?.Dispose();
    }

	
	#region Save & Load	
	public string Path { get { return Application.persistentDataPath + "/SaveData.json"; } }


	#endregion

	#region Action
	#endregion
}
