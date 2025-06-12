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
    Perfect
}

public class RhythmGameManager : MonoBehaviour
{
    // Sound related variables
    [Header("Sound Setting")]
    public AudioClip soundClip;
    public AudioSource audioSource;  // 키 사운드용 (기존 AudioSource)

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

    [Header("Countdown TMP Text")]
    public TextMeshProUGUI Num;

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
    private Coroutine rhythmCoroutine;
    private Coroutine inputCoroutine;
    private Coroutine metronomeCoroutine;

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

    private void Start()
    {
        KeyUI.SetActive(false);
        resultText.gameObject.SetActive(false);
        InitKeySoundDict();
    }

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

    public void StartRhythmSequence()
    {
        // 리듬게임 시작 시 메인 BGM 정지
        Managers.Sound.Stop(Define.ESound.Bgm);
        Debug.Log("<color=yellow>[RhythmGameManager]</color> 메인 BGM 정지");
        
        WaitASecond();

        // 현재 레시피가 없을 때만 새로운 레시피를 가져옴
        if (currentRecipe == null)
        {
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
                else
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
        else
        {
            Debug.Log($"<color=green>[RhythmGameManager]</color> 실패한 레시피 재시도: {currentRecipe.RecipeName}");
        }

        Debug.Log(currentRecipe.RecipeName);
        LoadAndSpawnCocktailPrefab(currentRecipe.NO.ToString());
        Debug.Log(string.Join(", ", currentRecipe.KeyCombination));

        rhythmPattern = new List<string>(currentRecipe.KeyCombination);
        Debug.Log("rhythmPattern:" + string.Join(", ", rhythmPattern));

        // 현재 레시피와 대기 중인 주문들을 UI에 표시
        UpdateRecipeNameUI();

        ShowKeyCombinationUI(currentRecipe.KeyCombination);
        //TrimTrailingSilence();
        rhythmCoroutine = StartCoroutine(InitAndStart());
        KeyUI.SetActive(true);
    }

    private void TrimTrailingSilence()
    {
        for (int i = rhythmPattern.Count - 1; i >= 0; i--)
        {
            if (rhythmPattern[i] != "_")
            {
                rhythmPattern = rhythmPattern.GetRange(0, i + 1);
                break;
            }
        }
    }

    private IEnumerator InitAndStart()
    {
        yield return new WaitForSeconds(1f);
        useMetronome = true;
        metronomeCoroutine = StartCoroutine(MetronomeLoop());
        rhythmCoroutine = StartCoroutine(PlayRhythm());
    }

    private IEnumerator MetronomeLoop()
    {
        while (useMetronome)
        {
            if (metronomeClip != null && metronomeSource != null)
                metronomeSource.PlayOneShot(metronomeClip);

            yield return new WaitForSeconds(interval);
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

    private IEnumerator PlayRhythm()
    {
        metronomeSource.volume = 0f;

        expectedTimes.Clear();
        inputTimes.Clear();
        inputKeys.Clear();

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

            yield return new WaitForSeconds(interval);
        }

        RestoreKeyUI();
        yield return StartCoroutine(PlayCountdownBeats());
        inputCoroutine = StartCoroutine(WaitForInputs());
    }

    private IEnumerator PlayCountdownBeats()
    {
        string[] countdown = { "3", "2", "1" };
        metronomeSource.volume = 0.7f;
        sampleImage.gameObject.SetActive(true);
        foreach (string c in countdown)
        {
            if (Num != null) Num.text = c;
            yield return new WaitForSeconds(interval);
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
        metronomeSource.volume = 0f; //플레이어 입력 시 메트로놈 볼륨 낮춤
        sampleImage.gameObject.SetActive(false);
        if (Num != null) Num.text = "";
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

    private IEnumerator WaitForInputs()
    {
        
        float inputStartTime = Time.time;
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
            float expectedTime = inputStartTime + i * interval;
            expectedTimes.Add(expectedTime);

            float slotEnd = expectedTime + interval;
            bool inputReceived = false;
            float recordedTime = -1f;
            string pressedKeyString = "";

            while (Time.time < slotEnd)
            {
                if (!inputReceived)
                {
                    List<string> keysPressed = new List<string>();

                    foreach (KeyCode k in allowedKeys)
                    {
                        if (Input.GetKey(k))
                        {
                            string key = (k == KeyCode.Space) ? "Z" : k.ToString().ToUpper();
                            keysPressed.Add(key);
                            
                            AudioClip soundToPlay = GetRecipeSound(key);
                            if (soundToPlay != null)
                            {
                                audioSource.PlayOneShot(soundToPlay);
                            }
                        }
                    }

                    if (keysPressed.Count > 0)
                    {
                        keysPressed.Sort();
                        pressedKeyString = string.Join("", keysPressed);
                        inputReceived = true;
                        recordedTime = Time.time;
                    }

                    // 입력 패턴이 맞는지 확인하고 흐리게 처리
                    bool isCorrectInput = false;
                    if (rhythmPattern[i] == "_") //none input
                    {
                        if (!inputReceived)
                        {
                            isCorrectInput = true;
                            recordedTime = Time.time;
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

    private string SortString(string input)
    {
        return new string(input.ToCharArray().OrderBy(c => c).ToArray());
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
    }

    public void ForceStopAndFail()
    {
        if (rhythmCoroutine != null) StopCoroutine(rhythmCoroutine);
        if (inputCoroutine != null) StopCoroutine(inputCoroutine);
        if (metronomeCoroutine != null) StopCoroutine(metronomeCoroutine);

        useMetronome = false;
        StartCoroutine(HandleGameEnd(RhythmResult.Fail));
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
            
            // 현재 레시피 초기화
            currentRecipe = null;
            
            // 실행 중인 코루틴들 정지
            if (rhythmCoroutine != null) StopCoroutine(rhythmCoroutine);
            if (inputCoroutine != null) StopCoroutine(inputCoroutine);
            if (metronomeCoroutine != null) StopCoroutine(metronomeCoroutine);
            
            useMetronome = false;
            
            // UI 초기화
            RestoreKeyUI();
            if (Num != null) Num.text = "";
            
            // 칵테일 오브젝트 정리
            if (currentCocktailPrefab != null)
            {
                Destroy(currentCocktailPrefab);
                currentCocktailPrefab = null;
            }
            
            // 새로운 레시피로 다시 시작
            StartRhythmSequence();
        }
        else
        {
            Debug.Log($"<color=yellow>[RhythmGameManager]</color> 건너뛸 수 있는 주문이 없습니다.");
        }
    }

    private IEnumerator HandleGameEnd(RhythmResult result) // Handle game end state and result display
    {
        yield return WaitASecond();
        KeyUI.SetActive(false);
        
        // Show result text with color
        resultText.gameObject.SetActive(true);
        if (result == RhythmResult.Fail)
        {
            resultText.text = "Bad";
            resultText.color = Color.red;
            
            // 실패 시 주문은 그대로 유지 (다시 시도)
            yield return WaitASecond();  // Show result text briefly
            RestoreKeyUI();
            resultText.gameObject.SetActive(false);
            
            // UI 업데이트 (주문은 그대로, 현재 레시피도 유지)
            UpdateRecipeNameUI();
            
            Debug.Log($"<color=red>[RhythmGameManager]</color> 레시피 {currentRecipe.RecipeName} 실패, 재시도합니다.");
            StartRhythmSequence();  // Restart with same order
            /////////////////////////////////////////////////////동현이가 일단추가
            Managers.Ingame.EndRhythmGame(result);
        }
        else
        {
            resultText.text = "Clear";
            resultText.color = Color.blue;
            
            // 성공 시 Final 오브젝트 활성화
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
            
            // 성공 시에만 주문 제거
            var completedOrder = Managers.Game.CustomerCreator.OrderManager.GetNextOrder();
            if (completedOrder != null)
            {
                Debug.Log($"<color=green>[RhythmGameManager]</color> 주문 완료: {completedOrder.RecipeName}");
                
            
            }
            
            currentRecipe = null;  // Clear current recipe on success only
            
            // UI 업데이트 (주문 제거됨, 현재 레시피 클리어)
            UpdateRecipeNameUI();
            
            // Wait for space input only on success
            //while (!Input.GetKeyDown(KeyCode.Space))
            //{
            //    yield return null;
            //}

            // Prepare for next sequence
            RestoreKeyUI();
            KeyUI.SetActive(true);
            resultText.gameObject.SetActive(false);
            
            // Send result to game manager
            Managers.Ingame.EndRhythmGame(result);
            
            // 리듬게임 완료 시 메인 BGM 재시작
            RestartMainBGM();
        }
    }

    /// <summary>
    /// 메인 BGM을 재시작합니다.
    /// </summary>
    private void RestartMainBGM()
    {
        try 
        {
            AudioClip audioClip = Managers.Resource.Load<AudioClip>("spring-day");
            if (audioClip != null)
            {
                Managers.Sound.Play(Define.ESound.Bgm, audioClip);
                Debug.Log("<color=green>[RhythmGameManager]</color> 메인 BGM 재시작: spring-day");
            }
            else
            {
                Debug.LogWarning("<color=yellow>[RhythmGameManager]</color> spring-day AudioClip을 찾을 수 없습니다.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"<color=red>[RhythmGameManager]</color> BGM 재시작 실패: {e.Message}");
        }
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
                
                // 프리팹 생성
                currentCocktailPrefab = Instantiate(prefab, cocktailSpawnPoint.position, cocktailSpawnPoint.rotation);
                
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


}
