using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;

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

    private bool useMetronome = true;

    [Header("Rhythm Game UI")]
    public TextMeshProUGUI recipeName;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI orderText;

    // Coroutine references
    private Coroutine rhythmCoroutine;
    private Coroutine inputCoroutine;
    private Coroutine metronomeCoroutine;

    // Current recipe being played
    private Data.RecipeData currentRecipe;

    private Dictionary<string, AudioClip> keySoundDict;

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
        WaitASecond();

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
                // 폴백으로 랜덤 레시피 사용
                currentRecipe = Managers.Ingame.getRandomRecipe();
                UpdateRecipeTempo();  // BPM에 따라 interval 업데이트
            }
        }
        else
        {
            Debug.LogWarning($"<color=yellow>[RhythmGameManager]</color> 대기 중인 주문이 없습니다. 랜덤 레시피 사용.");
            // 주문이 없으면 랜덤 레시피 사용
            currentRecipe = Managers.Ingame.getRandomRecipe();
            UpdateRecipeTempo();  // BPM에 따라 interval 업데이트
        }

        Debug.Log(currentRecipe.RecipeName);
        Debug.Log(string.Join(", ", currentRecipe.KeyCombination));

        rhythmPattern = new List<string>(currentRecipe.KeyCombination);
        Debug.Log("rhythmPattern:" + string.Join(", ", rhythmPattern));

        // 현재 레시피와 대기 중인 주문들을 UI에 표시
        UpdateRecipeNameUI();

        ShowKeyCombinationUI(currentRecipe.KeyCombination);
        TrimTrailingSilence();
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
        metronomeSource.volume = 0.7f;

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

            if (rhythmPattern[i] != "_")
            {
                // 키 입력이 있는 경우 해당 인덱스의 칵테일 이미지 활성화
                if (i < cocktailStepObjects.Count && cocktailStepObjects[i] != null)
                {
                    cocktailStepObjects[i].SetActive(true);
                }

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

        foreach (string c in countdown)
        {
            if (Num != null) Num.text = c;
            yield return new WaitForSeconds(interval);
        }
        metronomeSource.volume = 0.15f; //플레이어 입력 시 메트로놈 볼륨 낮춤
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

    private IEnumerator WaitForInputs()
    {
        float inputStartTime = Time.time;

        for (int i = 0; i < rhythmPattern.Count; i++)
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

                    foreach (KeyCode k in System.Enum.GetValues(typeof(KeyCode)))
                    {
                        if (Input.GetKey(k))
                        {
                            string key = (k == KeyCode.Space) ? "Z" : k.ToString().ToUpper();
                            keysPressed.Add(key);
                            
                            AudioClip soundToPlay = GetRecipeSound(key);
                            if (soundToPlay != null)
                            {
                                audioSource.PlayOneShot(soundToPlay);  // 키 사운드는 기존 AudioSource 사용
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
    }

    public void ForceStopAndFail()
    {
        if (rhythmCoroutine != null) StopCoroutine(rhythmCoroutine);
        if (inputCoroutine != null) StopCoroutine(inputCoroutine);
        if (metronomeCoroutine != null) StopCoroutine(metronomeCoroutine);

        useMetronome = false;
        StartCoroutine(HandleGameEnd(RhythmResult.Fail));
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
            
            StartRhythmSequence();  // Restart with same order
        }
        else
        {
            resultText.text = "Clear";
            resultText.color = Color.blue;
            
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
            while (!Input.GetKeyDown(KeyCode.Space))
            {
                yield return null;
            }

            // Prepare for next sequence
            RestoreKeyUI();
            KeyUI.SetActive(true);
            resultText.gameObject.SetActive(false);
            
            // Send result to game manager
            Managers.Ingame.EndRhythmGame(result);
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
            recipeName.text = $"🔥 제작 중: {currentRecipe.RecipeName}";
            Debug.Log($"[RhythmGameManager] 현재 레시피: {currentRecipe.RecipeName}");
        }
        else
        {
            recipeName.text = "🔥 제작 중: 없음";
            Debug.Log("[RhythmGameManager] 현재 제작 중인 레시피 없음");
        }
    }

    private void UpdateOrderQueueUI()
    {
        var allOrders = Managers.Game.CustomerCreator.OrderManager.GetAllOrders();
        Debug.Log($"[RhythmGameManager] 대기 중인 주문 수: {allOrders.Count}");
        
        string orderDisplayText = "";
        
        if (allOrders.Count > 0)
        {
            orderDisplayText = $"📋 대기 주문 ({allOrders.Count}개):\n";
            for (int i = 0; i < allOrders.Count; i++)
            {
                orderDisplayText += $"{i + 1}. {allOrders[i].RecipeName} x{allOrders[i].Quantity}\n";
                Debug.Log($"[RhythmGameManager] 주문 {i+1}: {allOrders[i].RecipeName} x{allOrders[i].Quantity}");
            }
        }
        else
        {
            orderDisplayText = "📋 대기 주문: 없음";
            Debug.Log("[RhythmGameManager] 대기 중인 주문이 없습니다.");
        }
        
        orderText.text = orderDisplayText.TrimEnd('\n');
        Debug.Log($"[RhythmGameManager] 주문 UI 업데이트 완료: {orderText.text}");
    }

}
