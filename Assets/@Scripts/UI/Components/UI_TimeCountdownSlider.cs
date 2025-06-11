using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_TimeCountdownSlider : MonoBehaviour
{
    [Header("슬라이더 설정")]
    public Slider timeSlider;
    
    [Header("시간 설정")]
    private float totalTime = 30f; // 총 시간 (초) - 기본 30초
    [SerializeField] private float currentTime;
    [SerializeField] private bool isCountingDown = false;
    
    [Header("방향 설정")]
    [SerializeField] private bool rightToLeft = true; // 오른쪽에서 왼쪽으로
    
    [Header("이벤트 설정")]
    public bool pauseOnTimeUp = true;
    public bool resetOnComplete = false;
    
    [Header("디버그")]
    public bool showTimeInConsole = false;
    
    // 이벤트
    public System.Action OnTimeUp; // 시간 종료 시 호출
    public System.Action<float> OnTimeChanged; // 시간 변경 시 호출 (남은 시간)
    public System.Action<float> OnPercentageChanged; // 퍼센트 변경 시 호출 (0~100)
    
    void Start()
    {
        InitializeSlider();
    }
    
    void Update()
    {
        if (isCountingDown)
        {
            UpdateCountdown();
        }
        
        if (showTimeInConsole && isCountingDown)
        {
            Debug.Log($"남은 시간: {currentTime:F1}초 ({GetPercentage():F1}%)");
        }
    }
    
    /// <summary>
    /// 슬라이더 초기화
    /// </summary>
    void InitializeSlider()
    {
        if (timeSlider == null)
        {
            timeSlider = GetComponent<Slider>();
        }
        
        if (timeSlider != null)
        {
            // 슬라이더 범위 설정 (0~1)
            timeSlider.minValue = 0f;
            timeSlider.maxValue = 1f;
            
            // 방향 설정
            if (rightToLeft)
            {
                timeSlider.direction = Slider.Direction.RightToLeft;
            }
            else
            {
                timeSlider.direction = Slider.Direction.LeftToRight;
            }
        }
        
        // 시간 초기화 (이미 설정된 값이 있으면 유지)
        if (currentTime <= 0f) // currentTime이 0 이하일 때만 totalTime으로 초기화
        {
            currentTime = totalTime;
        }
        UpdateSliderValue();
    }
    
    /// <summary>
    /// 카운트다운 업데이트
    /// </summary>
    void UpdateCountdown()
    {
        currentTime -= Time.deltaTime;
        
        // 시간이 0 이하가 되면
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            OnTimeFinished();
        }
        
        UpdateSliderValue();
        
        // 이벤트 호출
        OnTimeChanged?.Invoke(currentTime);
        OnPercentageChanged?.Invoke(GetPercentage());
    }
    
    /// <summary>
    /// 슬라이더 값 업데이트
    /// </summary>
    void UpdateSliderValue()
    {
        if (timeSlider != null)
        {
            float percentage = currentTime / totalTime;
            timeSlider.value = percentage;
        }
    }
    
    /// <summary>
    /// 시간 종료 처리
    /// </summary>
    void OnTimeFinished()
    {
        if (pauseOnTimeUp)
        {
            isCountingDown = false;
        }
        
        OnTimeUp?.Invoke();
        
        if (resetOnComplete)
        {
            ResetTimer();
        }
        
        Debug.Log("시간 종료!");
    }
    
    /// <summary>
    /// 카운트다운 시작
    /// </summary>
    public void StartCountdown()
    {
        isCountingDown = true;
        Debug.Log($"카운트다운 시작! 총 {totalTime}초");
    }
    
    /// <summary>
    /// 카운트다운 일시정지
    /// </summary>
    public void PauseCountdown()
    {
        isCountingDown = false;
        Debug.Log("카운트다운 일시정지");
    }
    
    /// <summary>
    /// 카운트다운 재개
    /// </summary>
    public void ResumeCountdown()
    {
        if (currentTime > 0)
        {
            isCountingDown = true;
            Debug.Log("카운트다운 재개");
        }
    }
    
    /// <summary>
    /// 타이머 리셋
    /// </summary>
    public void ResetTimer()
    {
        currentTime = totalTime;
        isCountingDown = false;
        UpdateSliderValue();
        Debug.Log("타이머 리셋");
    }
    
    /// <summary>
    /// 총 시간 설정
    /// </summary>
    /// <param name="newTotalTime">새로운 총 시간</param>
    public void SetTotalTime(float newTotalTime)
    {
        totalTime = newTotalTime;
        currentTime = totalTime;
        UpdateSliderValue();
    }
    
    /// <summary>
    /// 현재 시간 설정
    /// </summary>
    /// <param name="newCurrentTime">새로운 현재 시간</param>
    public void SetCurrentTime(float newCurrentTime)
    {
        currentTime = Mathf.Clamp(newCurrentTime, 0f, totalTime);
        UpdateSliderValue();
    }
    
    /// <summary>
    /// 시간 추가
    /// </summary>
    /// <param name="addTime">추가할 시간</param>
    public void AddTime(float addTime)
    {
        currentTime = Mathf.Min(currentTime + addTime, totalTime);
        UpdateSliderValue();
        Debug.Log($"시간 추가: +{addTime}초");
    }
    
    /// <summary>
    /// 시간 감소
    /// </summary>
    /// <param name="subtractTime">감소할 시간</param>
    public void SubtractTime(float subtractTime)
    {
        currentTime = Mathf.Max(currentTime - subtractTime, 0f);
        UpdateSliderValue();
        Debug.Log($"시간 감소: -{subtractTime}초");
        
        if (currentTime <= 0f)
        {
            OnTimeFinished();
        }
    }
    
    /// <summary>
    /// 방향 변경
    /// </summary>
    /// <param name="rightToLeftDirection">오른쪽에서 왼쪽으로 여부</param>
    public void SetDirection(bool rightToLeftDirection)
    {
        rightToLeft = rightToLeftDirection;
        
        if (timeSlider != null)
        {
            timeSlider.direction = rightToLeft ? 
                Slider.Direction.RightToLeft : 
                Slider.Direction.LeftToRight;
        }
    }
    
    /// <summary>
    /// 현재 시간 가져오기
    /// </summary>
    /// <returns>현재 남은 시간</returns>
    public float GetCurrentTime()
    {
        return currentTime;
    }
    
    /// <summary>
    /// 총 시간 가져오기
    /// </summary>
    /// <returns>총 시간</returns>
    public float GetTotalTime()
    {
        return totalTime;
    }
    
    /// <summary>
    /// 퍼센트 가져오기
    /// </summary>
    /// <returns>남은 시간 퍼센트 (0~100)</returns>
    public float GetPercentage()
    {
        return (currentTime / totalTime) * 100f;
    }
    
    /// <summary>
    /// 카운트다운 상태 확인
    /// </summary>
    /// <returns>카운트다운 중인지 여부</returns>
    public bool IsCountingDown()
    {
        return isCountingDown;
    }
    
    /// <summary>
    /// 시간을 "분:초" 형식으로 가져오기
    /// </summary>
    /// <returns>시간 문자열</returns>
    public string GetTimeString()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    
    /// <summary>
    /// 자동 시작 (인스펙터에서 호출 가능)
    /// </summary>
    [ContextMenu("Start Countdown")]
    public void StartCountdownFromInspector()
    {
        ResetTimer();
        StartCountdown();
    }
}

// 사용 예시:
/*
public class GameTimer : MonoBehaviour
{
    public TimeCountdownSlider countdownSlider;
    public Text timeText;
    
    void Start()
    {
        // 이벤트 연결
        countdownSlider.OnTimeUp += OnGameTimeUp;
        countdownSlider.OnTimeChanged += UpdateTimeDisplay;
        
        // 30초 게임 타이머 설정
        countdownSlider.SetTotalTime(30f);
        countdownSlider.StartCountdown();
    }
    
    void OnGameTimeUp()
    {
        Debug.Log("게임 종료!");
        // 게임 종료 로직
    }
    
    void UpdateTimeDisplay(float remainingTime)
    {
        if (timeText != null)
        {
            timeText.text = countdownSlider.GetTimeString();
        }
    }
    
    // 보너스 시간 추가
    public void AddBonusTime()
    {
        countdownSlider.AddTime(5f);
    }
}
*/