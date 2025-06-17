using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public enum RhythmResult
{
    Fail,
    Good,
    Perfect,
    Pause
}

public class RhythmGameManager : MonoBehaviour
{
    // Sound related variables
    [Header("Sound Setting")]
    public AudioClip soundClip;
    public AudioSource audioSource;  // 키 사운드용 (기존 AudioSource)

    public AudioClip successSound; // 완성 상태 사운드

    [Header("Key Sound Settings")]
    public AudioClip soundA;  // Default Sound for key A
    public AudioClip soundS;  // Default Sound for key S
    public AudioClip soundD;  // Default Sound for key D
    public AudioClip soundW;  // Default Sound for key W
    public AudioClip soundZ;  // Default Sound for Space key

    [Header("Recipe Sound Settings")]
    public RecipeSoundData recipeSoundData;

    [Header("Metronome Setting")]
    public AudioClip metronomeClip;
    public AudioSource metronomeSource;  // 메트로놈 전용 (새로 추가한 AudioSource)


    [Header("Judgment Range")]
    [SerializeField] private float perfectWindow = 0.15f;
    [SerializeField] private float goodWindow = 0.3f;

    [Header("Rhythm Key Setting")]
    [SerializeField] private float interval = 0.4615f;  // 기본값으로 130 BPM에 해당하는 interval 설정
    [SerializeField] private List<string> rhythmPattern = new List<string> { "A", "S", "AD", "_", "D", "_", "D", "A" };

    private List<float> expectedTimes = new List<float>();
    private List<float> inputTimes = new List<float>();
    private List<string> inputKeys = new List<string>();

    [Header("Key UI")]
    public GameObject KeyUI;
    public List<Sprite> keySprites;  // Order: A, S, D, W, Z (Z: Space)
    public List<string> keyLabels = new List<string> { "A", "S", "D", "W", "Z" };  // Z represents Space
    public List<Image> keyImages;  // T1~T8
    private Dictionary<string, Sprite> keySpriteDict;

    [Header("Cocktail Visual")]
    public List<GameObject> cocktailStepObjects;  // 8개의 칵테일 제작 단계 이미지
    private List<GameObject> cocktailFinalObjects; // 2_Final 하위의 완성된 모습
    private GameObject currentCocktailPrefab;     // 현재 생성된 칵테일 프리팹 인스턴스
    public Transform cocktailSpawnPoint;          // 칵테일이 생성될 위치

    private bool useMetronome = true;

    //[Header("Rhythm Game UI")]
    public TextMeshProUGUI recipeName;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI orderText;

    // Coroutine references
    //private Coroutine rhythmCoroutine;
    //private Coroutine inputCoroutine;
    //private Coroutine metronomeCoroutine;

    // Current recipe being played
    private Data.RecipeData currentRecipe;

    // 현재 레시피 정보를 외부에서 접근할 수 있도록 하는 프로퍼티
    public Data.RecipeData CurrentRecipe => currentRecipe;

    private Dictionary<string, AudioClip> keySoundDict;

    [Header("Key Animation Settings")]
    [SerializeField] private float scaleMultiplier = 1.3f;  // 커질 때의 크기 배수
    [SerializeField] private float scaleDuration = 0.2f;    // 애니메이션 지속 시간


    [Header("Sample Image")]
    public Image sampleImage;  // Sample 이미지

    [Header("Countdown Images")]
    public List<GameObject> countdownImages;  // 카운트다운 이미지들 (4, 3, 2, 1, GO!) 순서대로

    [Header("Result Images")]
    public GameObject badResultImage;   // Bad 결과 이미지


    private bool isRestart = false;
    private bool isRhythmGameEnded = true;

    private void Start()
    {
        KeyUI.SetActive(false);
        resultText.gameObject.SetActive(false);
        InitKeySoundDict();
    }

    // ====== 메인 게임플레이 메서드 ======
    public void StartRhythmSequence()
    {
        isRhythmGameEnded = false; 
        
        // 🆕 Glass 체크 임시 비활성화 (테스트용)
        /*
        // Glass 보유 여부 확인 (GameManager의 전용 메서드 사용)
        if (!Managers.Game.CanCraftRecipe(1))
        {
            Debug.LogWarning("<color=red>[RhythmGameManager]</color> Glass가 부족하여 리듬 게임을 시작할 수 없습니다.");
            return; // Glass가 없으면 시작하지 않음
        }
        */
        
        // 대신 Glass가 부족하면 자동으로 추가
        if (!Managers.Game.CanCraftRecipe(1))
        {
            Managers.Game.AddGlass(5);
            Debug.Log("<color=green>[RhythmGameManager]</color> Glass가 부족해서 자동으로 5개 추가했습니다.");
        }
        
        // 리듬게임 시작 시 메인 BGM 정지
        Managers.Sound.Stop(Define.ESound.Bgm);
        //Debug.Log("<color=yellow>[RhythmGameManager]</color> 메인 BGM 정지");
        
        WaitASecond();

        // 현재 레시피가 없을 때만 새로운 레시피를 가져옴
        if (isRestart) //실패 후 재시작 시
        {   
            RestoreKeyUI();
            Debug.Log($"<color=green>[RhythmGameManager]</color> 실패한 레시피 재시도: {currentRecipe.RecipeName}");
            // 프리팹을 다시 로드하지 않고 기존 프리팹(2_steps 상태) 그대로 사용
            // 나머지 UI/패턴만 갱신
            rhythmPattern = new List<string>(currentRecipe.KeyCombination);
            UpdateRecipeNameUI();
            ShowKeyCombinationUI(currentRecipe.KeyCombination);
            StartCoroutine(InitAndStart());
            KeyUI.SetActive(true);
            return;
        }
        else //새 레시피 사용용
        {
            RestoreKeyUI();
            // OrderManager에서 다음 주문을 확인만 하기 (꺼내지 않음)
            Order nextOrder = Managers.Game.CustomerCreator.OrderManager.PeekNextOrder();
            Debug.Log($"[RhythmGameManager] 다음 주문 확인: {(nextOrder != null ? nextOrder.RecipeName : "없음")}");
            
            if (nextOrder != null)
            {
                // 주문된 레시피 ID로 레시피 데이터 가져오기
                if (Managers.Data.RecipeDic.ContainsKey(nextOrder.recipeId))
                {
                    currentRecipe = Managers.Data.RecipeDic[nextOrder.recipeId];
                    UpdateRecipeTempo();  // BPM에 따라 interval 업데이트
                    Debug.Log($"<color=green>[RhythmGameManager]</color> 주문된 레시피로 게임 시작: {currentRecipe.RecipeName} (ID: {nextOrder.recipeId})");
                }
                else //레시피 id가 없으면 랜덤 레시피 사용
                {
                    Debug.LogError($"<color=red>[RhythmGameManager]</color> 레시피 ID {nextOrder.recipeId}를 찾을 수 없습니다!");
                    currentRecipe = Managers.Ingame.getRandomRecipe();
                    UpdateRecipeTempo();  // BPM에 따라 interval 업데이트
                }
            }
            else
            {
                Debug.LogWarning($"<color=yellow>[RhythmGameManager]</color> 대기 중인 주문이 없습니다. 랜덤 레시피 사용.");
                currentRecipe = Managers.Ingame.getRandomRecipe();
                UpdateRecipeTempo();  // BPM에 따라 interval 업데이트
            }
        }
        
        // Bad 이미지가 활성화되어 있을 수 있으니 비활성화
        if (badResultImage != null)
            badResultImage.SetActive(false);

        LoadAndSpawnCocktailPrefab(currentRecipe.NO.ToString());
        Debug.Log(string.Join(", ", currentRecipe.KeyCombination));

        rhythmPattern = new List<string>(currentRecipe.KeyCombination);
        Debug.Log("rhythmPattern:" + string.Join(", ", rhythmPattern));

        // 현재 레시피와 대기 중인 주문들을 UI에 표시
        UpdateRecipeNameUI();

        ShowKeyCombinationUI(currentRecipe.KeyCombination);
        //TrimTrailingSilence();
        StartCoroutine(InitAndStart());
        KeyUI.SetActive(true);
    }

    private IEnumerator InitAndStart()
    {
        yield return new WaitForSeconds(1f);
        
        if (isRestart)
        {
            // 1_Steps(혹은 step) 오브젝트 전부 활성화
            /*if (cocktailStepObjects != null)
            {
                foreach (var stepObj in cocktailStepObjects)
                {
                    if (stepObj != null)
                        stepObj.SetActive(true);
                }
            }*/
            yield return StartCoroutine(PlayCountdownBeats());
            yield return StartCoroutine(WaitForInputs());
        }
        else
        {
        useMetronome = true;
        //metronomeCoroutine = StartCoroutine(MetronomeLoop());
        //StartCoroutine(MetronomeLoop());
        //rhythmCoroutine = StartCoroutine(PlayRhythm());
        StartCoroutine(PlayRhythm());
        }
    }

    private IEnumerator PlayRhythm()
    {
        metronomeSource.volume = 0f;
        
        // 모든 칵테일 이미지 비활성화
        foreach (var obj in cocktailStepObjects)
        {
            if (obj != null) obj.SetActive(false);
        }

        for (int i = 0; i < rhythmPattern.Count; i++)
        {
            DimKeyUI(i);

            // 칵테일 스텝 이미지는 항상 활성화 (쉼표에서도)
            if (i < cocktailStepObjects.Count && cocktailStepObjects[i] != null)
            {
                cocktailStepObjects[i].SetActive(true);
            }

            // 키 입력이 있는 경우에만 사운드 재생
            if (rhythmPattern[i] != "_")
            {
                string keyString = rhythmPattern[i][0].ToString().ToUpper();
                AudioClip soundToPlay = GetRecipeSound(keyString);
                if (soundToPlay != null)
                {
                    audioSource.PlayOneShot(soundToPlay);
                }
            }

            // 오디오 시스템 시간 기준으로 정확한 대기
            double targetTime = AudioSettings.dspTime + interval;
            while (AudioSettings.dspTime < targetTime)
            {
                yield return null;
            }
        }

        RestoreKeyUI();
        yield return StartCoroutine(PlayCountdownBeats());
        //inputCoroutine = StartCoroutine(WaitForInputs());
        StartCoroutine(WaitForInputs());
    }

    private IEnumerator PlayCountdownBeats()
    {
        sampleImage.gameObject.SetActive(true); //첫박 도입 위한 가이드 이미지
        string[] countdown = { "4", "3", "2", "1", "GO!" };

        if (sampleImage != null)
        {
            sampleImage.transform.localScale = new Vector3(2f, 2f, 2f);
        }
        
        for (int i = 0; i < countdown.Length; i++)
        {
            string c = countdown[i];
            
            // 모든 카운트다운 이미지 비활성화
            foreach (var countdownImg in countdownImages)
            {
                if (countdownImg != null)
                    countdownImg.SetActive(false);
            }
            
            // 현재 카운트다운 이미지만 활성화
            if (i < countdownImages.Count && countdownImages[i] != null)
            {
                countdownImages[i].SetActive(true);
            }
            
            if (c != "GO!")
            {
            // 카운트다운 숫자마다 메트로놈 소리 재생
            if (metronomeClip != null && metronomeSource != null)
                metronomeSource.PlayOneShot(metronomeClip);
            yield return new WaitForSeconds(interval);
            }

                if (sampleImage != null)
                {
                    Vector3 currentScale = sampleImage.transform.localScale;
                    sampleImage.transform.localScale = new Vector3(
                        currentScale.x - 0.2f,
                        currentScale.y - 0.2f,
                        currentScale.z - 0.2f
                    );
                }

            
        }
        
        // 모든 카운트다운 이미지 비활성화
        foreach (var countdownImg in countdownImages)
        {
            if (countdownImg != null)
                countdownImg.SetActive(false);
        }
        
        metronomeSource.volume = 0f; //플레이어 입력 시 메트로놈 볼륨 낮춤
        sampleImage.gameObject.SetActive(false);
        
        
    }

    private IEnumerator WaitForInputs()
    {
        
        double inputStartTime = AudioSettings.dspTime;
        HashSet<KeyCode> allowedKeys = new HashSet<KeyCode> 
        { 
            KeyCode.A, 
            KeyCode.S, 
            KeyCode.D, 
            KeyCode.W, 
            KeyCode.Space, 
            KeyCode.Escape 
        };

        for (int i = 0; i < rhythmPattern.Count; i++) //
        {
            double expectedTime = inputStartTime + i * interval;
            expectedTimes.Add((float)expectedTime);

            double slotEnd = expectedTime + interval;
            bool inputReceived = false;
            float recordedTime = -1f;
            string pressedKeyString = "";

            while (AudioSettings.dspTime < slotEnd)
            {
                if (!inputReceived)
                {
                    List<string> keysPressed = new List<string>();

                    foreach (KeyCode k in allowedKeys)
                    {
                        if (Input.GetKey(k))
                        {
                            // 키 입력 시점 기록 (밀리세컨드)
                            float keyPressTime = Time.realtimeSinceStartup * 1000f;
                            
                            string key = (k == KeyCode.Space) ? "Z" : k.ToString().ToUpper();
                            keysPressed.Add(key);
                            
                            AudioClip soundToPlay = GetRecipeSound(key);
                            if (soundToPlay != null)
                            {
                                // 오디오 재생 시점 기록 (밀리세컨드)
                                float audioPlayTime = Time.realtimeSinceStartup * 1000f;
                                audioSource.PlayOneShot(soundToPlay);
                                
                                // 시간차 계산 및 로그 출력
                                float timeDifference = audioPlayTime - keyPressTime;
                                Debug.Log($"<color=cyan>[RhythmGameManager]</color> 키 입력-오디오 재생 시간차: {timeDifference:F3}ms (키: {key})");
                            }
                        }
                    }

                    if (keysPressed.Count > 0)
                    {
                        keysPressed.Sort();
                        pressedKeyString = string.Join("", keysPressed);
                        inputReceived = true;
                        recordedTime = (float)AudioSettings.dspTime;
                    }

                    // 입력 패턴이 맞는지 확인하고 흐리게 처리
                    bool isCorrectInput = false;
                    if (rhythmPattern[i] == "_") //none input
                    {
                        if (!inputReceived)
                        {
                            isCorrectInput = true;
                            recordedTime = (float)AudioSettings.dspTime;
                        }
                    }
                    else if (inputReceived) //key input
                    {
                        string expectedKey = rhythmPattern[i].ToUpper();  // 패턴을 대문자로 변환
                        foreach (string key in keysPressed)
                        {
                            if (key == expectedKey)
                            {
                                isCorrectInput = true;
                                StartCoroutine(AnimateKeyScale(i));  // 키 입력이 있고 맞았을 때만 애니메이션 실행
                                break;
                            }
                        }
                    }

                    if (isCorrectInput) //correct input
                    {
                        DimKeyUI(i);
                        inputReceived = true;
                    }
                }
                yield return null;
            }

            inputTimes.Add(inputReceived ? recordedTime : -1f);
            inputKeys.Add(inputReceived ? pressedKeyString : "");
        }

        useMetronome = false;
        metronomeSource.volume = 0.7f;  // 메트로놈 볼륨만 원래대로 복구
        JudgeResults();
    }

    private void JudgeResults()
    {
        bool allCorrect = true;
        bool allPerfect = true;

        for (int i = 0; i < rhythmPattern.Count; i++)
        {
            string expectedKey = SortString(rhythmPattern[i].ToUpper());
            string actualKey = SortString(inputKeys[i].ToUpper());
            float expectedTime = expectedTimes[i];
            float actualTime = inputTimes[i];

            Debug.Log($"[{i}] expected: {expectedKey}, actual: {actualKey}");

            if (expectedKey == "_")
            {
                if (actualKey != "") allCorrect = false;
                continue;
            }

            if (actualKey == "" || actualKey != expectedKey)
            {
                allCorrect = false;
                continue;
            }

            float diff = Mathf.Abs(actualTime - expectedTime);
            if (diff > goodWindow)
            {
                allCorrect = false;
            }
            else if (diff > perfectWindow)
            {
                allPerfect = false;
            }
        }

        Debug.Log("입력한 키 값: " + string.Join(", ", inputKeys));

        if (!allCorrect)
        {
            StartCoroutine(HandleGameEnd(RhythmResult.Fail));
        }
        else if (allPerfect)
        {
            StartCoroutine(HandleGameEnd(RhythmResult.Perfect));
        }
        else
        {
            StartCoroutine(HandleGameEnd(RhythmResult.Good));
        }
        expectedTimes.Clear();
        inputTimes.Clear();
        inputKeys.Clear();
        //RestoreKeyUI();
    }

    private IEnumerator HandleGameEnd(RhythmResult result) // Handle game end state and result display
    {
        yield return WaitASecond();
        KeyUI.SetActive(false);
        // Show result image
        if (result == RhythmResult.Fail) //리듬게임 실패
        {
            // Bad 이미지 표시
            if (badResultImage != null)
                badResultImage.SetActive(true);
            
            // 실패 시 주문은 그대로 유지 (다시 시도)
            yield return WaitASecond();  // Show result image briefly
            
            // Bad 이미지 숨기기
            if (badResultImage != null)
                badResultImage.SetActive(false);

            // UI 업데이트 (주문은 그대로, 현재 레시피도 유지)
            UpdateRecipeNameUI();

            Debug.Log($"<color=red>[RhythmGameManager]</color> 레시피 {currentRecipe.RecipeName} 실패, 재시도합니다.");
            isRestart = true;
            StartRhythmSequence();  // Restart with same order
            /////////////////////////////////////////////////////동현이가 일단추가
            //Managers.Ingame.EndRhythmGame(result);
        }
        else if (result == RhythmResult.Pause)
        {
            //
        }
        else //성공 시
        {
            isRestart = false;
            isRhythmGameEnded = true;

            //Final 오브젝트 활성화
            if (cocktailFinalObjects != null)
            {
                foreach (var finalObj in cocktailFinalObjects)
                {
                    if (finalObj != null)
                    {
                        finalObj.SetActive(true);
                    }
                }
            }

            if (successSound != null)
            {
                audioSource.PlayOneShot(successSound);//
            }
            
            //주문 제거
            var completedOrder = Managers.Game.CustomerCreator.OrderManager.GetNextOrder();
            if (completedOrder != null)
            {
                Debug.Log($"<color=green>[RhythmGameManager]</color> 주문 완료: {completedOrder.RecipeName}");
                
                // 🆕 완성된 레시피를 플레이어 인벤토리에 추가
                Managers.Game.AddRecipeToInventory(completedOrder.recipeId, completedOrder.RecipeName, currentRecipe?.Prefab ?? "");
                
                // 🆕 UI에 완성된 레시피 아이콘 표시
                var iconSprite = Managers.Resource.Load<Sprite>(currentRecipe?.IconImage ?? "");
                if (iconSprite != null)
                {
                    InGameManager.CompletedOrderData.LastCompletedSprite = iconSprite;
                    InGameManager.CompletedOrderData.LastCompletedRecipeId = completedOrder.recipeId;
                    InGameManager.CompletedOrderData.LastCompletedPrefabName = currentRecipe?.Prefab ?? "";
                    
                    // UI 업데이트 액션 발행
                    Managers.PublishAction(ActionType.GameScene_AddCompletedRecipe);
                    Debug.Log($"<color=cyan>[RhythmGameManager]</color> UI 아이콘 추가 액션 발행: {completedOrder.RecipeName}");
                }
                else
                {
                    Debug.LogWarning($"<color=yellow>[RhythmGameManager]</color> 아이콘 이미지를 찾을 수 없습니다: {currentRecipe?.IconImage}");
                }
            }
            
            currentRecipe = null;  // Clear current recipe on success only
            
            // UI 업데이트 (주문 제거됨, 현재 레시피 클리어)
            UpdateRecipeNameUI();
            
            // Send result to game manager
            Managers.Ingame.EndRhythmGame(result);

            // 잠시 대기
            yield return new WaitForSeconds(1.5f);
            
            //만약 리스트에 다음 레시피가 있으면 다음 레시피로 넘어가고 없으면 게임 종료
            if (Managers.Game.CustomerCreator.OrderManager.GetOrderCount() > 0)
            {
                Debug.Log("<color=cyan>[RhythmGameManager]</color> 다음 주문이 있습니다. 다음 리듬게임을 시작합니다.");
                // 완성된 칵테일 프리팹 제거 및 상태 초기화
                //ClearCocktailPrefab();
                isRhythmGameEnded = false; 
                StartRhythmSequence();
            }
            else
            {
                // Debug.Log("<color=yellow>[RhythmGameManager]</color> 남은 주문이 없습니다. 메인 BGM을 재시작합니다.");
                isRhythmGameEnded = true;
                ESCPressed();
                Managers.Input.RestartMainBGM();
                Managers.Ingame.Resume();
                
            }
        }

            //yield return WaitASecond();
    }

    /// <summary>
    /// 현재 레시피를 건너뛰고 다음 레시피로 변경 (Tab키 처리용)
    /// </summary>
    public void SkipCurrentRecipe()
    {
        // 주문이 2개 이상 있을 때만 건너뛰기 가능
        if (Managers.Game.CustomerCreator.OrderManager.MoveFirstOrderToBack())
        {
            Debug.Log($"<color=cyan>[RhythmGameManager]</color> 레시피 건너뛰기: {currentRecipe?.RecipeName}");
            
            // 🔥 모든 코루틴을 완전히 정지시키기 (이게 핵심!)
            StopAllCoroutines();
            
            // 🔥 게임 상태 완전 초기화
            useMetronome = false;
            isRhythmGameEnded = false; // 새 게임을 시작하므로 false
            isRestart = false; // 건너뛰기는 새 게임 시작이므로 false
            
            // UI 초기화
            RestoreKeyUI();
            KeyUI.SetActive(false); // UI도 완전히 숨기기
            
            // 모든 카운트다운 이미지 숨기기
            foreach (var countdownImg in countdownImages)
            {
                if (countdownImg != null)
                    countdownImg.SetActive(false);
            }
            
            // Bad 이미지도 숨기기
            if (badResultImage != null)
                badResultImage.SetActive(false);
                
            // Sample 이미지도 숨기기
            if (sampleImage != null)
                sampleImage.gameObject.SetActive(false);
            
            // 입력 데이터 초기화 (중요: 이전 게임의 입력 데이터를 지워야 함)
            expectedTimes.Clear();
            inputTimes.Clear();
            inputKeys.Clear();
            
            // 칵테일 오브젝트 정리
            if (currentCocktailPrefab != null)
            {
                Destroy(currentCocktailPrefab);
                currentCocktailPrefab = null;
            }
            
            // 칵테일 스텝 오브젝트들도 초기화
            if (cocktailStepObjects != null)
                cocktailStepObjects.Clear();
            if (cocktailFinalObjects != null)
                cocktailFinalObjects.Clear();
            
            // 새로운 레시피 가져오기 (null 상태를 방지)
            Order nextOrder = Managers.Game.CustomerCreator.OrderManager.PeekNextOrder();
            
            if (nextOrder != null)
            {
                // 주문된 레시피 ID로 레시피 데이터 가져오기
                if (Managers.Data.RecipeDic.ContainsKey(nextOrder.recipeId))
                {
                    currentRecipe = Managers.Data.RecipeDic[nextOrder.recipeId];
                    UpdateRecipeTempo();  // BPM에 따라 interval 업데이트
                    Debug.Log($"<color=green>[RhythmGameManager]</color> 건너뛰고 다음 레시피로 시작: {currentRecipe.RecipeName} (ID: {nextOrder.recipeId})");
                }
                else
                {
                    Debug.LogError($"<color=red>[RhythmGameManager]</color> 레시피 ID {nextOrder.recipeId}를 찾을 수 없습니다! 랜덤 레시피 사용.");
                    currentRecipe = Managers.Ingame.getRandomRecipe();
                    UpdateRecipeTempo();  // BPM에 따라 interval 업데이트
                }
            }
            else
            {
                Debug.LogWarning($"<color=yellow>[RhythmGameManager]</color> 대기 중인 주문이 없습니다. 랜덤 레시피 사용.");
                currentRecipe = Managers.Ingame.getRandomRecipe();
                UpdateRecipeTempo();  // BPM에 따라 interval 업데이트
            }
            
            // 잠시 대기 후 새로운 레시피로 시작 (코루틴이 완전히 정리될 시간 확보)
            StartCoroutine(DelayedRestartAfterSkip());
        }
        else
        {
            Debug.Log($"<color=yellow>[RhythmGameManager]</color> 건너뛸 수 있는 주문이 없습니다.");
        }
    }
    
    /// <summary>
    /// 건너뛰기 후 약간의 지연을 두고 새 게임을 시작하는 코루틴
    /// </summary>
    private IEnumerator DelayedRestartAfterSkip()
    {
        // 0.1초 대기 (코루틴 정리 시간 확보)
        yield return new WaitForSeconds(0.1f);
        
        Debug.Log($"<color=magenta>[RhythmGameManager]</color> Tab 건너뛰기 후 새 게임 시작!");
        
        // 새로운 레시피로 다시 시작
        StartRhythmSequence();
    }

    private void UpdateRecipeNameUI()
    {
        // 현재 제작 중인 레시피만 표시
        UpdateCurrentRecipeUI();
        // 대기 중인 주문들만 표시
        UpdateOrderQueueUI();
    }

    private void UpdateCurrentRecipeUI()
    {
        if (currentRecipe != null)
        {
            // 레시피 텍스트 업데이트 액션 발행
            Managers.PublishAction(ActionType.UI_UpdateRecipeText);
            Debug.Log($"[RhythmGameManager] 현재 레시피: {currentRecipe.RecipeName}");
        }
        else
        {
            // 레시피 텍스트 업데이트 액션 발행
            Managers.PublishAction(ActionType.UI_UpdateRecipeText);
            Debug.Log("[RhythmGameManager] 현재 제작 중인 레시피 없음");
        }
    }

    private void UpdateOrderQueueUI()
    {
        var allOrders = Managers.Game.CustomerCreator.OrderManager.GetAllOrders();
        Debug.Log($"[RhythmGameManager] 대기 중인 주문 수: {allOrders.Count}");
        
        // 주문 텍스트 업데이트 액션 발행
        Managers.PublishAction(ActionType.UI_UpdateOrderText);
        Debug.Log($"[RhythmGameManager] 주문 UI 업데이트 액션 발행");
    }

    private async void LoadAndSpawnCocktailPrefab(string recipeId)
    {
        // isRestart가 true면 프리팹을 새로 로드하지 않고 기존 프리팹을 그대로 사용
        if (isRestart && currentCocktailPrefab != null)
        {
            // 아무것도 하지 않고 바로 return (기존 프리팹 유지)
            return;
        }

        // 이전 프리팹이 있다면 제거
        if (currentCocktailPrefab != null)
        {
            Destroy(currentCocktailPrefab);
        }

        try
        {
            // Addressables를 통해 프리팹 로드
            var loadOperation = Addressables.LoadAssetAsync<GameObject>($"{recipeId}_prefab");
            // 이렇게 붙여놧어 _prefab을 
            var prefab = await loadOperation.Task;

            if (prefab != null)
            {
                Debug.Log($"<color=green>[LoadAndSpawnCocktailPrefab]</color> 프리팹 로드 성공: {recipeId}");
                
                if (recipeId == "200019" || recipeId == "200010") //고진감래 모히토
                {
                    // recipe가 빈 문자열일 때 y값을 10만큼 올림
                    Vector3 adjustedPosition = cocktailSpawnPoint.position + Vector3.up * 0.535f;
                    currentCocktailPrefab = Instantiate(prefab, adjustedPosition, cocktailSpawnPoint.rotation);
                    Debug.Log($"<color=yellow>[LoadAndSpawnCocktailPrefab]</color> 빈 레시피 - Y 위치 +0.5 조정: {adjustedPosition}"); 
                }
                else if (recipeId == "200012") //섹스온더비치치
                {
                    Vector3 adjustedPosition = cocktailSpawnPoint.position + Vector3.up * 0.15f;
                    currentCocktailPrefab = Instantiate(prefab, adjustedPosition, cocktailSpawnPoint.rotation);
                    Debug.Log($"<color=yellow>[LoadAndSpawnCocktailPrefab]</color> 빈 레시피 - Y 위치 +0.5 조정: {adjustedPosition}"); 
                }
                else
                {
                    currentCocktailPrefab = Instantiate(prefab, cocktailSpawnPoint.position, cocktailSpawnPoint.rotation);
                    Debug.Log($"<color=yellow>[LoadAndSpawnCocktailPrefab]</color> 레시피 생성: {recipeId}"); 
                }
                // 프리팹 생성
                
                

                // 모든 Steps와 Final 오브젝트 비활성화 (Base는 활성화 상태 유지)
                Transform stepsTransform = currentCocktailPrefab.transform.Find("1_Steps");
                Transform finalTransform = currentCocktailPrefab.transform.Find("2_Final");

                if (stepsTransform != null)
                {
                    foreach (Transform child in stepsTransform)
                    {
                        child.gameObject.SetActive(false);
                    }
                    //Debug.Log($"<color=green>[LoadAndSpawnCocktailPrefab]</color> Steps 비활성화 완료");
                }

                if (finalTransform != null)
                {
                    // Final 오브젝트들 저장
                    cocktailFinalObjects = new List<GameObject>();
                    foreach (Transform child in finalTransform)
                    {
                        child.gameObject.SetActive(false);
                        cocktailFinalObjects.Add(child.gameObject);
                    }
                    //Debug.Log($"<color=green>[LoadAndSpawnCocktailPrefab]</color> Final 비활성화 완료");
                }

                // Steps 오브젝트들의 참조 저장 (나중에 순차적 활성화를 위해)
                if (stepsTransform != null)
                {
                    cocktailStepObjects.Clear();
                    foreach (Transform child in stepsTransform)
                    {
                        cocktailStepObjects.Add(child.gameObject);
                    }
                   // Debug.Log($"<color=green>[LoadAndSpawnCocktailPrefab]</color> {cocktailStepObjects.Count}개의 Step 오브젝트 참조 저장됨");
                }

                // 프리팹 레퍼런스 해제
                Addressables.Release(loadOperation);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"<color=red>[LoadAndSpawnCocktailPrefab]</color> 프리팹 로드 실패: {recipeId}\n{e.Message}");
        }
    }

    /// <summary>
    /// 메인 BGM을 재시작합니다.
    /// </summary>
    // private void RestartMainBGM()
    // {
    //     try 
    //     {
    //         AudioClip audioClip = Managers.Resource.Load<AudioClip>("spring-day");
    //         if (audioClip != null)
    //         {
    //             Managers.Sound.Play(Define.ESound.Bgm, audioClip);
    //             Debug.Log("<color=green>[RhythmGameManager]</color> 메인 BGM 재시작: spring-day");
    //         }
    //         else
    //         {
    //             Debug.LogWarning("<color=yellow>[RhythmGameManager]</color> spring-day AudioClip을 찾을 수 없습니다.");
    //         }
    //     }
    //     catch (System.Exception e)
    //     {
    //         Debug.LogError($"<color=red>[RhythmGameManager]</color> BGM 재시작 실패: {e.Message}");
    //     }
    // }

    // ====== 보조/유틸리티 메서드 ======
    private void SetupAudioSources()
    {
        // 메트로놈용 AudioSource 설정
        if (metronomeSource == null)
        {
            // 기존 AudioSource가 있다면 그것을 메트로놈용으로 사용
            metronomeSource = GetComponent<AudioSource>();
            
            // 없다면 새로 생성
            if (metronomeSource == null)
            {
                metronomeSource = gameObject.AddComponent<AudioSource>();
            }
            metronomeSource.playOnAwake = false;
        }

        // 키 사운드용 AudioSource 생성
        if (audioSource == null)
        {
            // 새로운 AudioSource 컴포넌트 추가
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        Debug.Log("Audio Sources setup completed - Metronome: " + (metronomeSource != null) + ", KeySound: " + (audioSource != null));
    }

    private void InitKeySoundDict()
    {
        keySoundDict = new Dictionary<string, AudioClip>
        {
            { "A", soundA },
            { "S", soundS },
            { "D", soundD },
            { "W", soundW },
            { "Z", soundZ }
        };
    }

    private void InitKeySpriteDict() // Initialize dictionary for key sprites
    {
        if (keySpriteDict != null) return;

        keySpriteDict = new Dictionary<string, Sprite>();
        for (int i = 0; i < keyLabels.Count; i++)
        {
            keySpriteDict[keyLabels[i]] = keySprites[i];
        }
    }

    public void ShowKeyCombinationUI(List<string> keys) //show key combination ui
    {
        InitKeySpriteDict();

        for (int i = 0; i < keyImages.Count; i++)
        {
            if (i < keys.Count)
            {
                string key = keys[i].ToUpper();

                if (keySpriteDict.ContainsKey(key))
                {
                    keyImages[i].sprite = keySpriteDict[key];
                    keyImages[i].gameObject.SetActive(true);
                }
                else
                {
                    keyImages[i].gameObject.SetActive(false);
                }
            }
        }
    }

    private IEnumerator WaitASecond() // Wait for 1 second
    {
        yield return new WaitForSeconds(1f);
    }

    private void UpdateRecipeTempo()
    {
        if (currentRecipe != null)
        {
            // BPM을 interval로 변환 (60초 / BPM = 한 비트당 시간)
            interval = 60f / currentRecipe.BPM;
            Debug.Log($"레시피 {currentRecipe.RecipeName}의 BPM: {currentRecipe.BPM}, interval: {interval}");
        }
    }

    private AudioClip GetRecipeSound(string key)
    {
        if (currentRecipe == null)
            return keySoundDict[key];

        var soundSet = recipeSoundData.GetSoundSet(currentRecipe.NO);
        if (soundSet == null)
            return keySoundDict[key];

        switch (key)
        {
            case "A": return soundSet.soundA ?? keySoundDict[key];
            case "S": return soundSet.soundS ?? keySoundDict[key];
            case "D": return soundSet.soundD ?? keySoundDict[key];
            case "W": return soundSet.soundW ?? keySoundDict[key];
            case "Z": return soundSet.soundZ ?? keySoundDict[key];
            default: return keySoundDict[key];
        }
    }

    private void DimKeyUI(int index) // Dim the UI of pressed key
    {
        if (index >= 0 && index < keyImages.Count)
        {
            Image image = keyImages[index];
            Color color = image.color;
            color = new Color(color.r * 0.5f, color.g * 0.5f, color.b * 0.5f, 0.3f);
            image.color = color;
        }
    }

    private void RestoreKeyUI() // Restore UI brightness to original state
    {
        foreach (Image image in keyImages)
        {
            Color color = image.color;
            color = new Color(1f, 1f, 1f, 1f);
            image.color = color;
        }
    }

    private IEnumerator AnimateKeyScale(int keyIndex)
    {
        if (keyIndex < 0 || keyIndex >= keyImages.Count) yield break;
        
        Image keyImage = keyImages[keyIndex];
        Vector3 originalScale = keyImage.transform.localScale;
        Vector3 targetScale = originalScale * scaleMultiplier;
        
        // 크기 증가
        keyImage.transform.localScale = targetScale;
        
        // 지정된 시간 후 원래 크기로 복귀
        yield return new WaitForSeconds(scaleDuration);
        
        keyImage.transform.localScale = originalScale;

    }

    private string SortString(string input)
    {
        return new string(input.ToCharArray().OrderBy(c => c).ToArray());
    }


    public void ESCPressed() //정리해야됨
    {
                // 게임이 진행 중일 때 ESC를 누른 경우
        if (!isRhythmGameEnded)
        {
            Debug.Log("<color=orange>[RhythmGameManager]</color> ESC를 눌러 리듬 게임을 강제 중단합니다.");

            StopAllCoroutines();
            isRhythmGameEnded = true; 

            Managers.Ingame.EndRhythmGame(RhythmResult.Pause);
        }

        RestoreKeyUI();
        KeyUI.SetActive(false);
        sampleImage.gameObject.SetActive(false);
        badResultImage.SetActive(false);

        ClearCocktailPrefab();

        Managers.Ingame.isRhythmGameStarted = false;

    }

    public void ClearCocktailPrefab()
    {
        if (currentCocktailPrefab != null)
        {
            Destroy(currentCocktailPrefab);
            currentCocktailPrefab = null;
        }
    }

}
